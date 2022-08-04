using HotChocolate.Data.Sorting;
using HotChocolate.Data.Sorting.Expressions;
using HotChocolate.Raven.Spatial.Sorting.Expressions.Handlers;
using HotChocolate.Types;
using HotChocolate.Types.Spatial;
using Microsoft.Extensions.DependencyInjection;
using NetTopologySuite;
using NetTopologySuite.Geometries;

namespace HotChocolate.Raven.Spatial.Sorting.Types;

public class GeometrySortInputType<T> : SortInputType<T>
    where T : Geometry
{
    protected override void Configure(ISortInputTypeDescriptor<T> descriptor)
    {
        descriptor.BindFieldsExplicitly();
        descriptor.Field("direction").Type<DefaultSortEnumType>().Extend()
                  .OnBeforeNaming((_, definition) => definition.Handler = new RavenQueryableIgnoreSortFieldDataHandler());
        descriptor.Field("location").Type(typeof(LatLong)).Extend()
                  .OnBeforeNaming((_, definition) => definition.Handler = new RavenQueryableIgnoreSortFieldDataHandler());
        descriptor.Field("geometry").Type<GeometryType>().Extend()
                  .OnBeforeNaming((_, definition) => definition.Handler = new RavenQueryableIgnoreSortFieldDataHandler());
        descriptor.Field("wkt").Type<StringType>().Extend()
                  .OnBeforeNaming((_, definition) => definition.Handler = new RavenQueryableIgnoreSortFieldDataHandler());
    }
}
