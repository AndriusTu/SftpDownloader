using Renci.SshNet;
using Renci.SshNet.Sftp;
using SftpDownloader.Configuration;
using SftpDownloader.Entities;

namespace SftpDownloader.Services.FileDownload;

// Could be renamed to SftpFileDownloadService and implement FileDownloadService interface, for different protocols
public class FileDownloadService
{
    private readonly SftpConfiguration _sftpConfig;
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<FileDownloadService> _logger;

    public FileDownloadService(
        SftpConfiguration sftpConfig,
        ApplicationDbContext dbContext,
        ILogger<FileDownloadService> logger)
    {
        _sftpConfig = sftpConfig;
        _dbContext = dbContext;
        _logger = logger;
    }

    // Download new files from SFTP
    public async Task DownloadNewFiles()
    {
        // Create SFTP client
        using var sftpClient = new SftpClient(_sftpConfig.Host, _sftpConfig.Username, _sftpConfig.Password);

        try
        {
            // Connect to SFTP and get files
            sftpClient.Connect();
            var files = sftpClient.ListDirectory(_sftpConfig.RemotePath);
            _logger.LogInformation($"Found {files.Count()} files in {_sftpConfig.RemotePath}.");

            foreach (var file in files)
            {
                // Check if file is a regular file and if it is new
                if (file.IsRegularFile && IsNewFile(file))
                {
                    // Download file and store information in database
                    DownloadFile(sftpClient, file.FullName);
                    await StoreFileInDatabase(file.FullName, file.LastWriteTimeUtc);
                    _logger.LogInformation($"Downloaded {file.FullName}.");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while downloading files from SFTP.");
        }
        finally
        {
            // Disconnect from SFTP after download
            sftpClient.Disconnect();
        }
    }

    // Check if file is new by checking if it exists in the database
    private bool IsNewFile(SftpFile file)
    {
        return !_dbContext.Files.Any(x => x.FilePath == file.FullName && x.LastWrittenAt == file.LastWriteTimeUtc);
    }

    // Download file from SFTP to local path
    private void DownloadFile(SftpClient sftpClient, string remoteFilePath)
    {
        var localFilePath = Path.Combine(_sftpConfig.LocalPath, Path.GetFileName(remoteFilePath));
        using var fileStream = File.Create(localFilePath);
        sftpClient.DownloadFile(remoteFilePath, fileStream);
    }


    // Store file information in database
    private async Task StoreFileInDatabase(string filePath, DateTime lastWrittenAt)
    {
        var file = new FileEntity()
        {
            FilePath = filePath,
            LastWrittenAt = lastWrittenAt
        };

        _dbContext.Files.Add(file);
        await _dbContext.SaveChangesAsync();
    }
}
