using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using HotChocolate.Configuration;
using HotChocolate.Data.Filters;
using HotChocolate.Data.Filters.Expressions;
using HotChocolate.Language;
using HotChocolate.Language.Visitors;
using HotChocolate.Raven.Spatial.Filtering.Convention;
using HotChocolate.Types;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Raven.Client.Documents.Queries.Spatial;

namespace HotChocolate.Raven.Spatial.Filtering.Expressions.Handlers;

public abstract class RavenQueryableSpatialOperationHandlerBase : FilterFieldHandler<QueryableFilterContext, Expression>
{
    private readonly int _operation;
    private readonly NameString _geometryFieldName;
    private readonly NameString _locationFieldName;
    private readonly NameString _bufferFieldName;
    private readonly NameString _wktFieldName;
    private readonly InputParser _inputParser;

    public RavenQueryableSpatialOperationHandlerBase(
        IFilterConvention convention,
        InputParser inputParser,
        int operation
    )
    {
        _operation = operation;
        _inputParser = inputParser;
        _geometryFieldName = convention.GetOperationName(RavenSpatialFilterOperations.Geometry);
        _bufferFieldName = convention.GetOperationName(RavenSpatialFilterOperations.Buffer);
        _wktFieldName = convention.GetOperationName(RavenSpatialFilterOperations.Wkt);
        _locationFieldName = convention.GetOperationName(RavenSpatialFilterOperations.Location);
    }

    public override bool CanHandle(
        ITypeCompletionContext context,
        IFilterInputTypeDefinition typeDefinition,
        IFilterFieldDefinition fieldDefinition
    )
    {
        return fieldDefinition is FilterOperationFieldDefinition op && op.Id == _operation;
    }

    public override bool TryHandleEnter(
        QueryableFilterContext context,
        IFilterField field,
        ObjectFieldNode node,
        [NotNullWhen(true)] out ISyntaxVisitorAction? action
    )
    {
        if (field is IFilterOperationField filterOperationField)
        {
            if (node.Value.IsNull())
            {
                context.ReportError(ErrorHelper.CreateNonNullError(field, node.Value, context));
                action = SyntaxVisitor.Skip;
                return true;
            }

            if (!TryHandleOperation(context, filterOperationField, node, out var nestedProperty))
            {
                context.PushInstance(Expression.Constant(true));
                action = SyntaxVisitor.SkipAndLeave;
                return true;
            }

            context.PushInstance(nestedProperty);
            action = SyntaxVisitor.SkipAndLeave;
        }
        else
        {
            action = SyntaxVisitor.Break;
        }

        return true;
    }

    public override bool TryHandleLeave(
        QueryableFilterContext context,
        IFilterField field,
        ObjectFieldNode node,
        [NotNullWhen(true)] out ISyntaxVisitorAction? action
    )
    {
        // Dequeue last
        var condition = context.PopInstance();

        context.GetLevel().Enqueue(condition);
        action = SyntaxVisitor.Continue;
        return true;
    }

    protected bool TryGetParameter<T>(
        IFilterField parentField,
        IValueNode node,
        string fieldName,
        [NotNullWhen(true)] out T fieldNode
    )
    {
        return RavenSpatialOperationHandlerHelper.TryGetParameter(
            parentField,
            node,
            fieldName,
            _inputParser,
            out fieldNode
        );
    }

    protected bool TryHandleOperation(
        QueryableFilterContext context,
        IFilterOperationField field,
        ObjectFieldNode node,
        [NotNullWhen(true)] out Expression? result
    )
    {
        if (context is not RavenQueryableFilterContext ravenQueryableFilterContext)
        {
            result = null;
            return false;
        }

        string? wkt = null;
        double? buffer = null;

        if (TryGetParameter(field, node.Value, _bufferFieldName, out double rawBuffer))
        {
            buffer = rawBuffer;
        }

        if (TryGetParameter(field, node.Value, _wktFieldName, out wkt))
        {
        }

        if (wkt == null && TryGetParameter(field, node.Value, _locationFieldName, out LatLong ll))
        {
            wkt = ( (Point)ll ).AsText();
        }

        if (wkt == null && TryGetParameter(field, node.Value, _geometryFieldName, out Geometry g))
        {
            wkt = g.AsText();
        }

        if (!string.IsNullOrWhiteSpace(wkt) && context.GetScope().Instance.Peek() is MemberExpression memberExpression)
        {
            ravenQueryableFilterContext.SpatialFieldName =
                memberExpression.Member switch
                {
                    PropertyInfo pi => pi.PropertyType == typeof(LatLong)
                        ? new PointField(pi.Name + "." + nameof(LatLong.Latitude), pi.Name + "." + nameof(LatLong.Longitude))
                        : new WktField(pi.Name),
                    _ => ravenQueryableFilterContext.SpatialFieldName
                };
            ravenQueryableFilterContext.FieldName = memberExpression.Member.Name;
            ravenQueryableFilterContext.SpatialExpression = GetSpatialExpression(wkt, buffer);
            ravenQueryableFilterContext.NodeToRemove = node;
        }

        result = null;
        return false;
    }

    protected abstract Expression<Func<SpatialCriteriaFactory, SpatialCriteria>> GetSpatialExpression(string wkt, double? buffer);
}
