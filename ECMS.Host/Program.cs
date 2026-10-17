using ECMS.Collection;
using ECMS.Contracts;
using ECMS.Contracts.Configuration;
using ECMS.Host.Configuration;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.SetBasePath(AppContext.BaseDirectory);

const string configFileName = "config.json";
var configFilePath = Path.Combine(AppContext.BaseDirectory, configFileName);

if (!builder.Services.TryAddConfiguration(configFilePath, out var errors))
{
    // StartupFileLogger.LogErrors(errors);

    Environment.ExitCode = 1;
    return;
}

// builder.Services.AddDeviceCollection(collectionConfiguration);

var host = builder.Build();
host.Run();