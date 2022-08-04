using HotChocolate.Data.Filters;
using HotChocolate.Raven.Spatial.Filtering.Convention;
using HotChocolate.Raven.Spatial.Filtering.Types.Operations;

namespace HotChocolate.Raven.Spatial.Filtering.Types;

public class LatLongFilterInputType : FilterInputType<LatLong>
{
    protected override void Configure(IFilterInputTypeDescriptor<LatLong> descriptor)
    {
        descriptor.BindFieldsExplicitly();

        descriptor.Operation(RavenSpatialFilterOperations.Contains).Type<GeometryContainsOperationFilterInputType>();
        descriptor.Operation(RavenSpatialFilterOperations.Disjoint).Type<GeometryDisjointOperationFilterInputType>();
        descriptor.Operation(RavenSpatialFilterOperations.Intersects).Type<GeometryIntersectsOperationFilterInputType>();
        descriptor.Operation(RavenSpatialFilterOperations.Within).Type<GeometryWithinOperationFilterInputType>();
    }
}
