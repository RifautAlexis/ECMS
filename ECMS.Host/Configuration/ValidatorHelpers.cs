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

    public static bool TryGetString(
        JsonElement element,
        string propertyName,
        string prefix,
        List<string> errors,
        out string value)
    {
        value = default;

        if (!element.TryGetProperty(
            propertyName,
            out var property))
        {
            errors.Add($"{prefix}.{propertyName} is required.");
            return false;
        }

        if (property.ValueKind == JsonValueKind.Null)
        {
            errors.Add($"{prefix}.{propertyName} cannot be null.");
            return false;
        }

        if (property.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(property.GetString()))
        {
            errors.Add($"{prefix}.{propertyName} must be a non-empty string.");
            return false;
        }

        value = property.GetString()!;
        return true;
    }

    public static bool TryGetInteger(
        JsonElement element,
        string propertyName,
        string prefix,
        List<string> errors,
        out int value)
    {
        value = default;

        if (!element.TryGetProperty(
            propertyName,
            out var property))
        {
            errors.Add($"{prefix}.{propertyName} is required.");
            return false;
        }

        if (property.ValueKind != JsonValueKind.Number || !property.TryGetInt32(out var parsedValue))
        {
            errors.Add($"{prefix}.{propertyName} must be an integer.");
            return false;
        }

        value = parsedValue;
        return true;
    }
}
