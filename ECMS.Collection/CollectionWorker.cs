using System.Threading.Channels;
using ECMS.Contracts;
using ECMS.Contracts.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ECMS.Collection;

internal sealed class CollectionWorker : BackgroundService
{
    private readonly IReadOnlyList<IMockDevice> _devices;
    private readonly ChannelWriter<CollectedData> _writer;
    private readonly TimeSpan _pollingInterval;
    private readonly ILogger<CollectionWorker> _logger;

    public CollectionWorker(
        CollectionConfiguration configuration,
        ChannelWriter<CollectedData> writer,
        ILogger<CollectionWorker> logger)
    {
        _devices = configuration.DevicesConfiguration
            .Select(MockDeviceFactory.Create)
            .ToArray();
        _writer = writer;
        _pollingInterval = TimeSpan.FromSeconds(configuration.PollingIntervalSeconds);
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Starting collection for {DeviceCount} devices with a {PollingInterval} second interval.",
            _devices.Count,
            _pollingInterval.TotalSeconds);

        Exception? completionError = null;
        try
        {
            using var timer = new PeriodicTimer(_pollingInterval);
            do
            {
                foreach (var device in _devices)
                {
                    await _writer.WriteAsync(device.Collect(), stoppingToken);
                }
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            completionError = exception;
            throw;
        }
        finally
        {
            _writer.TryComplete(completionError);
        }
    }
}
