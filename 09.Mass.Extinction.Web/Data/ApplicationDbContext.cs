namespace _09.Mass.Extinction.Web.Data
{
    using Entities;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;

    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        { }

        public DbSet<Message> Messages { get; set; }

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

            base.OnModelCreating(builder);
        }
    }
}
