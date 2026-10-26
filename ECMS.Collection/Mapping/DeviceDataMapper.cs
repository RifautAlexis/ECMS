using System.Text.Json;
using ECMS.Contracts.Devices;

namespace ECMS.Collection.Mapping;

public sealed class DeviceDataMapper : IDeviceDataMapper
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public object Map(int kind, string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        return kind switch
        {
            1 => Deserialize<RfPowerSensorData>(json, kind),
            2 => Deserialize<RfPowerAmplifierData>(json, kind),
            3 => Deserialize<RfMonitoringSystemData>(json, kind),
            _ => throw new NotSupportedException($"Device kind {kind} is not supported.")
        };
    }

    private static T Deserialize<T>(string json, int kind)
    {
        return JsonSerializer.Deserialize<T>(json, JsonOptions)
            ?? throw new JsonException($"The response for device kind {kind} was empty.");
    }
}
