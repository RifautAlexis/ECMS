using ECMS.Contracts.Hosting;

namespace ECMS.Host;

public sealed class EcmsLifecycleService : IHostedService
{
    private static readonly TimeSpan WorkerStopTimeout = TimeSpan.FromSeconds(10);

    private readonly IReadOnlyList<ManagedWorker> _workers;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly ILogger<EcmsLifecycleService> _logger;
    private readonly List<ManagedWorker> _startedWorkers = [];
    private readonly CancellationTokenSource _stopping = new();
    private Task? _monitorTask;

    public EcmsLifecycleService(
        IEnumerable<ManagedWorker> workers,
        IHostApplicationLifetime lifetime,
        ILogger<EcmsLifecycleService> logger)
    {
        _workers = workers.OrderBy(worker => worker.Stage)
            .ToArray();

        var duplicate = _workers
            .GroupBy(worker => worker.Stage)
            .FirstOrDefault(group => group.Count() > 1);

        if (duplicate is not null)
            throw new InvalidOperationException($"Multiple workers are registered for stage {duplicate.Key}.");

        _lifetime = lifetime;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            foreach (var worker in _workers)
                await StartWorkerAsync(
                    worker,
                    cancellationToken);

            _monitorTask = MonitorWorkersAsync();
        }
        catch
        {
            // Cleanup must not replace the original startup failure or cancellation.
            try
            {
                await StopStartedWorkersAsync(CancellationToken.None);
            }
            catch (Exception shutdownException)
            {
                _logger.LogError(
                    shutdownException,
                    "One or more workers failed to stop after startup failed.");
            }

            throw;
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            await StopStartedWorkersAsync(cancellationToken);
        }
        finally
        {
            // The monitor ends as soon as shutdown is signalled and never throws.
            if (_monitorTask is not null)
                await _monitorTask;
        }
    }

    private async Task StartWorkerAsync(
        ManagedWorker worker,
        CancellationToken cancellationToken)
    {
        var workerName = worker.GetType()
            .Name;

        _logger.LogInformation(
            "Starting worker {WorkerName}",
            workerName);

        _startedWorkers.Add(worker);

        await worker.StartAsync(cancellationToken);

        await WaitUntilReadyAsync(
            worker,
            cancellationToken);

        _logger.LogInformation(
            "Worker {WorkerName} is ready",
            workerName);
    }

    // Waits for readiness while watching every started worker, so a failure
    // (including the starting worker itself) aborts startup immediately.
    private async Task WaitUntilReadyAsync(
        ManagedWorker worker,
        CancellationToken cancellationToken)
    {
        var ready = worker.Ready.WaitAsync(cancellationToken);

        var watched = _startedWorkers
            .Select(GetExecuteTask)
            .Append(ready)
            .ToArray();

        var first = await Task.WhenAny(watched);

        if (ReferenceEquals(
            first,
            ready))
        {
            await ready;
            return;
        }

        // Propagates the failure or cancellation of the worker that stopped.
        await first;

        throw new InvalidOperationException(
            "A worker stopped unexpectedly while ECMS was starting.");
    }

    private static Task GetExecuteTask(ManagedWorker worker)
    {
        return worker.ExecuteTask
         ?? throw new InvalidOperationException(
                $"Worker {worker.GetType().Name} did not create an execution task.");
    }

    // Never faults: every failure is logged and turned into an application stop.
    private async Task MonitorWorkersAsync()
    {
        try
        {
            var workerTasks = _startedWorkers
                .Select(GetExecuteTask)
                .ToArray();

            if (workerTasks.Length == 0)
                return;

            var stopSignal = Task.Delay(
                Timeout.Infinite,
                _stopping.Token);

            var completedTask = await Task.WhenAny(workerTasks.Append(stopSignal));

            if (_stopping.IsCancellationRequested || _lifetime.ApplicationStopping.IsCancellationRequested)
                return;

            try
            {
                await completedTask;

                _logger.LogError(
                    "A critical ECMS worker stopped unexpectedly.");
            }
            catch (Exception ex)
            {
                _logger.LogCritical(
                    ex,
                    "A critical ECMS worker failed.");
            }

            _lifetime.StopApplication();
        }
        catch (Exception ex)
        {
            _logger.LogCritical(
                ex,
                "ECMS worker monitoring failed.");

            _lifetime.StopApplication();
        }
    }

    private async Task StopStartedWorkersAsync(
        CancellationToken cancellationToken)
    {
        // Signal first so the monitor does not treat worker exits as failures.
        _stopping.Cancel();

        List<Exception>? errors = null;

        foreach (var worker in _startedWorkers.AsEnumerable()
            .Reverse())
        {
            var workerName = worker.GetType()
                .Name;

            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(WorkerStopTimeout);

            try
            {
                _logger.LogInformation(
                    "Stopping worker {WorkerName}",
                    workerName);

                await worker.StopAsync(timeout.Token);

                // BackgroundService.StopAsync returns silently when its token is cancelled.
                if (worker.ExecuteTask is { IsCompleted: false })
                    throw new TimeoutException($"Worker {workerName} did not stop within the allowed time.");
            }
            catch (Exception ex)
            {
                // Keep stopping the remaining workers.
                errors ??= [];
                errors.Add(ex);

                if (ex is OperationCanceledException or TimeoutException)
                    _logger.LogError(
                        "Worker {WorkerName} did not stop within the allowed time.",
                        workerName);
                else
                    _logger.LogError(
                        ex,
                        "Error while stopping worker {WorkerName}",
                        workerName);
            }
        }

        _startedWorkers.Clear();

        if (errors is not null)
            throw new AggregateException(
                "One or more workers failed to stop.",
                errors);
    }
}
