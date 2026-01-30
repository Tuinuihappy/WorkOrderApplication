using System;

namespace WorkOrderApplication.API.Configurations;

public class OrderProxyOptions
{
    public string BaseUrl { get; set; } = default!;
    public string Token { get; set; } = default!;
}
