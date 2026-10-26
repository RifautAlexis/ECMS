using System.Threading.Channels;
using ECMS.Contracts;
using ECMS.Contracts.Hosting;
using ECMS.Collection.Orchestration;
using Microsoft.Extensions.Logging;

namespace ECMS.Collection;

internal sealed class CollectionWorker : ManagedWorker
{
    private readonly CollectionOrchestrator _orchestrator;
    private readonly ChannelWriter<CollectionResult> _writer;

    public override WorkerStage Stage => WorkerStage.Collection;

    public CollectionWorker(
        CollectionOrchestrator orchestrator,
        ChannelWriter<CollectionResult> writer,
        ILogger<CollectionWorker> logger)
        : base(logger)
    {
        ArgumentNullException.ThrowIfNull(orchestrator);
        ArgumentNullException.ThrowIfNull(writer);

        _orchestrator = orchestrator;
        _writer = writer;

        Logger.LogInformation("{WorkerName} created", nameof(CollectionWorker));
    }

    protected override Task InitializeAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }

    protected override async Task ExecuteWorkerAsync(CancellationToken stoppingToken)
    {
        try
        {
            await _orchestrator.RunAsync(stoppingToken);
        }
        finally
        {
            _writer.TryComplete();
        }
    }
}
