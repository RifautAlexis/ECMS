namespace ECMS.Contracts.Configuration;

public interface IDeviceConfigurationUpdatePublisher
{
    ValueTask PublishAsync(
        IReadOnlyList<DeviceConfiguration> devices,
        CancellationToken cancellationToken = default);
}

public interface IDeviceConfigurationUpdateSource
{
    IAsyncEnumerable<IReadOnlyList<DeviceConfiguration>> ReadUpdatesAsync(
        CancellationToken cancellationToken = default);
}
