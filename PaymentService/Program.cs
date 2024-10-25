using Microsoft.OpenApi.Models;
using PaymentService.Inerfaces;
using PaymentService.Repositories;
using PaymentService.Services;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
{
    string bearerToken = Environment.GetEnvironmentVariable("ApiKey") ?? string.Empty;

    builder.Services.AddHttpClient<IClient, Client>(c =>
    {
        c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        c.Timeout = TimeSpan.FromSeconds(30);
    }).SetHandlerLifetime(TimeSpan.FromMinutes(5));
    
    builder.Services.AddSingleton<IBatchService, BatchService>();
    builder.Services.AddSingleton<IBonusRepository, BonusRepository>();
    
    builder.Services.AddControllers();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Payment Processing Service", Version = "v1" });
    });
}

var app = builder.Build();
{
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Payment Processing Service v1"));

    app.UseHttpsRedirection();

    app.UseRouting();

    app.UseAuthorization();

    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
    });
}

app.Run();