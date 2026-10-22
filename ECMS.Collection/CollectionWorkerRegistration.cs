using ECMS.Contracts.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace ECMS.Collection;

public static class CollectionWorkerRegistration
{
    public static IServiceCollection AddCollectionWorker(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<ManagedWorker, CollectionWorker>();
        return services;
    }
}
