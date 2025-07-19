global using FastEndpoints;
global using Serilog;
using HealthChecks.UI.Client;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Pragmatic.Configuration;
using Pragmatic.Helpers;


bool tokenExpired = false;
Environment.SetEnvironmentVariable("BASEDIR", AppContext.BaseDirectory);

var builder = WebApplication.CreateBuilder(args);


var logDirectory = Path.Combine(AppContext.BaseDirectory, "logs");
if (!Directory.Exists(logDirectory))
{
    Directory.CreateDirectory(logDirectory); // Create the directory if it doesn't exist
}

// Logging
builder.Host.UseSerilog((context, configuration) =>
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .WriteTo.Console()  // Logs to the console
        .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day) // Logs to a file, rolling every day
);


builder.Services.Configure<PragmaticApiSettings>(
    builder.Configuration.GetSection("PragmaticApi"));

builder.Services.Configure<PragmaticAssetSettings>(
    builder.Configuration.GetSection("PragmaticAsset"));


// Add FastEndpoints and HttpClient
builder.Services.AddFastEndpoints();
builder.Services.AddHttpClient();

// Optional Health Checks (just basic ping checks)
builder.Services.AddHealthChecks();

// Optional CORS (development only, open access for now)
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "DevelopmentPolicy",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

var app = builder.Build();

// Middlewares
app.UseSerilogRequestLogging();

app.UseCors("DevelopmentPolicy");

// Health Checks endpoint
app.UseHealthChecks("/_health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

// FastEndpoints Configuration
var settings = new JsonSerializerSettings
{
    ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver(),

};

app.UseFastEndpoints(c =>
{
    c.Endpoints.RoutePrefix = "pragmatic";

    c.Serializer.ResponseSerializer = (rsp, dto, cType, jCtx, ct) =>
    {
        rsp.ContentType = cType;
        return rsp.WriteAsync(JsonConvert.SerializeObject(dto, settings), ct);
    };
});

app.Run();