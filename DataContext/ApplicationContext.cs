using Gruppe4NLA.Models;
using Microsoft.EntityFrameworkCore;

namespace Gruppe4NLA.DataContext
{
    public class ApplicationContext : DbContext
    {
        public ApplicationContext(DbContextOptions<ApplicationContext> dbContextopt)
            : base(dbContextopt)
        {
        }

        public DbSet<AdviceDto> Advices { get; set; }
        public DbSet<ReportModel> Reports { get; set; } // Add this line

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AdviceDto>().HasKey(keyId => keyId.AdviceId);

            // Configure ReportModel primary key if needed
            modelBuilder.Entity<ReportModel>().HasKey(r => r.Id);
        }
    }
}
