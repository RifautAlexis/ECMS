namespace ECMS.Contracts.Devices;

public sealed record RfPowerSensorData(
    double ForwardPower,
    double ReflectedPower,
    double Vswr,
    double Temperature);
