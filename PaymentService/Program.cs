using Microsoft.OpenApi.Models;
using PaymentService;
using PaymentService.Interfaces;
using PaymentService.Repositories;
using PaymentService.Services;
using Polly;
using Polly.Contrib.WaitAndRetry;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
{
    builder.Services.AddHttpClient<IClient, Client>(c =>
    {
        c.Timeout = TimeSpan.FromSeconds(30);
    }).SetHandlerLifetime(TimeSpan.FromMinutes(5)).AddPolicyHandler(GetRetryPolicy());
    
    builder.Services.AddSingleton<IBatchService, BatchService>();
    builder.Services.AddSingleton<IBonusRepository, BonusRepository>();
    builder.Services.AddSingleton<ICustomerRepository, CustomerRepository>();
    builder.Services.AddSingleton<IPayQuickerService, PayQuickerService>();
    builder.Services.AddSingleton<BatchQueue>();

    builder.Services.AddHostedService<BatchProcessingService>();

    builder.Services.AddControllers();

    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Payment Processing Service", Version = "v1" });
    });

    var config = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables()
        .Build();
}

var app = builder.Build();
{
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Payment Processing Service v1"));

    app.MapGet("/", () =>
    {
        var assemblyVersion = "1.2.0.0";
        var runtimeVersion = RuntimeInformation.FrameworkDescription;

        return $"Ver: {assemblyVersion}, Runtime: {runtimeVersion}";
    });

    app.MapGet("/debug/ip", async () =>
    {
        using var httpClient = new HttpClient();
        var ip = await httpClient.GetStringAsync("https://api.ipify.org");

        var liveIdentityUrl = Environment.GetEnvironmentVariable("LiveIdentityUrl") ?? string.Empty;
        var liveBaseUrl = Environment.GetEnvironmentVariable("LiveBaseUrl") ?? string.Empty;

        var sandboxIdentityUrl = Environment.GetEnvironmentVariable("SandboxIdentityUrl") ?? string.Empty;
        var sandboxBaseUrl = Environment.GetEnvironmentVariable("SandboxBaseUrl") ?? string.Empty;

        var commissionRootUrl = Environment.GetEnvironmentVariable("PillarsApiUrl") ?? string.Empty;

        return $"Live Identity: {liveIdentityUrl}\r\nLive Base:{liveBaseUrl}\r\nSandbox Identity:{sandboxIdentityUrl}\r\nSandbox Base:{sandboxBaseUrl}\r\nPillars Url:{commissionRootUrl}\r\nEgress IP: {ip}";
    });

    app.MapControllers();
}

app.Run();




static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    var delay = Backoff.DecorrelatedJitterBackoffV2(medianFirstRetryDelay: TimeSpan.FromSeconds(1), retryCount: 5);

    return Policy<HttpResponseMessage>
        .Handle<HttpRequestException>()
        .OrResult(response =>
            (int)response.StatusCode >= 500
            || response.StatusCode == HttpStatusCode.RequestTimeout
            || response.StatusCode == HttpStatusCode.TooManyRequests)
        .WaitAndRetryAsync(delay);
}