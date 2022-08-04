using HotChocolate.Data.Filters;
using HotChocolate.Data.Filters.Expressions;
using HotChocolate.Raven.Spatial.Filtering.Convention;
using HotChocolate.Raven.Spatial.Filtering.Types;
using NetTopologySuite.Geometries;
using HotChocolate.Raven.Spatial;

// ReSharper disable once CheckNamespace
namespace HotChocolate.Data;

public static class RavenSpatialFilterConventionDescriptorExtensions
{
    /// <summary>
    ///     Adds the spatial filter defaults
    /// </summary>
    public static IFilterConventionDescriptor AddRavenSpatialDefaults(
        this IFilterConventionDescriptor descriptor
    )
    {
        if (descriptor is null)
        {
            throw new ArgumentNullException(nameof(descriptor));
        }

        descriptor.AddRavenSpatialOperations();
        descriptor.BindRavenSpatialTypes();
        descriptor.AddProviderExtension<IFilterProviderExtension>(
            new QueryableFilterProviderExtension(providerDescriptor => providerDescriptor.AddRavenSpatialHandlers())
        );

        return descriptor;
    }

    /// <summary>
    ///     The default names of the spatial filter operations
    /// </summary>
    public static IFilterConventionDescriptor AddRavenSpatialOperations(
        this IFilterConventionDescriptor descriptor
    )
    {
        if (descriptor is null)
        {
            throw new ArgumentNullException(nameof(descriptor));
        }

        descriptor.Operation(RavenSpatialFilterOperations.Contains).Name("contains");
        descriptor.Operation(RavenSpatialFilterOperations.Intersects).Name("intersects");
        descriptor.Operation(RavenSpatialFilterOperations.Disjoint).Name("disjoint");
        descriptor.Operation(RavenSpatialFilterOperations.Within).Name("within");
        descriptor.Operation(RavenSpatialFilterOperations.Buffer).Name("buffer");
        descriptor.Operation(RavenSpatialFilterOperations.Geometry).Name("geometry");
        descriptor.Operation(RavenSpatialFilterOperations.Wkt).Name("wkt");
        descriptor.Operation(RavenSpatialFilterOperations.Location).Name("location");

        return descriptor;
    }

    /// <summary>
    ///     The fields and operations available to each type
    /// </summary>
    public static IFilterConventionDescriptor BindRavenSpatialTypes(
        this IFilterConventionDescriptor descriptor
    )
    {
        if (descriptor is null)
        {
            throw new ArgumentNullException(nameof(descriptor));
        }

        descriptor
           .BindRuntimeType<Geometry, GeometryFilterInputType>()
           .BindRuntimeType<Point, PointFilterInputType>()
           .BindRuntimeType<MultiPoint, MultiPointFilterInputType>()
           .BindRuntimeType<LineString, LineStringFilterInputType>()
           .BindRuntimeType<MultiLineString, MultiLineStringFilterInputType>()
           .BindRuntimeType<Polygon, PolygonFilterInputType>()
           .BindRuntimeType<MultiPolygon, MultiPolygonFilterInputType>()
           .BindRuntimeType<LatLong, LatLongFilterInputType>()
            ;

        return descriptor;
    }
}
