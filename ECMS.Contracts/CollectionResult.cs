namespace ECMS.Contracts;

public sealed record CollectionResult(
    int DeviceId,
    int Kind,
    DateTimeOffset Timestamp,
    bool IsSuccess,
    CollectedData? Data,
    string? Error)
{
    public static CollectionResult Success(
        CollectedData data,
        DateTimeOffset timestamp)
    {
        ArgumentNullException.ThrowIfNull(data);

        return new CollectionResult(
            data.Id,
            data.Kind,
            timestamp,
            true,
            data,
            null);
    }

    public static CollectionResult Failure(
        int deviceId,
        int kind,
        DateTimeOffset timestamp,
        string error)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(error);

        return new CollectionResult(
            deviceId,
            kind,
            timestamp,
            false,
            null,
            error);
    }
}
