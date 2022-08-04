using HotChocolate.Data.Filters;
using HotChocolate.Raven.Spatial.Filtering.Convention;
using HotChocolate.Types;
using HotChocolate.Types.Spatial;
using static HotChocolate.Raven.Spatial.Filtering.Convention.RavenSpatialFilterOperations;

namespace HotChocolate.Raven.Spatial.Filtering.Types.Operations;

public class GeometryContainsOperationFilterInputType : FilterInputType
{
    protected override void Configure(IFilterInputTypeDescriptor descriptor)
    {
        descriptor.Operation(Geometry).Type<GeometryType>();
        descriptor.Operation(RavenSpatialFilterOperations.Location).Type(typeof(LatLong));
        descriptor.Operation(RavenSpatialFilterOperations.Buffer).Type<FloatType>();
        descriptor.Operation(Wkt).Type<StringType>();
        descriptor.AllowAnd().AllowOr();
    }
}
