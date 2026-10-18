namespace ECMS.Host.Configuration;

public sealed class ConfigurationValidationException : Exception
{
    public IReadOnlyList<string> Errors { get; }

    public ConfigurationValidationException(IEnumerable<string> errors) : base("The ECMS configuration is invalid.")
    {
        Errors = errors.ToArray();
    }
}