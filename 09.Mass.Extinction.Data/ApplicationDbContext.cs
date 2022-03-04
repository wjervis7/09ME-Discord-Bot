namespace _09.Mass.Extinction.Data;

using Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Message> Messages { get; set; }
    public DbSet<DiscordUser> DiscordUsers { get; set; }
    public DbSet<ActivityReport> ActivityReports { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("MessageId").ValueGeneratedOnAdd();
            entity.Property(e => e.Sender).IsRequired();
            entity.Property(e => e.Body).IsRequired();
            entity.Property(e => e.DateSent).IsRequired();
            entity.Property(e => e.IsAnonymous).IsRequired();
        });

        builder.Entity<DiscordUser>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("DiscordUserId").ValueGeneratedNever();
            entity.Property(e => e.TimeZone).IsRequired(false);
        });

        builder.Entity<ActivityReport>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("ActivityReportId").ValueGeneratedOnAdd();
            entity.Property(e => e.Initiator).IsRequired();
            entity.Property(e => e.StartTime).IsRequired();
            entity.Property(e => e.EndTime).IsRequired();
            entity.Property(e => e.ReportType).IsRequired().HasMaxLength(250);
            entity.Property(e => e.Args).HasColumnType("nvarchar(max)").IsRequired();
            entity.Property(e => e.Report).HasColumnType("nvarchar(max)").IsRequired();
        });

        base.OnModelCreating(builder);
    }
}
