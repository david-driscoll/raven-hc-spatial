using System.Linq.Expressions;
using HotChocolate.Data.Sorting;
using HotChocolate.Data.Sorting.Expressions;
using Raven.Client.Documents.Queries.Spatial;

namespace HotChocolate.Raven.Spatial.Sorting;

public class RavenQueryableSortContext : QueryableSortContext
{
    public RavenQueryableSortContext(ISortInputType initialType, bool inMemory) : base(initialType, inMemory)
    {
    }

    public RavenSortDirection SortDirection { get; set; }
    public DynamicSpatialField? SpatialFieldName { get; set; }
    public string? SpatialWkt { get; set; }
    public string? FieldName { get; set; }
}

public enum RavenSortDirection
{
    None,
    Asc,
    Desc
}
