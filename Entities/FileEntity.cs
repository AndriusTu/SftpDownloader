namespace SftpDownloader.Entities
{
    public class FileEntity
    {
        public Guid Id { get; set; }
        public string FilePath { get; set; } = null!;
        public DateTime LastWrittenAt { get; set; } = DateTime.UtcNow;
    }
}
