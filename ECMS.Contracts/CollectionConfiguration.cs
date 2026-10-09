namespace ECMS.Contracts;

public sealed class CollectionConfiguration
{
    public int PollingIntervalSeconds { get; init; } = 1;

    public List<DeviceConfiguration> DevicesConfiguration { get; init; } = [];
}

public sealed class DeviceConfiguration
{
    public int Id { get; init; }

    public int Kind { get; init; }
}
