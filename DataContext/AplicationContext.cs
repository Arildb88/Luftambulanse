using Gruppe4NLA.Models;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.EntityFrameworkCore;

namespace Gruppe4NLA.DataContext
{
    public class ApplicationContext : DbContext
    {
        public ApplicationContext(DbContextOptions<ApplicationContext> dbContextopt) : base (dbContextopt)

        {

        }
          

            public DbSet<AdviceDto> Advices { get; set; } 

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AdviceDto>().HasKey(keyId => keyId.AdviceId);
        }





    }
}
