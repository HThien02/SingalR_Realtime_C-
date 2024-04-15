using BlogApplication.Models;
using Microsoft.EntityFrameworkCore;

namespace BlogApplication.Context
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Post> Post { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AppUser>().ToTable("AppUsers");
            modelBuilder.Entity<Category>().ToTable("PostCategories");
            modelBuilder.Entity<Post>().ToTable("Posts");
        }
    }
}
