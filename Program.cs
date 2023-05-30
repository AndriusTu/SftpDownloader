using Microsoft.EntityFrameworkCore;
using Quartz;
using Serilog;
using SftpDownloader;
using SftpDownloader.Configuration;
using SftpDownloader.Quartz.Jobs;
using SftpDownloader.Quartz.Workers;
using SftpDownloader.Services.FileDownload;
using System.Configuration;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;
var configuration = builder.Configuration;

services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(configuration.GetConnectionString("Database")));
services.AddSingleton(builder.Configuration.GetSection("SftpConfiguration").Get<SftpConfiguration>());


// Configure Quartz
services.AddQuartz(q =>
{
    q.UseMicrosoftDependencyInjectionJobFactory();
});
services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

// Configure logging
builder.Host.UseSerilog((ctx, lc) => lc
     .WriteTo.File("sftpDownloaderLogs.logs",
        rollingInterval: RollingInterval.Day)
     .MinimumLevel.Debug()
     .ReadFrom.Configuration(configuration)
    );

// If different protocols are used, type of service could be injected based on configuration
services.AddTransient<FileDownloadService>();
services.AddTransient<FileDownloadJob>();
services.AddHostedService<FileDownloadWorker>();


var app = builder.Build();

MigrateDatabase(app);

app.Run();
static void MigrateDatabase(IApplicationBuilder app)
{
    using (var scope = app.ApplicationServices.CreateScope())
    {
        var services = scope.ServiceProvider;
        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.Migrate();
    }
}