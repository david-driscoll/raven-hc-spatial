using HotChocolate.Data.Sorting;
using HotChocolate.Data.Sorting.Expressions;
using HotChocolate.Raven.Spatial.Sorting.Expressions.Handlers;

// ReSharper disable once CheckNamespace
namespace HotChocolate.Data;

public static class SpatialSortProviderDescriptorQueryableExtensions
{
    public static ISortProviderDescriptor<QueryableSortContext> AddRavenSpatialHandlers(
        this ISortProviderDescriptor<QueryableSortContext> descriptor
    )
    {
        return descriptor
               .AddFieldHandler<RavenQueryableSpatialDistanceHandler>()
            ;
//        return descriptor
//              .AddFieldHandler<RavenQueryableSpatialGeometryDataHandler>()
//              .AddFieldHandler<RavenQueryableSpatialWktDataHandler>()
//              .AddFieldHandler<RavenQueryableSpatialBufferDataHandler>()
//              .AddFieldHandler<RavenQueryableSpatialContainsOperationHandler>()
//              .AddFieldHandler<RavenQueryableSpatialDisjointOperationHandler>()
//              .AddFieldHandler<RavenQueryableSpatialIntersectsOperationHandler>()
//              .AddFieldHandler<RavenQueryableSpatialWithinOperationHandler>();
    }
}
