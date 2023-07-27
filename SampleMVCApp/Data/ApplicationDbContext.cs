using Microsoft.EntityFrameworkCore;
using SampleMVCApp.Models;

namespace SampleMVCApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            
        }

        public DbSet<Category> Categories { get; set; }

    }
}
