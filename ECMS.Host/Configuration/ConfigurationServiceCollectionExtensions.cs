using System.Text.Json;

namespace ECMS.Host.Configuration;

public static class ConfigurationServiceCollectionExtensions
{
    public static bool TryAddConfiguration(
        this IServiceCollection services,
        string configFilePath,
        out List<string> errors)
    {
        
        errors = ValidateConfiguration(configFilePath);
        
        if (errors.Count > 0)
        {
            return false;
        }
        
        var configuration = new ConfigurationBuilder()
                           .SetBasePath(AppContext.BaseDirectory)
                           .AddJsonFile(configFilePath, optional: false, reloadOnChange: false)
                           .Build();

        services.Configure<CollectionConfiguration>(configuration.GetSection("collection"));
        services.Configure<CoreConfiguration>(configuration.GetSection("core"));
        services.Configure<CommunicationConfiguration>(configuration.GetSection("communication"));

        return true;
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
            using JsonDocument document = JsonDocument.Parse(File.ReadAllText(configFilePath));
            
            JsonElement root = document.RootElement;
            
            if(root.ValueKind != JsonValueKind.Object)
            {
                errors.Add($"The configuration file is not a valid JSON object: {configFilePath}");
                return errors;
            }
            else
            {
                if(ValidatorHelpers.TryGetObject(root, "collection", errors, out var collection))
                {
                    CollectionConfigurationValidator.ValidateCollection(collection, errors);
                }
                if(ValidatorHelpers.TryGetObject(root, "core", errors, out var core))
                {
                    CoreConfigurationValidator.ValidateCore(core, errors);
                }
                if(ValidatorHelpers.TryGetObject(root, "communication", errors, out var communication))
                {
                    CommunicationConfigurationValidator.ValidateCommunication(communication, errors);
                }
            }
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
}