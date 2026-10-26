namespace ECMS.Contracts.Configuration;

public record CollectionConfiguration(
    int PollingIntervalSeconds,
    int MaxConcurrentRequests,
    DeviceConfiguration[] DevicesConfiguration
);

public record DeviceConfiguration(
    int Id,
    int Kind,
    string IpAddress,
    int Port,
    string Protocol,
    int RequestTimeoutSeconds
);
