using HotChocolate.Data.Filters.Expressions;
using HotChocolate.Raven.Spatial.Filtering.Convention;

namespace HotChocolate.Raven.Spatial.Filtering.Expressions.Handlers;

public class RavenQueryableSpatialGeometryDataHandler : QueryableDataOperationHandler
{
    protected override int Operation => RavenSpatialFilterOperations.Geometry;
}
