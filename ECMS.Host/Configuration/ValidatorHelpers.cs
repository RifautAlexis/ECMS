using System.Text.Json;

namespace ECMS.Host.Configuration;

public static class ValidatorHelpers
{
    public static bool TryGetObject(
        JsonElement parent,
        string propertyName,
        List<string> errors,
        out JsonElement value)
    {
        value = default;

        if (!parent.TryGetProperty(
            propertyName,
            out value))
        {
            errors.Add($"Missing required property '{propertyName}'.");
            return false;
        }

        if (value.ValueKind != JsonValueKind.Object)
        {
            errors.Add(
                $"Property '{propertyName}' must be a JSON object.");
            return false;
        }

        return true;
    }

    public static void ValidateRequiredString(
        JsonElement parent,
        string propertyName,
        string prefix,
        List<string> errors)
    {
        var path = $"{prefix}.{propertyName}";

        if (!parent.TryGetProperty(
            propertyName,
            out var value))
        {
            errors.Add($"Missing required property '{path}'.");
            return;
        }

        if (value.ValueKind != JsonValueKind.String)
        {
            errors.Add($"'{path}' must be a string.");
            return;
        }

        if (string.IsNullOrWhiteSpace(value.GetString())) errors.Add($"'{path}' must not be empty.");
    }

    public static void ValidateRequiredInteger(
        JsonElement parent,
        string propertyName,
        string prefix,
        List<string> errors)
    {
        var path = $"{prefix}.{propertyName}";

        if (!parent.TryGetProperty(
            propertyName,
            out var value))
        {
            errors.Add($"Missing required property '{path}'.");
            return;
        }

        if (value.ValueKind != JsonValueKind.Number || !value.TryGetInt32(out _))
            errors.Add($"'{path}' must be an integer.");
    }
}
