using CommandsService.Models;
using Microsoft.EntityFrameworkCore;

namespace CommandsService.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> opt) : base(opt)
        {
            
        }

        public DbSet<Platform> Platforms {get;set;}
        public DbSet<Command> Commands {get;set;}


        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Platform>()
            .HasMany(x=>x.Commands)
            .WithOne(x=>x.Platform!)
            .HasForeignKey(x=>x.PlatformId);

            builder.Entity<Command>()
            .HasOne(x=>x.Platform)
            .WithMany(x=>x.Commands)
            .HasForeignKey(x=>x.PlatformId);

        }
    }
}