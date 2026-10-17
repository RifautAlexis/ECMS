namespace ECMS.Host.Configuration;

public record ConfigurationSettings(
    CollectionConfiguration CollectionConfiguration,
    CoreConfiguration CoreConfiguration,
    CommunicationConfiguration CommunicationConfiguration);

public record CollectionConfiguration(
    int PollingIntervalSeconds,
    DeviceConfiguration[] DevicesConfiguration);

public record DeviceConfiguration(
    string Kind,
    string IpAddress,
    int Port);

public record CoreConfiguration();
    
public record CommunicationConfiguration();
