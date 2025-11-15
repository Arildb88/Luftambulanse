using Gruppe4NLA.Models;
using Gruppe4NLA.Areas.Identity.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Gruppe4NLA.DataContext;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Your domain sets:
    public DbSet<ReportModel> Reports { get; set; } = default!;
    public DbSet<ReportAssignmentLog> ReportAssignmentLogs { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // keep Identity mappings

        modelBuilder.Entity<ReportModel>().HasKey(x => x.Id);
        modelBuilder.Entity<ReportAssignmentLog>().HasKey(x => x.Id);
        // any extra configuration…
    }
}