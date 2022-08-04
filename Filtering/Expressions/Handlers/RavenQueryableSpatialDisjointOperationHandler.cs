using System.Linq.Expressions;
using HotChocolate.Data.Filters;
using HotChocolate.Raven.Spatial.Filtering.Convention;
using HotChocolate.Types;
using Raven.Client.Documents.Queries.Spatial;

namespace HotChocolate.Raven.Spatial.Filtering.Expressions.Handlers;

public class RavenQueryableSpatialDisjointOperationHandler : RavenQueryableSpatialOperationHandlerBase
{
    public RavenQueryableSpatialDisjointOperationHandler(
        IFilterConvention convention,
        InputParser inputParser
    )
        : base(convention, inputParser, RavenSpatialFilterOperations.Disjoint)
    {
    }

    protected override Expression<Func<SpatialCriteriaFactory, SpatialCriteria>> GetSpatialExpression(string wkt, double? buffer)
    {
        return RavenExpressionBuilder.Disjoint(wkt, buffer);
    }
}
