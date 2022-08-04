using System.Linq.Expressions;
using System.Reflection;
using HotChocolate.Configuration;
using HotChocolate.Data.Filters;
using HotChocolate.Data.Filters.Expressions;
using HotChocolate.Language;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using HotChocolate.Types.Descriptors;
using HotChocolate.Types.Descriptors.Definitions;
using NetTopologySuite.Operation.Buffer.Validate;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Queries.Spatial;
using Raven.Client.Documents.Session;

namespace HotChocolate.Raven.Spatial.Filtering;

public static class UseRavenSpatialFilteringAttributeExtensions
{
    public static IObjectFieldDescriptor UseRavenSpatialFiltering(this IObjectFieldDescriptor descriptor, string? scope = null)
    {
        descriptor
           .Extend()
           .OnBeforeNaming(
                (c, definition) =>
                {
                    var convention = c.GetFilterConvention(scope);

                    if (definition.ResultType is not { IsGenericType: true }
                     || definition.ResultType.GetGenericTypeDefinition() == typeof(IRavenQueryable<>)
                     || !c.TypeInspector.TryCreateTypeInfo(definition.ResultType, out var typeInfo)
                       )
                    {
                        throw new ArgumentException(
                            "FilterObjectFieldDescriptorExtensions_UseFiltering_CannotHandleType",
                            nameof(descriptor)
                        );
                    }

                    var argumentTypeReference = convention.GetFieldType(typeInfo.NamedType);

                    definition.Configurations.Add(
                        new CompleteConfiguration<ObjectFieldDefinition>(
                            (ctx, d) =>
                                CompileMiddleware(ctx, d, argumentTypeReference, scope),
                            definition,
                            ApplyConfigurationOn.Completion,
                            argumentTypeReference,
                            TypeDependencyKind.Completed
                        )
                    );
                }
            );
        return descriptor;
    }

    private static RavenFilterVisitor Visitor { get; } = new(new QueryableCombinator());

    private static readonly MethodInfo _factoryTemplate =
        typeof(UseRavenSpatialFilteringAttributeExtensions)
           .GetMethod(nameof(CreateExecutor), BindingFlags.Static | BindingFlags.NonPublic)!;

    private static void CompileMiddleware(
        ITypeCompletionContext context,
        ObjectFieldDefinition definition,
        ITypeReference argumentTypeReference,
        string? scope
    )
    {
        var type = context.GetType<IFilterInputType>(argumentTypeReference);
        var convention = context.DescriptorContext.GetFilterConvention(scope);

        var factory = _factoryTemplate.MakeGenericMethod(type.EntityType.Source);
        var middleware = (FieldMiddleware)factory.Invoke(null, new object[] { convention.GetArgumentName() })!;

        var filteringMiddleware = definition.MiddlewareDefinitions.SingleOrDefault(z => z.Key == WellKnownMiddleware.Filtering);
        if (filteringMiddleware is null)
        {
            throw new NotSupportedException("Filtering middleware is required");
        }

        var index = definition.MiddlewareDefinitions.IndexOf(filteringMiddleware);
        definition.MiddlewareDefinitions.Insert(index + 1, new(middleware, key: "HotChocolate.Raven.Spatial.Filtering"));
    }


    private static ApplyFiltering CreateApplicatorAsync<TEntityType>(NameString argumentName)
    {
        return (context, input) =>
        {
            // next we get the filter argument. If the filter argument is already on the context
            // we use this. This enabled overriding the context with LocalContextData
            var argument = context.Selection.Field.Arguments[argumentName];
            var filter = context.LocalContextData.ContainsKey(QueryableFilterProvider.ContextValueNodeKey) &&
                         context.LocalContextData[QueryableFilterProvider.ContextValueNodeKey] is IValueNode node
                ? node
                : context.ArgumentLiteral<IValueNode>(argumentName);

            if (filter.IsNull())
            {
                return input;
            }


            if (argument.Type is IFilterInputType filterInput)
            {
                var visitorContext = VisitFilterArgumentExecutor(filter, filterInput, false);

                if (filter is ObjectValueNode { Fields.Count: 0 })
                {
                    context.LocalContextData = context.LocalContextData.SetItem(QueryableFilterProvider.SkipFilteringKey, true);
                }

                if (visitorContext is
                    {
                        SpatialExpression: { } spatialExpression,
                        SpatialFieldName: { } spatialFieldName,
                        FieldName: { } fieldName
                    }
                 && input is IRavenQueryable<TEntityType> ravenQueryable and IRavenQueryInspector ravenQueryInspector)
                {
                    return ravenQueryInspector.IndexName == null
                        ? // dynamic query
                        ravenQueryable.Spatial(x => spatialFieldName, spatialExpression.Compile())
                        : // index query
                        ravenQueryable.Spatial(fieldName, spatialExpression.Compile());
                }
            }

            if (filter is ObjectValueNode { Fields.Count: 0 })
            {
                context.LocalContextData = context.LocalContextData.SetItem(QueryableFilterProvider.SkipFilteringKey, true);
            }

            return input;
        };
    }

    private static RavenQueryableFilterContext VisitFilterArgumentExecutor(
        IValueNode valueNode,
        IFilterInputType filterInput,
        bool inMemory
    )
    {
        var visitorContext = new RavenQueryableFilterContext(filterInput, inMemory);

        // rewrite GraphQL input object into expression tree.
        Visitor.Visit(valueNode, visitorContext);

        return visitorContext;
    }

    private static FieldMiddleware CreateExecutor<TEntityType>(NameString argumentName)
    {
        var applyFilter = CreateApplicatorAsync<TEntityType>(argumentName);

        return next => context => ExecuteAsync(next, context);

        async ValueTask ExecuteAsync(FieldDelegate next, IMiddlewareContext context)
        {
            // first we let the pipeline run and produce a result.
            await next(context).ConfigureAwait(false);

            context.Result = applyFilter(context, context.Result);
        }
    }
}
