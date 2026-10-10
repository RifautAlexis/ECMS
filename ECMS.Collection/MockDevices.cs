using ECMS.Contracts;
using ECMS.Contracts.Configuration;
using ECMS.Contracts.Devices;

namespace ECMS.Collection;

internal interface IMockDevice
{
    CollectedData Collect();
}

internal static class MockDeviceFactory
{
    public static IMockDevice Create(DeviceConfiguration configuration) =>
        configuration.Kind switch
        {
            1 => new RfPowerSensor(configuration.Id),
            2 => new RfPowerAmplifier(configuration.Id),
            3 => new RfMonitoringSystem(configuration.Id),
            _ => throw new InvalidDataException($"Unsupported device kind {configuration.Kind}.")
        };
}

internal sealed class RfPowerSensor : IMockDevice
{
    private readonly int _id;
    private readonly Random _random;
    private double _forwardPower;
    private double _temperature;

    public RfPowerSensor(int id)
    {
        _id = id;
        _random = new Random(id);
        _forwardPower = 75 + DeviceSimulationMath.RandomValue(_random, 0, 20);
        _temperature = 28 + DeviceSimulationMath.RandomValue(_random, 0, 8);
    }

    public CollectedData Collect()
    {
        _forwardPower = DeviceSimulationMath.MoveTowards(_random, _forwardPower, 82, 3, 6, 110);
        var reflectionRatio = Math.Clamp(
            0.015 + DeviceSimulationMath.RandomValue(_random, -0.008, 0.02),
            0.002,
            0.08);
        var reflectedPower = _forwardPower * reflectionRatio;
        var vswr = DeviceSimulationMath.CalculateVswr(_forwardPower, reflectedPower);
        _temperature = DeviceSimulationMath.MoveTowards(
            _random,
            _temperature,
            22 + _forwardPower * 0.12,
            0.2,
            15,
            65);

        return new CollectedData(
            _id,
            1,
            new RfPowerSensorData(
                DeviceSimulationMath.Round(_forwardPower),
                DeviceSimulationMath.Round(reflectedPower),
                DeviceSimulationMath.Round(vswr),
                DeviceSimulationMath.Round(_temperature)));
    }
}

internal sealed class RfPowerAmplifier : IMockDevice
{
    private readonly int _id;
    private readonly Random _random;
    private double _forwardPower;
    private double _temperature;

    public RfPowerAmplifier(int id)
    {
        _id = id;
        _random = new Random(id);
        _forwardPower = 1150 + DeviceSimulationMath.RandomValue(_random, 0, 200);
        _temperature = 35 + DeviceSimulationMath.RandomValue(_random, 0, 10);
    }

    public CollectedData Collect()
    {
        _forwardPower = DeviceSimulationMath.MoveTowards(_random, _forwardPower, 1250, 45, 0, 1600);
        var reflectedPower = _forwardPower * Math.Clamp(
            0.012 + DeviceSimulationMath.RandomValue(_random, -0.008, 0.045),
            0.001,
            0.06);
        var vswr = DeviceSimulationMath.CalculateVswr(_forwardPower, reflectedPower);
        var rfInputPower = 30 + _forwardPower / 1000 + DeviceSimulationMath.RandomValue(_random, -1, 1);
        var efficiency = Math.Clamp(
            80 + DeviceSimulationMath.RandomValue(_random, -2, 2),
            70,
            90);
        var supplyVoltage = 48 + DeviceSimulationMath.RandomValue(_random, -0.8, 0.8);
        var current = _forwardPower / (supplyVoltage * (efficiency / 100));
        _temperature = DeviceSimulationMath.MoveTowards(
            _random,
            _temperature,
            25 + _forwardPower * 0.04,
            1.5,
            20,
            90);
        var enabled = true;
        var overTemperature = _temperature >= 75;
        var overCurrent = current >= 40;
        var vswrAlarm = vswr >= 1.5;

        return new CollectedData(
            _id,
            2,
            new RfPowerAmplifierData(
                DeviceSimulationMath.Round(_forwardPower),
                DeviceSimulationMath.Round(reflectedPower),
                DeviceSimulationMath.Round(vswr),
                DeviceSimulationMath.Round(_temperature),
                DeviceSimulationMath.Round(supplyVoltage),
                DeviceSimulationMath.Round(current),
                DeviceSimulationMath.Round(rfInputPower),
                DeviceSimulationMath.Round(10 * Math.Log10(_forwardPower / rfInputPower)),
                DeviceSimulationMath.Round(efficiency),
                enabled,
                enabled,
                true,
                overTemperature,
                overCurrent,
                vswrAlarm));
    }
}

internal sealed class RfMonitoringSystem(int id) : IMockDevice
{
    private readonly Random _random = new(id);
    private readonly double[] _channelPower = [820, 795];
    private double _temperature = 34;

    public CollectedData Collect()
    {
        var channels = new RfMonitoringChannelData[_channelPower.Length];
        for (var index = 0; index < _channelPower.Length; index++)
        {
            _channelPower[index] = DeviceSimulationMath.MoveTowards(
                _random,
                _channelPower[index],
                810 - index * 15,
                30,
                0,
                1000);
            var reflectedPower = _channelPower[index] * Math.Clamp(
                0.012 + DeviceSimulationMath.RandomValue(_random, -0.005, 0.04),
                0.001,
                0.06);
            var vswr = DeviceSimulationMath.CalculateVswr(_channelPower[index], reflectedPower);
            channels[index] = new RfMonitoringChannelData(
                DeviceSimulationMath.Round(_channelPower[index]),
                DeviceSimulationMath.Round(reflectedPower),
                DeviceSimulationMath.Round(vswr),
                true,
                vswr >= 1.5);
        }

        _temperature = DeviceSimulationMath.MoveTowards(
            _random,
            _temperature,
            25 + channels.Average(channel => channel.ForwardPower) * 0.016,
            0.8,
            20,
            85);
        var systemStatus = channels.Any(channel => channel.Alarm) ? "Warning" : "Running";

        return new CollectedData(
            id,
            3,
            new RfMonitoringSystemData(
                DeviceSimulationMath.Round(_temperature),
                systemStatus,
                channels));
    }
}

internal static class DeviceSimulationMath
{
    public static double RandomValue(Random random, double minimum, double maximum) =>
        minimum + random.NextDouble() * (maximum - minimum);

    public static double MoveTowards(
        Random random,
        double current,
        double target,
        double maxStep,
        double minimum,
        double maximum) =>
        Math.Clamp(
            current + Math.Clamp(target - current + RandomValue(random, -maxStep, maxStep), -maxStep, maxStep),
            minimum,
            maximum);

    public static double CalculateVswr(double forwardPower, double reflectedPower)
    {
        var reflectionCoefficient = Math.Sqrt(reflectedPower / forwardPower);
        return (1 + reflectionCoefficient) / (1 - reflectionCoefficient);
    }

    public static double Round(double value) => Math.Round(value, 2);
}
