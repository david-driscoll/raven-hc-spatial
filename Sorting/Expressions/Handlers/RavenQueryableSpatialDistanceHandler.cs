using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using HotChocolate.Configuration;
using HotChocolate.Data.Sorting;
using HotChocolate.Data.Sorting.Expressions;
using HotChocolate.Language;
using HotChocolate.Language.Visitors;
using HotChocolate.Raven.Spatial.Sorting.Convention;
using HotChocolate.Types;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Raven.Client.Documents.Queries.Spatial;

namespace HotChocolate.Raven.Spatial.Sorting.Expressions.Handlers;

public class RavenQueryableSpatialDistanceHandler : SortFieldHandler<QueryableSortContext, QueryableSortOperation>
{
    private readonly NameString _geometryFieldName;
    private readonly NameString _wktFieldName;
    private readonly InputParser _inputParser;
    private readonly WKTReader _wktReader;
    private readonly NameString _locationFieldName;

    public RavenQueryableSpatialDistanceHandler(InputParser inputParser, NtsGeometryServices? geometryServices)
    {
        _inputParser = inputParser;
        _geometryFieldName = "geometry";
        _wktFieldName = "wkt";
        _locationFieldName = "location";
        _wktReader = new WKTReader(geometryServices ?? NtsGeometryServices.Instance);
    }

    public override bool CanHandle(ITypeCompletionContext context, ISortInputTypeDefinition typeDefinition, ISortFieldDefinition fieldDefinition)
    {
        return fieldDefinition.Member switch
        {
            PropertyInfo pi => pi.PropertyType.IsAssignableTo(typeof(Geometry)) || pi.PropertyType == typeof(LatLong),
            _               => false
        };
    }

    public override bool TryHandleEnter(
        QueryableSortContext context, ISortField field, ObjectFieldNode node, [NotNullWhen(true)] out ISyntaxVisitorAction? action
    )
    {
        if (node.Value.IsNull())
        {
            context.ReportError(ErrorHelper.CreateNonNullError(field, node.Value, context));
            action = SyntaxVisitor.Skip;
            return true;
        }

        if (context is RavenQueryableSortContext ravenQueryableSortContext)
        {
            string? wkt = null;
            var direction = "ASC";
            double? buffer = null;

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

            if (TryGetParameter(field, node.Value, "direction", out string d))
            {
                direction = d;
            }

            if (!string.IsNullOrWhiteSpace(wkt))
            {
                ravenQueryableSortContext.SpatialFieldName =
                    field.Member switch
                    {
                        PropertyInfo pi => pi.PropertyType == typeof(LatLong)
                            ? new PointField(pi.Name + "." + nameof(LatLong.Latitude), pi.Name + "." + nameof(LatLong.Longitude))
                            : new WktField(pi.Name),
                        _ => ravenQueryableSortContext.SpatialFieldName
                    };
                ravenQueryableSortContext.SortDirection = direction == "ASC" ? RavenSortDirection.Asc : RavenSortDirection.Desc;
                ravenQueryableSortContext.SpatialWkt = wkt;
                ravenQueryableSortContext.FieldName = field.Member!.Name;
            }
        }

//            context.PushInstance(context.GetInstance());
//            context.RuntimeTypes.Push(field.RuntimeType);
        action = SyntaxVisitor.SkipAndLeave;

        return true;
    }

    public override bool TryHandleLeave(
        QueryableSortContext context, ISortField field, ObjectFieldNode node, [NotNullWhen(true)] out ISyntaxVisitorAction? action
    )
    {
        action = SyntaxVisitor.Continue;
        return true;
    }

    protected bool TryGetParameter<T>(ISortField parentField, IValueNode node, string fieldName, [NotNullWhen(true)] out T fieldNode)
    {
        return RavenSpatialOperationHandlerHelper.TryGetParameter(
            parentField,
            node,
            fieldName,
            _inputParser,
            out fieldNode
        );
    }
}
