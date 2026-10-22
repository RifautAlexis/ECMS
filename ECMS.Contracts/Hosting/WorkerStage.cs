namespace ECMS.Contracts.Hosting;

/// <summary>
///     Startup stage of a worker. Lower values start first and stop last,
///     so consumers start before the producers that feed them.
/// </summary>
public enum WorkerStage
{
    Core = 0,
    Communication = 1,
    Collection = 2
}
