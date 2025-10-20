using Gruppe4NLA.Models;
using Gruppe4NLA.Areas.Identity.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;



namespace Gruppe4NLA.DataContext;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Your domain sets:
    public DbSet<AdviceDto> Advices { get; set; } = default!;
    public DbSet<ReportModel> Reports { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);                 // keep Identity mappings

        modelBuilder.Entity<AdviceDto>().HasKey(x => x.AdviceId);
        modelBuilder.Entity<ReportModel>().HasKey(x => x.Id);
        // any extra configuration…
    }
}



// OLD CODE BELOW:

//namespace Gruppe4NLA.DataContext
//{
//    public class ApplicationContext : DbContext
//    {
//        public ApplicationContext(DbContextOptions<ApplicationContext> dbContextopt)
//            : base(dbContextopt)
//        {
//        }

//        public DbSet<AdviceDto> Advices { get; set; }
//        public DbSet<ReportModel> Reports { get; set; } // Add this line

//        protected override void OnModelCreating(ModelBuilder modelBuilder)
//        {
//            modelBuilder.Entity<AdviceDto>().HasKey(keyId => keyId.AdviceId);

//            // Configure ReportModel primary key if needed
//            modelBuilder.Entity<ReportModel>().HasKey(r => r.Id);
//        }
//    }
//}
