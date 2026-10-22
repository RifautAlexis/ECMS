using System.Threading.Channels;
using ECMS.Contracts;
using ECMS.Contracts.Configuration;
using ECMS.Contracts.Hosting;
using Microsoft.Extensions.Logging;

namespace ECMS.Collection;

internal sealed class CollectionWorker : ManagedWorker
{
    private readonly CollectionConfiguration _settings;
    private readonly ChannelWriter<CollectedData> _writer;

    public override WorkerStage Stage => WorkerStage.Collection;

    public CollectionWorker(
        CollectionConfiguration settings,
        ChannelWriter<CollectedData> writer,
        ILogger<CollectionWorker> logger)
        : base(logger)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(writer);

        _settings = settings;
        _writer = writer;

        Logger.LogInformation(
            $"{nameof(CollectionWorker)} created with settings: {{Settings}}",
            _settings);
    }

    protected override async Task InitializeAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(
            TimeSpan.FromSeconds(5),
            stoppingToken);
        // return Task.CompletedTask;
    }

    protected override Task ExecuteWorkerAsync(CancellationToken stoppingToken)
    {
        return Task.Delay(
            Timeout.InfiniteTimeSpan,
            stoppingToken);
    }
}
