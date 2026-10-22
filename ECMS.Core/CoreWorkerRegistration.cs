using ECMS.Contracts.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace ECMS.Core;

public static class CoreWorkerRegistration
{
    public static IServiceCollection AddCoreWorker(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<ManagedWorker, CoreWorker>();
        return services;
    }
}
