using System.Linq.Expressions;
using HotChocolate.Data.Filters;
using HotChocolate.Raven.Spatial.Filtering.Convention;
using HotChocolate.Types;
using NetTopologySuite;
using Raven.Client.Documents.Queries.Spatial;

namespace HotChocolate.Raven.Spatial.Filtering.Expressions.Handlers;

public class RavenQueryableSpatialWithinOperationHandler : RavenQueryableSpatialOperationHandlerBase
{
    public RavenQueryableSpatialWithinOperationHandler(
        IFilterConvention convention,
        InputParser inputParser,
        NtsGeometryServices geometryServices
    )
        : base(convention, inputParser, RavenSpatialFilterOperations.Within)
    {
    }

    protected override Expression<Func<SpatialCriteriaFactory, SpatialCriteria>> GetSpatialExpression(string wkt, double? buffer)
    {
        return RavenExpressionBuilder.Within(wkt, buffer);
    }
}
