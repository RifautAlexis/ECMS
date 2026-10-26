using ECMS.Contracts.Configuration;

namespace ECMS.Collection.Adapters;

public sealed class HttpAdapter : IProtocolAdapter
{
    private readonly HttpClient _httpClient;

    public HttpAdapter(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        _httpClient = httpClient;
    }

    public string Protocol => "Http";

    public async Task<string> CollectAsync(
        DeviceConfiguration device,
        CancellationToken cancellationToken)
    {
        var uri = new UriBuilder(
            Uri.UriSchemeHttp,
            device.IpAddress,
            device.Port).Uri;

        using var response = await _httpClient.GetAsync(
            uri,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }
}
