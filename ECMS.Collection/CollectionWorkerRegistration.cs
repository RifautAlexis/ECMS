using ECMS.Collection.Adapters;
using ECMS.Collection.Mapping;
using ECMS.Collection.Orchestration;
using ECMS.Contracts.Configuration;
using ECMS.Contracts.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace ECMS.Collection;

public static class CollectionWorkerRegistration
{
    public static IServiceCollection AddCollectionWorker(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<HttpClient>(_ => new HttpClient
        {
            Timeout = Timeout.InfiniteTimeSpan
        });

        services.AddSingleton<DeviceConfigurationUpdateChannel>();


        // 
        services.AddSingleton<IDeviceConfigurationUpdatePublisher>(provider =>
            provider.GetRequiredService<DeviceConfigurationUpdateChannel>());

        // 
        services.AddSingleton<IDeviceConfigurationUpdateSource>(provider =>
            provider.GetRequiredService<DeviceConfigurationUpdateChannel>());

        // Register the protocol adapter (e.g., HTTP adapter) for device communication
        // Used by the CollectionOrchestrator to send requests to devices and receive responses
        services.AddSingleton<IProtocolAdapter, HttpAdapter>();

        services.AddSingleton<IDeviceDataMapper, DeviceDataMapper>();

        services.AddSingleton<CollectionOrchestrator>();

        services.AddSingleton<ManagedWorker, CollectionWorker>();

        return services;
    }
}
