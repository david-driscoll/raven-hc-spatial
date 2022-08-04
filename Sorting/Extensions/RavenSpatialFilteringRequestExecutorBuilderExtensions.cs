using HotChocolate.Execution.Configuration;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace HotChocolate;

public static class RavenSpatialSortingRequestExecutorBuilderExtensions
{
    public static IRequestExecutorBuilder AddRavenSpatialSorting(
        this IRequestExecutorBuilder builder
    )
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        return builder.ConfigureSchema(x => x.AddRavenSpatialSorting());
    }
}
