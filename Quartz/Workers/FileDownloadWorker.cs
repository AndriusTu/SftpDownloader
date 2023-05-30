using Quartz;
using SftpDownloader.Quartz.Jobs;

namespace SftpDownloader.Quartz.Workers;
public class FileDownloadWorker : IHostedService
{
    private readonly ISchedulerFactory _schedulerFactory;
    private IScheduler? _scheduler;

    public FileDownloadWorker(ISchedulerFactory schedulerFactory)
    {
        _schedulerFactory = schedulerFactory;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Create scheduler
        _scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
        await _scheduler.Start(cancellationToken);

        // Create job and trigger, jobs could be created dynamically from database/config
        var job = JobBuilder.Create<FileDownloadJob>()
            .WithIdentity("FileDownloadJob")
            .Build();

        // Trigger could be created dynamically from database/config
        var trigger = TriggerBuilder.Create()
            .WithIdentity("FileDownloadTrigger")
            .StartNow()
            .WithSimpleSchedule(x => x.WithInterval(TimeSpan.FromMinutes(1)).RepeatForever())
            .Build();

        await _scheduler.ScheduleJob(job, trigger, cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_scheduler == null)
        {
            return;
        }

        await _scheduler.Shutdown(cancellationToken);
    }
}

