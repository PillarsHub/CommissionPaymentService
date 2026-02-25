using Microsoft.OpenApi.Models;
using PaymentService;
using PaymentService.Interfaces;
using PaymentService.Repositories;
using PaymentService.Services;
using Polly;
using Polly.Contrib.WaitAndRetry;
using System.Net;
using System.Runtime.InteropServices;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
{
    builder.Services.AddHttpClient<IClient, Client>(c =>
    {
        c.Timeout = TimeSpan.FromSeconds(60);
    }).SetHandlerLifetime(TimeSpan.FromMinutes(5)).AddPolicyHandler(GetRetryPolicy());
    
    builder.Services.AddSingleton<ProgressStatusManager>();
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

    app.MapGet("/status", (ProgressStatusManager pgManager) =>
    {
        var items = pgManager.GetProgressStatus();

        return items.Select(x => new
        {
            x.Item1.Currency,
            x.Item1.BatchId,
            x.Item1.DetailId,
            x.Item1.PeriodId,
            ReleaseStatus = x.Item1.Status,
            x.Item1.StatusReason,
            ManagerStatus = x.Item2
        });
    });

    app.MapGet("/counts", (ProgressStatusManager pgManager) =>
    {
        // List<(long,(int,int))>
        var counts = pgManager.GetUpdateCount();

        // Return JSON-friendly shape
        return counts.Select(x => new
        {
            Id = x.Item1,              // the long
            Count1 = x.Item2.Item1,    // first int
            Count2 = x.Item2.Item2     // second int
        });
    });

    app.MapGet("/last-error", (ProgressStatusManager pgManager) =>
    {
        return Results.Ok(new
        {
            message = pgManager.GetLastErrorMessage()
        });
    });

    app.MapGet("/", () =>
    {
        var assemblyVersion = "1.3.0.0";
        var runtimeVersion = RuntimeInformation.FrameworkDescription;

        return Results.Content($$"""
<!DOCTYPE html>
<html>
<head>
    <title>Progress Status</title>
    <style>
        body { font-family: Arial; padding:20px; }

        table { border-collapse: collapse; width:100%; }
        th, td { border:1px solid #ccc; padding:6px; }
        th { background:#f4f4f4; }

        .meta { margin-bottom:14px; }

        .errorBox {
            margin: 10px 0 20px 0;
            padding: 10px;
            border-radius: 4px;
            background: #fff3f3;
            border: 1px solid #cc0000;
            color: #900;
            font-weight: bold;
        }

        .errorBox.ok {
            background: #f3fff3;
            border-color: #2a8a2a;
            color: #1f6f1f;
        }

        .counts { margin-bottom:20px; }
        .counts table { width:auto; min-width:420px; }
    </style>
</head>
<body>

<div class="meta">
  <h3>Version: {{assemblyVersion}}</h3>
  <h4>Runtime: {{runtimeVersion}}</h4>
</div>

<div id="errorBox" class="errorBox ok">
    No errors
</div>

<div class="counts">
  <h4>Counts</h4>
  <table>
    <thead>
      <tr>
        <th>Id</th>
        <th>Count 1</th>
        <th>Count 2</th>
      </tr>
    </thead>
    <tbody id="countsBody"></tbody>
  </table>
</div>

<table>
    <thead>
        <tr>
            <th>Currency</th>
            <th>BatchId</th>
            <th>DetailId</th>
            <th>PeriodId</th>
            <th>Release Status</th>
            <th>Status Reason</th>
            <th>Manager Status</th>
        </tr>
    </thead>
    <tbody id="tableBody"></tbody>
</table>

<script>
async function refreshStatus() {
    const res = await fetch('/status', { cache: "no-store" });
    const data = await res.json();

    const body = document.getElementById("tableBody");
    body.innerHTML = "";

    for (const row of data) {
        body.insertAdjacentHTML("beforeend", `
            <tr>
                <td>${row.currency}</td>
                <td>${row.batchId ?? ""}</td>
                <td>${row.detailId}</td>
                <td>${row.periodId}</td>
                <td>${row.releaseStatus}</td>
                <td>${row.statusReason ?? ""}</td>
                <td>${row.managerStatus}</td>
            </tr>
        `);
    }
}

async function refreshCounts() {
    const res = await fetch('/counts', { cache: "no-store" });
    const data = await res.json();

    const body = document.getElementById("countsBody");
    body.innerHTML = "";

    for (const row of data) {
        body.insertAdjacentHTML("beforeend", `
            <tr>
                <td>${row.id}</td>
                <td>${row.count1}</td>
                <td>${row.count2}</td>
            </tr>
        `);
    }
}

async function refreshError() {
    const res = await fetch('/last-error', { cache: "no-store" });
    const data = await res.json();

    const box = document.getElementById("errorBox");

    if (!data.message || data.message.trim() === "") {
        box.textContent = "No errors";
        box.classList.add("ok");
    } else {
        box.textContent = data.message;
        box.classList.remove("ok");
    }
}

function refreshAll() {
    refreshError();
    refreshCounts();
    refreshStatus();
}

setInterval(refreshAll, 100);
refreshAll();
</script>

</body>
</html>
""", "text/html");
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