using HotChocolate.Data.Filters;
using HotChocolate.Data.Filters.Expressions;
using HotChocolate.Raven.Spatial.Filtering.Expressions.Handlers;

// ReSharper disable once CheckNamespace
namespace HotChocolate.Data;

public static class SpatialFilterProviderDescriptorQueryableExtensions
{
    public static IFilterProviderDescriptor<QueryableFilterContext> AddRavenSpatialHandlers(
        this IFilterProviderDescriptor<QueryableFilterContext> descriptor
    )
    {
        return descriptor
              .AddFieldHandler<RavenQueryableSpatialGeometryDataHandler>()
              .AddFieldHandler<RavenQueryableSpatialLocationDataHandler>()
              .AddFieldHandler<RavenQueryableSpatialWktDataHandler>()
              .AddFieldHandler<RavenQueryableSpatialBufferDataHandler>()
              .AddFieldHandler<RavenQueryableSpatialContainsOperationHandler>()
              .AddFieldHandler<RavenQueryableSpatialDisjointOperationHandler>()
              .AddFieldHandler<RavenQueryableSpatialIntersectsOperationHandler>()
              .AddFieldHandler<RavenQueryableSpatialWithinOperationHandler>()
            ;
    }
}
