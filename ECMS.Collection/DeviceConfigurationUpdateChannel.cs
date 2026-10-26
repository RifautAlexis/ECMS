using System.Threading.Channels;
using ECMS.Contracts.Configuration;

namespace ECMS.Collection;

internal sealed class DeviceConfigurationUpdateChannel :
    IDeviceConfigurationUpdatePublisher,
    IDeviceConfigurationUpdateSource
{
    private readonly Channel<IReadOnlyList<DeviceConfiguration>> _channel =
        Channel.CreateBounded<IReadOnlyList<DeviceConfiguration>>(
            new BoundedChannelOptions(1)
            {
                FullMode = BoundedChannelFullMode.DropOldest,
                SingleReader = true,
                SingleWriter = false
            });

    public DeviceConfigurationUpdateChannel(CollectionConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        Validate(configuration.DevicesConfiguration);
        _channel.Writer.TryWrite(configuration.DevicesConfiguration.ToArray());
    }

    public ValueTask PublishAsync(
        IReadOnlyList<DeviceConfiguration> devices,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(devices);
        cancellationToken.ThrowIfCancellationRequested();
        Validate(devices);

        return _channel.Writer.WriteAsync(devices.ToArray(), cancellationToken);
    }

    public IAsyncEnumerable<IReadOnlyList<DeviceConfiguration>> ReadUpdatesAsync(
        CancellationToken cancellationToken = default)
    {
        return _channel.Reader.ReadAllAsync(cancellationToken);
    }

    private static void Validate(IReadOnlyList<DeviceConfiguration> devices)
    {
        var ids = new HashSet<int>();
        foreach (var device in devices)
        {
            if (device.Id <= 0)
                throw new ArgumentOutOfRangeException(nameof(devices), "Device IDs must be positive.");
            if (!ids.Add(device.Id))
                throw new ArgumentException($"Duplicate device ID {device.Id}.", nameof(devices));
            if (string.IsNullOrWhiteSpace(device.IpAddress))
                throw new ArgumentException($"Device {device.Id} has no IP address.", nameof(devices));
            if (device.Port is < 1 or > 65535)
                throw new ArgumentOutOfRangeException(nameof(devices), $"Device {device.Id} has an invalid port.");
            if (device.RequestTimeoutSeconds is <= 0 or > 2592000)
                throw new ArgumentOutOfRangeException(nameof(devices), $"Device {device.Id} has an invalid request timeout.");
            if (!string.Equals(device.Protocol, "Http", StringComparison.OrdinalIgnoreCase))
                throw new NotSupportedException($"Protocol '{device.Protocol}' is not supported.");
        }
    }
}
