using HotChocolate.Data.Filters;
using HotChocolate.Raven.Spatial.Filtering.Types.Operations;
using NetTopologySuite.Geometries;
using static HotChocolate.Raven.Spatial.Filtering.Convention.RavenSpatialFilterOperations;

namespace HotChocolate.Raven.Spatial.Filtering.Types;

public class GeometryFilterInputType<T> : FilterInputType<T>
    where T : Geometry
{
    protected override void Configure(IFilterInputTypeDescriptor<T> descriptor)
    {
        descriptor.BindFieldsExplicitly();

        descriptor.Operation(Contains).Type<GeometryContainsOperationFilterInputType>();
        descriptor.Operation(Disjoint).Type<GeometryDisjointOperationFilterInputType>();
        descriptor.Operation(Intersects).Type<GeometryIntersectsOperationFilterInputType>();
        descriptor.Operation(Within).Type<GeometryWithinOperationFilterInputType>();
    }
}
