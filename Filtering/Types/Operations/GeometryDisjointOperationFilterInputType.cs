using HotChocolate.Data.Filters;
using HotChocolate.Raven.Spatial.Filtering.Convention;
using HotChocolate.Types;
using HotChocolate.Types.Spatial;

namespace HotChocolate.Raven.Spatial.Filtering.Types.Operations;

public class GeometryDisjointOperationFilterInputType : FilterInputType
{
    protected override void Configure(IFilterInputTypeDescriptor descriptor)
    {
        descriptor.Operation(RavenSpatialFilterOperations.Geometry).Type<GeometryType>();
        descriptor.Operation(RavenSpatialFilterOperations.Location).Type(typeof(LatLong));
        descriptor.Operation(RavenSpatialFilterOperations.Buffer).Type<FloatType>();
        descriptor.Operation(RavenSpatialFilterOperations.Wkt).Type<StringType>();
        descriptor.AllowAnd().AllowOr();
    }
}
