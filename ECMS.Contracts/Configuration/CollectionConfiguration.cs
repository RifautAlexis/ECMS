namespace ECMS.Contracts.Configuration;

public record CollectionConfiguration(
    int PollingIntervalSeconds,
    DeviceConfiguration[] DevicesConfiguration
);

public record DeviceConfiguration(
    int Id,
    int Kind,
    string IpAddress,
    int Port
);
