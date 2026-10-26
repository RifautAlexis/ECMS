using System.Threading.Channels;
using System.Text.Json;
using ECMS.Collection.Adapters;
using ECMS.Collection.Mapping;
using ECMS.Collection.Models;
using ECMS.Contracts;
using ECMS.Contracts.Configuration;
using Microsoft.Extensions.Logging;

namespace ECMS.Collection.Orchestration;

public sealed class CollectionOrchestrator
{
    private readonly IDeviceConfigurationUpdateSource _deviceUpdates;
    private readonly IReadOnlyDictionary<string, IProtocolAdapter> _adapters;
    private readonly IDeviceDataMapper _mapper;
    private readonly ChannelWriter<CollectionResult> _resultWriter;
    private readonly ILogger<CollectionOrchestrator> _logger;
    private readonly TimeSpan _pollingInterval;
    private readonly SemaphoreSlim _requestSlots;
    private readonly Channel<RawCollectionResult> _rawResults =
        Channel.CreateBounded<RawCollectionResult>(
            new BoundedChannelOptions(128)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = true,
                SingleWriter = false
            });

    public CollectionOrchestrator(
        IDeviceConfigurationUpdateSource deviceUpdates,
        IEnumerable<IProtocolAdapter> adapters,
        IDeviceDataMapper mapper,
        ChannelWriter<CollectionResult> resultWriter,
        CollectionConfiguration configuration,
        ILogger<CollectionOrchestrator> logger)
    {
        ArgumentNullException.ThrowIfNull(deviceUpdates);
        ArgumentNullException.ThrowIfNull(adapters);
        ArgumentNullException.ThrowIfNull(mapper);
        ArgumentNullException.ThrowIfNull(resultWriter);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(logger);

        _deviceUpdates = deviceUpdates;
        _adapters = adapters.ToDictionary(
            adapter => adapter.Protocol,
            StringComparer.OrdinalIgnoreCase);
        _mapper = mapper;
        _resultWriter = resultWriter;
        _logger = logger;
        _pollingInterval = TimeSpan.FromSeconds(configuration.PollingIntervalSeconds);
        if (configuration.MaxConcurrentRequests <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(configuration),
                "Maximum concurrent requests must be positive.");
        _requestSlots = new SemaphoreSlim(configuration.MaxConcurrentRequests);
    }

    public async Task RunAsync(CancellationToken stoppingToken)
    {
        var loops = new Dictionary<int, DeviceLoop>();
        
        var processor = ProcessRawResultsAsync(stoppingToken);
        await using var updates = _deviceUpdates.ReadUpdatesAsync(stoppingToken)
            .GetAsyncEnumerator(stoppingToken);
        var nextUpdate = updates.MoveNextAsync().AsTask();

        try
        {
            while (true)
            {
                var activeLoops = loops.Values.Select(loop => loop.Task).ToArray();
                var loopCompletion = activeLoops.Length == 0
                    ? null
                    : Task.WhenAny(activeLoops);
                var completed = loopCompletion is null
                    ? await Task.WhenAny(nextUpdate, processor)
                    : await Task.WhenAny(nextUpdate, processor, loopCompletion);

                if (completed == processor)
                {
                    await processor;
                    throw new InvalidOperationException("Collection result processing stopped unexpectedly.");
                }

                if (loopCompletion is not null && completed == loopCompletion)
                {
                    var endedLoop = await loopCompletion;
                    var currentLoop = loops.FirstOrDefault(pair => ReferenceEquals(pair.Value.Task, endedLoop));
                    if (currentLoop.Value is not null)
                    {
                        await endedLoop;
                        throw new InvalidOperationException(
                            $"Polling loop for device {currentLoop.Key} stopped unexpectedly.");
                    }

                    continue;
                }

                if (!await nextUpdate)
                    break;

                await ReconcileAsync(
                    updates.Current,
                    loops,
                    stoppingToken);
                nextUpdate = updates.MoveNextAsync().AsTask();
            }
        }
        finally
        {
            foreach (var loop in loops.Values)
                loop.Cancellation.Cancel();

            foreach (var loop in loops.Values)
            {
                try
                {
                    await loop.Task;
                }
                catch (OperationCanceledException) when (loop.Cancellation.IsCancellationRequested)
                {
                }
                finally
                {
                    loop.Cancellation.Dispose();
                }
            }

            _rawResults.Writer.TryComplete();

            try
            {
                await processor;
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
            }
        }
    }

    private async Task ReconcileAsync(
        IReadOnlyList<DeviceConfiguration> desiredDevices,
        Dictionary<int, DeviceLoop> loops,
        CancellationToken stoppingToken)
    {
        var desired = desiredDevices.ToDictionary(device => device.Id);

        foreach (var current in loops.ToArray())
        {
            if (desired.TryGetValue(current.Key, out var configuration) &&
                configuration == current.Value.Configuration)
                continue;

            current.Value.Cancellation.Cancel();
            try
            {
                await current.Value.Task;
            }
            catch (OperationCanceledException) when (current.Value.Cancellation.IsCancellationRequested)
            {
            }
            finally
            {
                current.Value.Cancellation.Dispose();
            }

            loops.Remove(current.Key);
        }

        foreach (var device in desired.Values)
        {
            stoppingToken.ThrowIfCancellationRequested();
            if (loops.ContainsKey(device.Id))
                continue;

            if (!_adapters.TryGetValue(device.Protocol, out var adapter))
                throw new InvalidOperationException(
                    $"No protocol adapter is registered for '{device.Protocol}'.");

            var cancellation = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
            loops.Add(
                device.Id,
                new DeviceLoop(
                    device,
                    cancellation,
                    PollDeviceAsync(device, adapter, cancellation.Token)));
        }
    }

    private async Task PollDeviceAsync(
        DeviceConfiguration device,
        IProtocolAdapter adapter,
        CancellationToken stoppingToken)
    {
        while (true)
        {
            var timestamp = DateTimeOffset.UtcNow;
            using var requestTimeout = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
            requestTimeout.CancelAfter(TimeSpan.FromSeconds(device.RequestTimeoutSeconds));

            RawCollectionResult result;
            try
            {
                await _requestSlots.WaitAsync(requestTimeout.Token);
                try
                {
                    var payload = await adapter.CollectAsync(device, requestTimeout.Token);
                    result = new RawCollectionResult(device, timestamp, payload, null);
                }
                finally
                {
                    _requestSlots.Release();
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                var error = requestTimeout.IsCancellationRequested
                    ? $"Request timed out after {device.RequestTimeoutSeconds} seconds."
                    : exception.Message;

                _logger.LogWarning(
                    exception,
                    "Collection request failed for device {DeviceId}: {Error}",
                    device.Id,
                    error);

                result = new RawCollectionResult(device, timestamp, null, error);
            }

            await _rawResults.Writer.WriteAsync(result, stoppingToken);
            await Task.Delay(_pollingInterval, stoppingToken);
        }
    }

    private async Task ProcessRawResultsAsync(CancellationToken stoppingToken)
    {
        await foreach (var raw in _rawResults.Reader.ReadAllAsync(stoppingToken))
        {
            CollectionResult result;
            if (raw.Error is not null)
            {
                result = CollectionResult.Failure(
                    raw.Device.Id,
                    raw.Device.Kind,
                    raw.Timestamp,
                    raw.Error);
            }
            else
            {
                try
                {
                    var data = new CollectedData(
                        raw.Device.Id,
                        raw.Device.Kind,
                        _mapper.Map(raw.Device.Kind, raw.Payload!));
                    result = CollectionResult.Success(data, raw.Timestamp);
                }
                catch (Exception exception) when (exception is JsonException or NotSupportedException)
                {
                    result = CollectionResult.Failure(
                        raw.Device.Id,
                        raw.Device.Kind,
                        raw.Timestamp,
                        exception.Message);
                }
            }

            await _resultWriter.WriteAsync(result, stoppingToken);
        }
    }

    private sealed record DeviceLoop(
        DeviceConfiguration Configuration,
        CancellationTokenSource Cancellation,
        Task Task);
}
