using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.OpenApi;
using WorkOrderApplication.API.Data;
using WorkOrderApplication.API.Endpoints;
using FluentValidation;
using FluentValidation.AspNetCore;
using WorkOrderApplication.API.Validators;
using WorkOrderApplication.API.Configurations;
using WorkOrderApplication.API.Services;
using WorkOrderApplication.API.Dtos;
using WorkOrderApplication.API.Hubs;
using Serilog;
using Microsoft.Extensions.Options;
using WebPush;



var builder = WebApplication.CreateBuilder(args); // Create WebApplication builder
builder.WebHost.UseUrls("http://0.0.0.0:5034");

// --------------------------------- 🔧 Config & Logging ----------------------------------------------
SerilogConfiguration.ConfigureSerilog(builder);

// --------------------------------- 📦 Register Services ---------------------------------------------
builder.Services.AddSignalR(); // SignalR

var connString = builder.Configuration.GetConnectionString("WorkOrder"); //Add database context
builder.Services.AddNpgsql<AppDbContext>(connString, o => o.CommandTimeout(120)); // Use PostgreSQL Database with Timeout

builder.Services.AddFluentValidationAutoValidation(); // Enable automatic validation in ASP.NET Core
builder.Services.AddValidatorsFromAssemblyContaining<Program>(); // Register all validators in the assembly

// --------------------------------- 🧩 Custom Services -----------------------------------------------
builder.Services.Configure<OrderProxyOptions>(builder.Configuration.GetSection("OrderProxy")); // Config OrderProxyOptions
builder.Services.AddHttpClient<OrderProxyService>((sp, client) =>
{
    var options = sp.GetRequiredService<IOptions<OrderProxyOptions>>().Value;

    if (string.IsNullOrWhiteSpace(options.BaseUrl))
        throw new InvalidOperationException("❌ Missing OrderProxy:BaseUrl in appsettings.json");

    client.BaseAddress = new Uri(options.BaseUrl); // ✅ สำคัญที่สุด
    client.DefaultRequestHeaders.Clear();

    if (!string.IsNullOrEmpty(options.Token))
        client.DefaultRequestHeaders.Add("Authorization", options.Token);
});


// -------------------- Bind Config --------------------
builder.Services.Configure<VehicleProxyOptions>(
    builder.Configuration.GetSection("VehicleProxy"));

// -------------------- Register Service --------------------
builder.Services.AddHttpClient<VehicleProxyService>((sp, client) =>
{
    var options = sp.GetRequiredService<IOptions<VehicleProxyOptions>>().Value;

    client.BaseAddress = new Uri(options.BaseUrl);
    client.DefaultRequestHeaders.Accept.Add(
        new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

    // ✅ ใส่ Bearer Token
    if (!string.IsNullOrWhiteSpace(options.Token))
    {
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", options.Token.Replace("Bearer ", ""));
    }
});





builder.Services.AddHostedService<OrderRecordBackgroundService>(); // Background Service for OrderRecord
builder.Services.AddScoped<IValidator<OrderGroupRequestDto>, OrderGroupRequestDtoValidator>(); // Validator for OrderGroupRequestDto
builder.Services.AddHostedService<OrderRecordByIdBackgroundService>(); // Background Service for OrderRecordById
builder.Services.AddScoped<OrderProcessNotifier>(); // OrderProcess Notifier Service
builder.Services.AddHostedService<WorkOrderLineSyncService>(); // Sync DefaultLine for WorkOrders


// --------------------------------- 🏭 MES Services --------------------------------------------------
builder.Services.Configure<MesOptions>(
    builder.Configuration.GetSection("Mes"));

// Register HTTP clients and custom clients
builder.Services.AddHttpClient<MesTdcClient>();
builder.Services.AddScoped(sp =>
{
    var opt = sp.GetRequiredService<IOptions<MesOptions>>().Value;
    var http = sp.GetRequiredService<HttpClient>();
    return new MesTdcClient(http, opt);
});

// Register HTTP clients and custom clients
builder.Services.AddHttpClient<MesQueryClient>();
builder.Services.AddScoped(sp =>
{
    var opt = sp.GetRequiredService<IOptions<MesOptions>>().Value;
    var http = sp.GetRequiredService<HttpClient>();
    return new MesQueryClient(http, opt);
});

// ------------------------------------ Add Swagger -------------------------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("workorders", new() { Title = "WorkOrder API", Version = "v1" });
});



// ------------------------------------ Build App ---------------------------------------------------
var app = builder.Build();

// ----------------------------------- Add Swagger --------------------------------------------------
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/workorders/swagger.json", "WorkOrder API v1");
    c.RoutePrefix = string.Empty; // serve the Swagger UI at the app's root
});

// ----------------------------------- 📡 Minimal API ------------------------------------------------------
app.MapGroup("/api/workorders").WithTags("WorkOrders").MapWorkOrderEndpoints(); // WorkOrders
app.MapGroup("/api/materials").WithTags("Materials").MapMaterialEndpoints(); // Materials
app.MapGroup("/api/users").WithTags("Users").MapUserEndpoints(); // Users
app.MapGroup("/api/orderprocesses/{orderProcessId:int}/confirmprocesses").WithTags("ConfirmProcesses").MapConfirmProcessEndpoints(); // ConfirmProcesses
app.MapGroup("/api/orderprocesses").WithTags("OrderProcesses").MapOrderProcessEndpoints(); // OrderProcesses
app.MapGroup("/api/ordermaterials").WithTags("OrderMaterials").MapOrderMaterialEndpoints(); // OrderMaterials
app.MapGroup("/api/preparingprocesses").WithTags("PreparingProcesses").MapPreparingProcessEndpoints(); // PreparingProcesses
app.MapGroup("/api/preparingmaterials").WithTags("PreparingMaterials").MapPreparingMaterialEndpoints(); // PreparingMaterials
app.MapGroup("/api/shipmentprocesses").WithTags("ShipmentProcesses").MapShipmentProcessEndpoints(); // ShippingProcesses
app.MapGroup("/api/receivedprocesses").WithTags("ReceivedProcesses").MapReceivedProcessEndpoints(); // ReceivedProcesses
app.MapGroup("/api/receivedmaterials").WithTags("ReceivedMaterials").MapReceivedMaterialEndpoints(); // ReceivedMaterials
app.MapGroup("/api/returnprocesses").WithTags("ReturnProcesses").MapReturnProcessEndpoints(); // ReturnProcesses
app.MapGroup("/api/cancelledprocesses").WithTags("CancelProcesses").MapCancelledProcessEndpoints(); // CancelProcesses
app.MapGroup("/api/ordergroup").WithTags("OrderGroup").MapOrderProxyEndpoints(); // RIOT OrderGroup
app.MapGroup("/api/orderGroupAMR").WithTags("OrderGroupAMR").MapOrderGroupAMREndpoints(); // RIOT OrderGroupAMR
app.MapGroup("/api/vehicleProxy").WithTags("VehicleProxy").MapVehicleProxyEndpoints(); // Vehicle Proxy
app.MapGroup("/api/mes").WithTags("MES").MapMesEndpoints(); // MES Endpoints







app.MapHub<OrderRecordHub>("/hubs/orderRecord"); // OrderRecord Hub
app.MapHub<ShipmentProcessHub>("/hubs/shipmentProcess"); // ShipmentProcess Hub
app.MapHub<OrderProcessHub>("/hubs/orderProcess"); // OrderProcess Hub

// ------------------------------- Migration Database ------------------------------------
await app.MigrateDb();

app.Run();
