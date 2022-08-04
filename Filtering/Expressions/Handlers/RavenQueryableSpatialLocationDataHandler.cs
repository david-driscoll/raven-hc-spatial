using HotChocolate.Data.Filters.Expressions;
using HotChocolate.Raven.Spatial.Filtering.Convention;

namespace HotChocolate.Raven.Spatial.Filtering.Expressions.Handlers;

public class RavenQueryableSpatialLocationDataHandler : QueryableDataOperationHandler
{
    protected override int Operation => RavenSpatialFilterOperations.Location;
}
