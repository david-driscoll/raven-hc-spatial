using HotChocolate.Data.Filters;

// ReSharper disable once CheckNamespace
namespace HotChocolate;

public static class RavenSpatialFilteringSchemaBuilderExtensions
{
    public static ISchemaBuilder AddRavenSpatialFiltering(this ISchemaBuilder builder)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        return builder.AddConvention<IFilterConvention>(new FilterConventionExtension(x => x.AddRavenSpatialDefaults()));
    }
}
