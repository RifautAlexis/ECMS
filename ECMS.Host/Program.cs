using System.Threading.Channels;
using ECMS.Collection;
using ECMS.Communication;
using ECMS.Contracts;
using ECMS.Core;
using ECMS.Host;
using ECMS.Host.Configuration;
using ECMS.Host.Logging;
using Serilog;

const int channelCapacity = 1024; // TODO: move to config.json

try
{
    Log.Logger = LoggingSetup.CreateLogger();

    Log.Information("Starting ECMS");

    var builder = Host.CreateApplicationBuilder(
        new HostApplicationBuilderSettings
        {
            Args = args,
            ContentRootPath = AppContext.BaseDirectory
        });

    builder.Services.Configure<HostOptions>(
        builder.Configuration.GetSection("HostOptions"));

    builder.Logging.ClearProviders();
    builder.Services.AddSerilog();

    try
    {
        // Validate configuration before registering runtime components
        builder.Services.ValidateAndAddConfiguration();

        // Shared pipeline: Collection writes, Core reads
        var channel = Channel.CreateBounded<CollectionResult>(
            new BoundedChannelOptions(channelCapacity)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = true,
                SingleWriter = true
            });

        builder.Services.AddSingleton(channel.Reader);
        builder.Services.AddSingleton(channel.Writer);

        // Startup order comes from each worker's Stage; shutdown runs in reverse.
        builder.Services.AddCoreWorker();
        builder.Services.AddCommunicationWorker();
        builder.Services.AddCollectionWorker();
        builder.Services.AddHostedService<EcmsLifecycleService>();
    }
    catch (ConfigurationValidationException ex)
    {
        foreach (var error in ex.Errors)
            Log.Error(
                "Configuration validation failed: {Error}",
                error);

        Environment.ExitCode = 1;
        return;
    }

    var host = builder.Build();

    Log.Information("ECMS host built successfully");

    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(
        ex,
        "ECMS terminated unexpectedly");
    Environment.ExitCode = 1;
}
finally
{
    Log.CloseAndFlush();
}
