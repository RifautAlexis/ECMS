using System.Text.Json;

namespace ECMS.Host.Configuration;

public static class CollectionConfigurationValidator
{
    public static void ValidateCollection(JsonElement collection,
        List<string> errors)
    {
        // Validate pollingIntervalSeconds
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

        // Validate maxConcurrentRequests
        if (!collection.TryGetProperty(
            "maxConcurrentRequests",
            out var concurrency))
            errors.Add("Missing required property 'collection.maxConcurrentRequests'.");
        else if (concurrency.ValueKind != JsonValueKind.Number ||
            !concurrency.TryGetInt32(out var maximumConcurrency))
            errors.Add("'collection.maxConcurrentRequests' must be an integer.");
        else if (maximumConcurrency <= 0)
            errors.Add("'collection.maxConcurrentRequests' must be positive.");

        // Validate devicesConfiguration
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
        var deviceIds = new HashSet<int>();

        foreach (var device in devices.EnumerateArray())
        {
            var prefix = $"collection.devicesConfiguration[{index}]";

            if (device.ValueKind != JsonValueKind.Object)
            {
                errors.Add($"{prefix} must be a JSON object.");
                index++;
                continue;
            }

            var validId = ValidatorHelpers.TryGetInteger(
                device,
                "id",
                prefix,
                errors,
                out var id);
            var validKind = ValidatorHelpers.TryGetInteger(
                device,
                "kind",
                prefix,
                errors,
                out var kind);
            ValidatorHelpers.ValidateRequiredString(
                device,
                "ipAddress",
                prefix,
                errors);
            var validPort = ValidatorHelpers.TryGetInteger(
                device,
                "port",
                prefix,
                errors,
                out var port);
            var validTimeout = ValidatorHelpers.TryGetInteger(
                device,
                "requestTimeoutSeconds",
                prefix,
                errors,
                out var timeout);

            if (!device.TryGetProperty(
                    "protocol",
                    out var protocol) ||
                protocol.ValueKind != JsonValueKind.String ||
                string.IsNullOrWhiteSpace(protocol.GetString()))
                errors.Add($"{prefix}.protocol must be a non-empty string.");
            else if (!string.Equals(
                protocol.GetString(),
                "Http",
                StringComparison.OrdinalIgnoreCase)) errors.Add($"{prefix}.protocol must be 'Http'.");

            // Validate id
            if (validId && id <= 0)
                errors.Add($"{prefix}.id must be positive.");
            if (validId && id > 0 && !deviceIds.Add(id))
                errors.Add($"{prefix}.id must be unique.");

            // Validate kind
            if (validKind && kind is < 1 or > 3)
                errors.Add($"{prefix}.kind must be 1, 2, or 3.");

            // Validate port
            if (validPort && port is < 1 or > 65535)
                errors.Add($"{prefix}.port must be between 1 and 65535.");

            // Validate requestTimeoutSeconds
            if (validTimeout && timeout <= 0)
                errors.Add($"{prefix}.requestTimeoutSeconds must be positive.");
            else if (validTimeout && timeout > 2592000)
                errors.Add($"{prefix}.requestTimeoutSeconds must be 30 days or less.");

            index++;
        }
    }
}
