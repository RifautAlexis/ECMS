using ECMS.Contracts.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace ECMS.Communication;

public static class CommunicationWorkerRegistration
{
    public static IServiceCollection AddCommunicationWorker(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<ManagedWorker, CommunicationWorker>();
        return services;
    }
}
