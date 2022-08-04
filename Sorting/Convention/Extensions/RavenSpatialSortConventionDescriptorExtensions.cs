using HotChocolate.Data.Sorting;
using HotChocolate.Data.Sorting.Expressions;
using HotChocolate.Raven.Spatial.Sorting.Convention;
using HotChocolate.Raven.Spatial.Sorting.Types;
using NetTopologySuite.Geometries;
using HotChocolate.Raven.Spatial;

// ReSharper disable once CheckNamespace
namespace HotChocolate.Data;

public static class RavenSpatialSortConventionDescriptorExtensions
{
    /// <summary>
    ///     Adds the spatial Sort defaults
    /// </summary>
    public static ISortConventionDescriptor AddRavenSpatialDefaults(
        this ISortConventionDescriptor descriptor
    )
    {
        if (descriptor is null)
        {
            throw new ArgumentNullException(nameof(descriptor));
        }

        descriptor.AddRavenSpatialOperations();
        descriptor.BindRavenSpatialTypes();
        descriptor.AddProviderExtension<ISortProviderExtension>(
            new QueryableSortProviderExtension(providerDescriptor => providerDescriptor.AddRavenSpatialHandlers())
        );

        return descriptor;
    }

    /// <summary>
    ///     The default names of the spatial Sort operations
    /// </summary>
    public static ISortConventionDescriptor AddRavenSpatialOperations(
        this ISortConventionDescriptor descriptor
    )
    {
        if (descriptor is null)
        {
            throw new ArgumentNullException(nameof(descriptor));
        }

        descriptor.Operation(RavenSpatialSortOperations.Distance).Name("distance");
        descriptor.Operation(RavenSpatialSortOperations.Geometry).Name("geometry");
        descriptor.Operation(RavenSpatialSortOperations.Wkt).Name("wkt");

        return descriptor;
    }

    /// <summary>
    ///     The fields and operations available to each type
    /// </summary>
    public static ISortConventionDescriptor BindRavenSpatialTypes(
        this ISortConventionDescriptor descriptor
    )
    {
        if (descriptor is null)
        {
            throw new ArgumentNullException(nameof(descriptor));
        }

        descriptor
           .BindRuntimeType<Geometry, GeometrySortInputType>()
           .BindRuntimeType<Point, PointSortInputType>()
           .BindRuntimeType<MultiPoint, MultiPointSortInputType>()
           .BindRuntimeType<LineString, LineStringSortInputType>()
           .BindRuntimeType<MultiLineString, MultiLineStringSortInputType>()
           .BindRuntimeType<Polygon, PolygonSortInputType>()
           .BindRuntimeType<MultiPolygon, MultiPolygonSortInputType>()
           .BindRuntimeType<LatLong, LatLongSortInputType>()
            ;

        return descriptor;
    }
}
