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
using Microsoft.Extensions.DependencyInjection;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

        Serilog.Debugging.SelfLog.Enable(Console.Error);
        Log.Logger = new LoggerConfiguration()
           .MinimumLevel.Information()
            .CreateLogger();
        builder.Host.UseSerilog(Log.Logger);

        builder.Services.AddScoped<TooltipService>();
        builder.Services.AddScoped<DialogService>();
        builder.Services.AddScoped<NotificationService>();
        builder.Services.AddScoped<ContextMenuService>();

        builder.Services.AddRazorPages();
        builder.Services.AddServerSideBlazor();

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

        builder.Services.AddSingleton<SearchResultStore>();
        builder.Services.AddScoped<ISearchEngineClient, DuckDuckGoClient>();
        builder.Services.AddScoped<IScraperService, ScraperService>();
        builder.Services.AddScoped<IAnalysisService, AnalysisService>();
        builder.Services.AddScoped<DdgOrchestrator>();

        var app = builder.Build();


        using (var scope = app.Services.CreateScope())
        {
            var store = scope.ServiceProvider.GetRequiredService<SearchResultStore>();
            store.SeedResults();
        }

        
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
