using HotChocolate.Execution.Configuration;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace HotChocolate;

public static class RavenSpatialFilteringRequestExecutorBuilderExtensions
{
    public static IRequestExecutorBuilder AddRavenSpatialFiltering(
        this IRequestExecutorBuilder builder
    )
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        return builder.ConfigureSchema(x => x.AddRavenSpatialFiltering());
    }
}
