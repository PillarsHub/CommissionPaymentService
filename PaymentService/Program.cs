using Microsoft.Extensions.Caching.Memory;
using Microsoft.OpenApi.Models;
using PaymentService.Interfaces;
using PaymentService.Repositories;
using PaymentService.Services;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.InteropServices;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
{
    string bearerToken = Environment.GetEnvironmentVariable("ApiKey") ?? builder.Configuration["ApiKey"] ?? String.Empty;

    builder.Services.AddHttpClient<IClient, Client>(c =>
    {
        c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        c.Timeout = TimeSpan.FromSeconds(30);
    }).SetHandlerLifetime(TimeSpan.FromMinutes(5));
    
    builder.Services.AddSingleton<IBatchService, BatchService>();
    builder.Services.AddSingleton<IBonusRepository, BonusRepository>();
    builder.Services.AddSingleton<ICustomerRepository, CustomerRepository>();
    builder.Services.AddControllers();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Payment Processing Service", Version = "v1" });
    });

    var config = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables()
        .Build();

    builder.Services.AddSingleton<IPayQuickerService>(_ => new PayQuickerService(builder.Configuration, new MemoryCache(new MemoryCacheOptions())));
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
        var assemblyVersion = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "Unknown";
        var runtimeVersion = RuntimeInformation.FrameworkDescription;

        return $"Ver: {assemblyVersion}, Runtime: {runtimeVersion}";
    });

    app.MapControllers();
}

app.Run();