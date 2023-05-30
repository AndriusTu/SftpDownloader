using Quartz;
using SftpDownloader.Services.FileDownload;

namespace SftpDownloader.Quartz.Jobs;
public class FileDownloadJob : IJob
{
    private readonly FileDownloadService _fileDownloadService;

    public FileDownloadJob(FileDownloadService fileDownloadService)
    {
        _fileDownloadService = fileDownloadService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        await _fileDownloadService.DownloadNewFiles();
    }
}
