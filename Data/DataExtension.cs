using Microsoft.EntityFrameworkCore;
namespace WorkOrderApplication.API.Data;

public static class DataExtension
{
    public static async Task MigrateDb(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.MigrateAsync();
    }
}
