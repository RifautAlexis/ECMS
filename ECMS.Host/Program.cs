using ECMS.Host.Configuration;
using ECMS.Host.Logging;
using Serilog;

try
{
    Log.Logger = LoggingSetup.CreateLogger();

    Log.Information("Starting ECMS");
    
    var builder = Host.CreateApplicationBuilder(args);

    builder.Logging.ClearProviders();
    builder.Services.AddSerilog();

    builder.Configuration.SetBasePath(AppContext.BaseDirectory);

    try
    {
        builder.Services.AddConfiguration();
    }
    catch (ConfigurationValidationException ex)
    {
        foreach (var error in ex.Errors)
        {
            Log.Error("Configuration validation failed: {Error}", error);
        }

        Environment.ExitCode = 1;
        return;
    }

    // builder.Services.AddDeviceCollection(collectionConfiguration);

    var host = builder.Build();

    Log.Information("ECMS host built successfully");
    
    host.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "ECMS terminated unexpectedly");
    Environment.ExitCode = 1;
}
finally
{
    Log.CloseAndFlush();
}