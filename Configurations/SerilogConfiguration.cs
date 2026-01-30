using Serilog;

namespace WorkOrderApplication.API.Configurations;

public static class SerilogConfiguration
{
    public static void ConfigureSerilog(WebApplicationBuilder builder)
    {
        // โหลดการตั้งค่าจาก appsettings.json + Environment-specific
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithThreadId()
            .CreateLogger();

        // ใช้ Serilog แทนระบบ logging ปกติ
        builder.Host.UseSerilog();
    }
}
