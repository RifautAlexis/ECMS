using System.Threading.Channels;
using ECMS.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace ECMS.Collection;

public static class CollectionServiceCollectionExtensions
{
    private const int ChannelCapacity = 1024;

    public static IServiceCollection AddDeviceCollection(
        this IServiceCollection services,
        CollectionConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        
        ValidateConfiguration(configuration);

        services.AddSingleton(configuration);
        services.AddSingleton<Channel<CollectedData>>(_ =>
            Channel.CreateBounded<CollectedData>(
                new BoundedChannelOptions(ChannelCapacity)
                {
                    FullMode = BoundedChannelFullMode.Wait,
                    SingleReader = true,
                    SingleWriter = true
                }));
        services.AddSingleton(provider => provider.GetRequiredService<Channel<CollectedData>>().Reader);
        services.AddSingleton(provider => provider.GetRequiredService<Channel<CollectedData>>().Writer);
        services.AddHostedService<CollectionWorker>();

        return services;
    }

    private static void ValidateConfiguration(CollectionConfiguration configuration)
    {
        if (configuration.PollingIntervalSeconds <= 0)
        {
            throw new InvalidDataException("collection.pollingIntervalSeconds must be greater than zero.");
        }

        if (configuration.DevicesConfiguration is null)
        {
            throw new InvalidDataException("collection.devicesConfiguration must be an array.");
        }

        var seenIds = new HashSet<int>();
        foreach (var device in configuration.DevicesConfiguration)
        {
            if (device is null)
            {
                throw new InvalidDataException("collection.devicesConfiguration cannot contain null entries.");
            }

            if (device.Id <= 0)
            {
                throw new InvalidDataException("Every configured device must have a positive id.");
            }

            if (!seenIds.Add(device.Id))
            {
                throw new InvalidDataException($"Device id {device.Id} is configured more than once.");
            }

            if (device.Kind is < 1 or > 3)
            {
                throw new InvalidDataException(
                    $"Device {device.Id} has unsupported kind {device.Kind}. Supported kinds are 1, 2, and 3.");
            }
        }
    }
}
