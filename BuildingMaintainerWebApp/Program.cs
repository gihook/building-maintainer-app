using BuildingMaintainerWebApp;
using Hangfire;
using Hangfire.MemoryStorage;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<MaintainerConfig>(builder.Configuration.GetSection("MaintainerConfig"));
builder.Services.AddScoped<BuildingMaintainerJob>();
builder.Services.AddSingleton<BuildingMaintainerWebApp.Services.SheetsService>();
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();

// Add Hangfire services
builder.Services.AddHangfire(configuration =>
    configuration
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseMemoryStorage()
);

// Add the processing server as IHostedService
builder.Services.AddHangfireServer();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHangfireDashboard(); // Optional: Map dashboard to /hangfire
app.MapRazorPages();

app.MapGet("/", () => "Building Maintainer Web App is running. Check /hangfire for dashboard.");
app.MapRazorPages();
app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapRazorPages();
});

// Schedule the job to run daily. You can adjust the cron expression as needed.
// Cron.Daily() runs at midnight UTC. You can change this to run at a specific time, e.g., "0 8 * * *" for 8 AM.
RecurringJob.AddOrUpdate<BuildingMaintainerJob>(
    "building-maintainer-daily-job",
    job => job.RunAsync(),
    Cron.Minutely()
);

app.Run();

