using ScraperWapp.Adapter;
using ScraperWapp.Orchestrators;
using ScraperWapp.Services;
using Serilog;
using System.Net;
using Microsoft.EntityFrameworkCore;
using ScraperWapp.Data;
using Radzen;
using ScraperWapp.BackEnd.Interfaces.Adapters;
using ScraperWapp.BackEnd.Interfaces.Services;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Load configuration
        builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

        // Configure Serilog
        Serilog.Debugging.SelfLog.Enable(Console.Error);
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File("Logs/log-.json", rollingInterval: RollingInterval.Month,
                          restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information)
            .CreateLogger();
        builder.Host.UseSerilog(Log.Logger);

        // Radzen services
        builder.Services.AddScoped<TooltipService>();
        builder.Services.AddScoped<DialogService>();
        builder.Services.AddScoped<NotificationService>();
        builder.Services.AddScoped<ContextMenuService>();

        // Razor/Blazor
        builder.Services.AddRazorPages();
        builder.Services.AddServerSideBlazor();

        // HTTP client for DuckDuckGo
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

        // App services
        builder.Services.AddScoped<ISearchEngineClient, DuckDuckGoClient>();
        builder.Services.AddScoped<IScraperService, ScraperService>();
        builder.Services.AddScoped<IAnalysisService, AnalysisService>();
        builder.Services.AddScoped<DdgOrchestrator>();
        builder.Services.AddScoped<SearchResultRepository>();

        // AppDbContext
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite("Data Source=TestDb.db"));

        builder.WebHost.UseUrls("https://localhost:5000");

        var app = builder.Build();


        // --- Ensure database exists and seed data ---
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var dbPath = db.Database.GetDbConnection().DataSource;

            try
            {
                db.Database.EnsureCreated();
                Console.WriteLine($"Using database: {dbPath}");
            }
            catch
            {
                Console.WriteLine("Database corrupted. Recreating...");
                if (File.Exists(dbPath))
                    File.Delete(dbPath);

                db.Database.EnsureCreated();
                Console.WriteLine($"New database created: {dbPath}");
            }
            try
            {
                var orchestrator = scope.ServiceProvider.GetRequiredService<DdgOrchestrator>();
                await orchestrator.SeedResultsAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error during seeding:");
                Console.WriteLine(ex);
            }
        }

        // --- Configure middleware ---
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

        _ = Task.Run(() =>
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "http://localhost:5000",
                    UseShellExecute = true
                });
            }
            catch
            {
                Console.WriteLine("Could not launch browser automatically. Open http://localhost:5000 manually.");
            }
        });

        await app.RunAsync();
    }
}
