using ECMS.Contracts.Hosting;

namespace ECMS.Communication;

internal sealed class CommunicationWorker : ManagedWorker
{
    public override WorkerStage Stage => WorkerStage.Communication;

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
