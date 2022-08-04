using System.Reflection;
using HotChocolate.Configuration;
using HotChocolate.Data.Sorting;
using HotChocolate.Data.Sorting.Expressions;
using HotChocolate.Language;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using HotChocolate.Types.Descriptors;
using HotChocolate.Types.Descriptors.Definitions;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Queries.Spatial;
using Raven.Client.Documents.Session;

namespace HotChocolate.Raven.Spatial.Sorting;

public static class UseRavenSpatialSortingAttributeExtensions
{
    public static IObjectFieldDescriptor UseRavenSpatialSorting(this IObjectFieldDescriptor descriptor, string? scope = null)
    {
        descriptor
           .Extend()
           .OnBeforeNaming(
                (c, definition) =>
                {
                    var convention = c.GetSortConvention(scope);

                    if (definition.ResultType is not { IsGenericType: true }
                     || definition.ResultType.GetGenericTypeDefinition() == typeof(IRavenQueryable<>)
                     || !c.TypeInspector.TryCreateTypeInfo(definition.ResultType, out var typeInfo)
                       )
                    {
                        throw new ArgumentException(
                            "SortObjectFieldDescriptorExtensions_UseSorting_CannotHandleType",
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

    private static SortVisitor<QueryableSortContext, QueryableSortOperation> Visitor { get; } = new();

    private static readonly MethodInfo _factoryTemplate =
        typeof(UseRavenSpatialSortingAttributeExtensions)
           .GetMethod(nameof(CreateExecutor), BindingFlags.Static | BindingFlags.NonPublic)!;

    private static void CompileMiddleware(
        ITypeCompletionContext context,
        ObjectFieldDefinition definition,
        ITypeReference argumentTypeReference,
        string? scope
    )
    {
        var type = context.GetType<ISortInputType>(argumentTypeReference);
        var convention = context.DescriptorContext.GetSortConvention(scope);

        var factory = _factoryTemplate.MakeGenericMethod(type.EntityType.Source);
        var middleware = (FieldMiddleware)factory.Invoke(null, new object[] { convention.GetArgumentName() })!;

        var sortingMiddleware = definition.MiddlewareDefinitions.SingleOrDefault(z => z.Key == WellKnownMiddleware.Sorting);
        if (sortingMiddleware is null)
        {
            throw new NotSupportedException("Sorting middleware is required");
        }

        var index = definition.MiddlewareDefinitions.IndexOf(sortingMiddleware);
        definition.MiddlewareDefinitions.Insert(index, new(middleware, key: "HotChocolate.Raven.Spatial.Sorting"));
    }


    private static ApplySorting CreateApplicatorAsync<TEntityType>(NameString argumentName)
    {
        return (context, input) =>
        {
            // next we get the Sort argument. If the Sort argument is already on the context
            // we use this. This enabled overriding the context with LocalContextData
            var argument = context.Selection.Field.Arguments[argumentName];
            var sort = context.LocalContextData.ContainsKey(QueryableSortProvider.ContextArgumentNameKey) &&
                       context.LocalContextData[QueryableSortProvider.ContextArgumentNameKey] is IValueNode node
                ? node
                : context.ArgumentLiteral<IValueNode>(argumentName);

            if (sort.IsNull())
            {
                return input;
            }

            if (argument.Type is ListType lt &&
                lt.ElementType is NonNullType nn &&
                nn.NamedType() is ISortInputType sortInput)
            {
                var visitorContext = VisitSortArgumentExecutor(sort, sortInput, false);
                if (visitorContext is
                    {
                        SortDirection: not RavenSortDirection.None,
                        SpatialFieldName: { } SpatialFieldName,
                        SpatialWkt: { } SpatialWkt,
                        FieldName: { } FieldName
                    }
                 && input is IRavenQueryable<TEntityType> ravenQueryable and IRavenQueryInspector ravenQueryInspector)
                {
                    return ravenQueryInspector.IndexName == null
                        ? visitorContext.SortDirection == RavenSortDirection.Asc
                            ? ravenQueryable.OrderByDistance(SpatialFieldName.ToField((s, b) => s), SpatialWkt)
                            : ravenQueryable.OrderByDistanceDescending(SpatialFieldName.ToField((s, b) => s), SpatialWkt)
                        : visitorContext.SortDirection == RavenSortDirection.Asc
                            ? ravenQueryable.OrderByDistance(FieldName, SpatialWkt)
                            : ravenQueryable.OrderByDistanceDescending(FieldName, SpatialWkt);
                }
            }

            return input;
        };
    }

    private static RavenQueryableSortContext VisitSortArgumentExecutor(
        IValueNode valueNode, ISortInputType sortInput, bool inMemory
    )
    {
        var visitorContext = new RavenQueryableSortContext(sortInput, inMemory);

        // rewrite GraphQL input object into expression tree.
        Visitor.Visit(valueNode, visitorContext);

        return visitorContext;
    }

    private static FieldMiddleware CreateExecutor<TEntityType>(NameString argumentName)
    {
        var applySort = CreateApplicatorAsync<TEntityType>(argumentName);

        return next => context => executeAsync(next, context);

        async ValueTask executeAsync(FieldDelegate next, IMiddlewareContext context)
        {
            // first we let the pipeline run and produce a result.
            await next(context).ConfigureAwait(false);

            context.Result = applySort(context, context.Result);
        }
    }
}
