using ECMS.Contracts.Configuration;

namespace ECMS.Collection.Adapters;

public interface IProtocolAdapter
{
    string Protocol { get; }

    Task<string> CollectAsync(
        DeviceConfiguration device,
        CancellationToken cancellationToken);
}
