using Gruppe4NLA.Areas.Identity.Data;
using Gruppe4NLA.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Gruppe4NLA.DataContext;

// The AppDbContext inherits from IdentityDbContext to include ASP.NET Core Identity features.
// It includes DbSet properties for your domain models and configures them in OnModelCreating.


// Inherit from IdentityDbContext to include Identity features
public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    // Constructor accepting DbContextOptions
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<ReportModel> Reports { get; set; } = default!;
    public DbSet<ReportAssignmentLog> ReportAssignmentLogs { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Defines primary key for ReportModel and ReportAssignmentLog
        modelBuilder.Entity<ReportModel>().HasKey(x => x.Id); 
        modelBuilder.Entity<ReportAssignmentLog>().HasKey(x => x.Id); 
    }
}