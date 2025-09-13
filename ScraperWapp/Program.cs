using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using ScraperWapp.Adapter;
using ScraperWapp.Orchestrators;
using ScraperWapp.Services;
using Serilog;
using Serilog.Extensions.Hosting; // Ensure this namespace is included
using System.Net;
using System.Net.Http;

var builder = WebApplication.CreateBuilder(args);

// -----------------------------
// Configuration
// -----------------------------
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

// -----------------------------
// Serilog setup
// -----------------------------
Serilog.Debugging.SelfLog.Enable(Console.Error);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.File("Logs/log-.json", rollingInterval: RollingInterval.Month,
                  restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information)
    .CreateLogger();

// Fix: Use `UseSerilog` extension method from `Serilog.Extensions.Hosting`
builder.Host.UseSerilog(Log.Logger);

// -----------------------------
// Services
// -----------------------------
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// HttpClient for your PageCollectionService
builder.Services.AddHttpClient<DuckDuckGoClient>(client =>
{
    client.DefaultRequestHeaders.UserAgent.ParseAdd(
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) " +
        "AppleWebKit/537.36 (KHTML, like Gecko) Chrome/139.0.0.0 Safari/537.36"
    );
    client.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
    client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en-GB,en;q=0.9");
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    UseCookies = true,
    CookieContainer = new CookieContainer(),
    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
});

// Your app services
builder.Services.AddSingleton<DuckDuckGoClient>();
builder.Services.AddSingleton<ScraperService>();
builder.Services.AddSingleton<DdgOrchestrator>();
builder.Services.AddSingleton<AnalysisService>();

var app = builder.Build();

// -----------------------------
// Middleware
// -----------------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

// -----------------------------
// Optional: Trigger a scraper run on startup
// -----------------------------
using (var scope = app.Services.CreateScope())
{
    var orchestrator = scope.ServiceProvider.GetRequiredService<DdgOrchestrator>();
    _ = Task.Run(async () =>
    {
        try
        {
            await orchestrator.CollectResultsAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error during initial scraper run");
        }
    });
}

app.Run();
