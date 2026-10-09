namespace ECMS.Contracts.Devices;

public sealed record RfMonitoringChannelData(
    double ForwardPower,
    double ReflectedPower,
    double Vswr,
    bool Enabled,
    bool Alarm);

public sealed record RfMonitoringSystemData(
    double Temperature,
    string SystemStatus,
    IReadOnlyList<RfMonitoringChannelData> Channels);
