using System.Threading.Channels;
using ECMS.Contracts;
using ECMS.Contracts.Hosting;
using Microsoft.Extensions.Logging;

namespace ECMS.Core;

internal sealed class CoreWorker : ManagedWorker
{
    private readonly ChannelReader<CollectionResult> _reader;

    public CoreWorker(
        ChannelReader<CollectionResult> reader,
        ILogger<CoreWorker> logger) : base(logger)
    {
        _reader = reader;
    }

    public override WorkerStage Stage => WorkerStage.Core;

    protected override async Task InitializeAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(
            TimeSpan.FromSeconds(5),
            stoppingToken);
        // return Task.CompletedTask;
    }

    protected override async Task ExecuteWorkerAsync(CancellationToken stoppingToken)
    {
        await foreach (var result in _reader.ReadAllAsync(stoppingToken))
        {
            if (result.IsSuccess)
                Logger.LogDebug(
                    "Collected data from device {DeviceId} (kind {Kind}) at {Timestamp}",
                    result.DeviceId,
                    result.Kind,
                    result.Timestamp);
            else
                Logger.LogWarning(
                    "Collection failed for device {DeviceId} (kind {Kind}): {Error}",
                    result.DeviceId,
                    result.Kind,
                    result.Error);
        }
    }
}
