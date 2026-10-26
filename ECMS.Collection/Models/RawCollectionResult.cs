using ECMS.Contracts.Configuration;

namespace ECMS.Collection.Models;

internal sealed record RawCollectionResult(
    DeviceConfiguration Device,
    DateTimeOffset Timestamp,
    string? Payload,
    string? Error);
