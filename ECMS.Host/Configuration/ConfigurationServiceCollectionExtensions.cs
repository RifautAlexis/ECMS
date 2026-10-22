using System.Text.Json;
using ECMS.Contracts.Configuration;
using ECMS.Host.Configuration.Models;

namespace ECMS.Host.Configuration;

public static class ConfigurationServiceCollectionExtensions
{
    public static void ValidateAndAddConfiguration(
        this IServiceCollection services)
    {
        const string configFileName = "config.json";
        var configFilePath = Path.Combine(
            AppContext.BaseDirectory,
            configFileName);

        var errors = ValidateConfiguration(configFilePath);

        if (errors.Count > 0) throw new ConfigurationValidationException(errors);

        AddConfiguration(
            services,
            configFilePath);
    }

    private static void AddConfiguration(IServiceCollection services,
        string configFilePath)
    {
        var contentFile = File.ReadAllText(configFilePath);
        var configurationSettings = JsonSerializer.Deserialize<ConfigurationSettings>(
            contentFile,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        if (configurationSettings == null)
            throw new ConfigurationValidationException(
                new List<string>
                {
                    $"Failed to deserialize configuration file: {configFilePath}"
                });

        var collectionConfiguration = BuildCollectionConfiguration(configurationSettings);
        var coreConfiguration = BuildCoreConfiguration(configurationSettings);
        var communicationConfiguration = BuildCommunicationConfiguration(configurationSettings);

        services.AddSingleton(collectionConfiguration);
        services.AddSingleton(coreConfiguration);
        services.AddSingleton(communicationConfiguration);
    }

    private static List<string> ValidateConfiguration(string configFilePath)
    {
        var configFileExists = File.Exists(configFilePath);

        var errors = new List<string>();

        if (!configFileExists)
        {
            errors.Add($"The configuration file was not found at path: {configFilePath}");
            return errors;
        }

        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(configFilePath));

            var root = document.RootElement;

            if (root.ValueKind != JsonValueKind.Object)
            {
                errors.Add($"The configuration file is not a valid JSON object: {configFilePath}");
                return errors;
            }

            if (ValidatorHelpers.TryGetObject(
                root,
                "collection",
                errors,
                out var collection))
                CollectionConfigurationValidator.ValidateCollection(
                    collection,
                    errors);
            if (ValidatorHelpers.TryGetObject(
                root,
                "core",
                errors,
                out var core))
                CoreConfigurationValidator.ValidateCore(
                    core,
                    errors);
            if (ValidatorHelpers.TryGetObject(
                root,
                "communication",
                errors,
                out var communication))
                CommunicationConfigurationValidator.ValidateCommunication(
                    communication,
                    errors);
        }
        catch (JsonException ex)
        {
            errors.Add(
                $"Invalid JSON syntax at line " +
                $"{ex.LineNumber}, byte position " +
                $"{ex.BytePositionInLine}: {ex.Message}");
        }
        catch (IOException ex)
        {
            errors.Add($"Unable to read configuration file: {ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
            errors.Add($"Access denied to configuration file: {ex.Message}");
        }

        return errors;
    }

    private static CollectionConfiguration BuildCollectionConfiguration(ConfigurationSettings configurationSettings)
    {
        return new CollectionConfiguration(
            configurationSettings.Collection.PollingIntervalSeconds,
            configurationSettings.Collection.DevicesConfiguration
        );
    }

    private static CoreConfiguration BuildCoreConfiguration(
        ConfigurationSettings configurationSettings)
    {
        return new CoreConfiguration();
    }

    private static CommunicationConfiguration BuildCommunicationConfiguration(
        ConfigurationSettings configurationSettings)
    {
        return new CommunicationConfiguration();
    }
}
