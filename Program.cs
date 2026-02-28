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
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;

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

// --------------------------------- 🔐 Security & Auth Services --------------------------------------
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtOptions>()!;
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
        };
    });

builder.Services.AddAuthorization();


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

    // JWT Swagger setup
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer {token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
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

app.UseAuthentication();
app.UseAuthorization();

// ----------------------------------- 📡 Minimal API ------------------------------------------------------
app.MapGroup("/api/workorders").WithTags("WorkOrders").RequireAuthorization().MapWorkOrderEndpoints(); // WorkOrders
app.MapGroup("/api/materials").WithTags("Materials").RequireAuthorization().MapMaterialEndpoints(); // Materials
app.MapGroup("/api/users").WithTags("Users").RequireAuthorization().MapUserEndpoints(); // Users
app.MapGroup("/api/orderprocesses/{orderProcessId:int}/confirmprocesses").WithTags("ConfirmProcesses").RequireAuthorization().MapConfirmProcessEndpoints();
app.MapGroup("/api/orderprocesses").WithTags("OrderProcesses").RequireAuthorization().MapOrderProcessEndpoints(); // OrderProcesses
app.MapGroup("/api/ordermaterials").WithTags("OrderMaterials").RequireAuthorization().MapOrderMaterialEndpoints(); // OrderMaterials
app.MapGroup("/api/preparingprocesses").WithTags("PreparingProcesses").RequireAuthorization().MapPreparingProcessEndpoints(); // PreparingProcesses
app.MapGroup("/api/preparingmaterials").WithTags("PreparingMaterials").RequireAuthorization().MapPreparingMaterialEndpoints(); // PreparingMaterials
app.MapGroup("/api/orderprocesses/{orderProcessId:int}/shipmentprocess").WithTags("ShipmentProcesses").RequireAuthorization().MapShipmentProcessEndpoints(); // ShipmentProcesses (nested)
app.MapGroup("/api/receivedprocesses").WithTags("ReceivedProcesses").RequireAuthorization().MapReceivedProcessEndpoints(); // ReceivedProcesses
app.MapGroup("/api/receivedmaterials").WithTags("ReceivedMaterials").RequireAuthorization().MapReceivedMaterialEndpoints(); // ReceivedMaterials
app.MapGroup("/api/returnprocesses").WithTags("ReturnProcesses").RequireAuthorization().MapReturnProcessEndpoints(); // ReturnProcesses
app.MapGroup("/api/cancelledprocesses").WithTags("CancelProcesses").RequireAuthorization().MapCancelledProcessEndpoints(); // CancelProcesses
app.MapGroup("/api/ordergroup").WithTags("OrderGroup").RequireAuthorization().MapOrderProxyEndpoints(); // RIOT OrderGroup
app.MapGroup("/api/orderGroupAMR").WithTags("OrderGroupAMR").RequireAuthorization().MapOrderGroupAMREndpoints(); // RIOT OrderGroupAMR
app.MapGroup("/api/vehicleProxy").WithTags("VehicleProxy").RequireAuthorization().MapVehicleProxyEndpoints(); // Vehicle Proxy
app.MapGroup("/api/mes").WithTags("MES").RequireAuthorization().MapMesEndpoints(); // MES Endpoints
app.MapGroup("/api/auth").WithTags("Auth").MapAuthEndpoints(); // Auth Endpoints







app.MapHub<OrderProcessHub>("/hubs/orderProcess"); // OrderProcess Hub

// ------------------------------- Migration Database ------------------------------------
await app.MigrateDb();

app.Run();
