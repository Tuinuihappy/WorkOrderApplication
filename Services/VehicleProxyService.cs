using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using WorkOrderApplication.API.Configurations;

namespace WorkOrderApplication.API.Services;

public class VehicleProxyService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<VehicleProxyService> _logger;
    private readonly VehicleProxyOptions _options;

    public VehicleProxyService(HttpClient httpClient, IOptions<VehicleProxyOptions> options, ILogger<VehicleProxyService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    // 🔹 POST: /pass?vehicleKey=...
    public async Task<string?> PassVehicleAsync(string vehicleKey)
    {
        var url = $"{_options.PassEndpoint}?vehicleKey={vehicleKey}";
        var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.Token.Replace("Bearer ", ""));

        try
        {
            _logger.LogInformation("[VehicleProxy] 🚀 Sending pass request for {VehicleKey}", vehicleKey);

            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("[VehicleProxy] ❌ Failed vehicle pass ({StatusCode}): {Body}", response.StatusCode, content);
                return $"Error: {response.StatusCode} | {content}";
            }

            _logger.LogInformation("[VehicleProxy] ✅ Pass successful for {VehicleKey}", vehicleKey);
            return content;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[VehicleProxy] ❌ Error calling pass endpoint");
            return null;
        }
    }
}
