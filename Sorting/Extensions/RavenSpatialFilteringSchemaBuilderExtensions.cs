using HotChocolate.Data.Sorting;

// ReSharper disable once CheckNamespace
namespace HotChocolate;

public static class RavenSpatialSortingSchemaBuilderExtensions
{
    public static ISchemaBuilder AddRavenSpatialSorting(this ISchemaBuilder builder)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        return builder.AddConvention<ISortConvention>(new SortConventionExtension(x => x.AddRavenSpatialDefaults()));
    }
}
