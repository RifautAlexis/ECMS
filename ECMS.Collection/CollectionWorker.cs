using ECMS.Contracts.Hosting;

namespace ECMS.Collection;

internal sealed class CollectionWorker : ManagedWorker
{
    public override WorkerStage Stage => WorkerStage.Collection;

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
