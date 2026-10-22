using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ECMS.Contracts.Hosting;

public abstract class ManagedWorker : BackgroundService
{
    private readonly TaskCompletionSource _ready = new(TaskCreationOptions.RunContinuationsAsynchronously);

    protected ILogger Logger { get; }

    protected ManagedWorker(ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(logger);

        Logger = logger;
    }

    public Task Ready => _ready.Task;

    public abstract WorkerStage Stage { get; }

    protected sealed override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await InitializeAsync(stoppingToken);
            _ready.TrySetResult();
            await ExecuteWorkerAsync(stoppingToken);
        }
        catch (Exception exception)
        {
            _ready.TrySetException(exception);
            throw;
        }
    }

    protected abstract Task InitializeAsync(CancellationToken stoppingToken);

    protected abstract Task ExecuteWorkerAsync(CancellationToken stoppingToken);
}
