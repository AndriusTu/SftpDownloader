namespace SftpDownloader.Configuration
{
    public class SftpConfiguration
    {
        public string Host { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string RemotePath { get; set; } = null!;
        public string LocalPath { get; set; } = null!;
    }

}
