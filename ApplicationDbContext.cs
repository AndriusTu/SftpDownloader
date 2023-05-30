
using Microsoft.EntityFrameworkCore;
using SftpDownloader.Entities;

namespace SftpDownloader;
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<FileEntity> Files { get; set; }
}


