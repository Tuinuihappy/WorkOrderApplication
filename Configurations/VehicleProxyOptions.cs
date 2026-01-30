namespace WorkOrderApplication.API.Configurations;

public class VehicleProxyOptions
{
    public string BaseUrl { get; set; } = string.Empty;
    public string PassEndpoint { get; set; } = "pass";
    public string Token { get; set; } = string.Empty;
}
