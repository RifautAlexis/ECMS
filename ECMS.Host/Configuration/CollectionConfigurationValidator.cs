using System.Text.Json;

namespace ECMS.Host.Configuration;

public static class CollectionConfigurationValidator
{
    public static void ValidateCollection(JsonElement collection,
        List<string> errors)
    {
        // Validate pollingIntervalSeconds.
        if (!collection.TryGetProperty(
            "pollingIntervalSeconds",
            out var interval))
            errors.Add(
                "Missing required property 'collection.pollingIntervalSeconds'.");
        else if (interval.ValueKind != JsonValueKind.Number ||
            !interval.TryGetInt32(out var seconds))
            errors.Add(
                "'collection.pollingIntervalSeconds' must be an integer.");
        else if (seconds <= 0)
            errors.Add(
                "'collection.pollingIntervalSeconds' must be positive.");

        // Validate devicesConfiguration.
        if (!collection.TryGetProperty(
            "devicesConfiguration",
            out var devices))
        {
            errors.Add(
                "Missing required property 'collection.devicesConfiguration'.");
            return;
        }

        if (devices.ValueKind != JsonValueKind.Array)
        {
            errors.Add(
                "'collection.devicesConfiguration' must be an array.");
            return;
        }

        // Validate each device.
        var index = 0;

        foreach (var device in devices.EnumerateArray())
        {
            var prefix = $"collection.devicesConfiguration[{index}]";

            if (device.ValueKind != JsonValueKind.Object)
            {
                errors.Add($"{prefix} must be a JSON object.");
                index++;
                continue;
            }

            ValidatorHelpers.ValidateRequiredInteger(
                device,
                "id",
                prefix,
                errors);
            ValidatorHelpers.ValidateRequiredInteger(
                device,
                "kind",
                prefix,
                errors);
            ValidatorHelpers.ValidateRequiredString(
                device,
                "ipAddress",
                prefix,
                errors);
            ValidatorHelpers.ValidateRequiredInteger(
                device,
                "port",
                prefix,
                errors);

            index++;
        }
    }
}
