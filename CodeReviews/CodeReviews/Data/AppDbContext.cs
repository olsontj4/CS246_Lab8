using CodeReviews.Models;
using Microsoft.EntityFrameworkCore;

namespace CodeReviews.Data
{
    public class AppDbContext : DbContext
    {
        // constructor just calls the base class constructor
        public AppDbContext(
           DbContextOptions<AppDbContext> options) : base(options) { }

        // one DbSet for each domain model class
        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<Review> Reviews { get; set; }
    }
}
