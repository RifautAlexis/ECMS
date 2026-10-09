namespace ECMS.Contracts.Devices;

public sealed record RfPowerAmplifierData(
    double ForwardPower,
    double ReflectedPower,
    double Vswr,
    double Temperature,
    double SupplyVoltage,
    double Current,
    double RfInputPower,
    double Gain,
    double Efficiency,
    bool Enabled,
    bool RfOutputEnabled,
    bool PllLocked,
    bool OverTemperature,
    bool OverCurrent,
    bool VswrAlarm);
