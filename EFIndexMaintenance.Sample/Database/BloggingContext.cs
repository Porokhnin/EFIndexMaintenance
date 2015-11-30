using System.Data.Entity;

namespace EFIndexMaintenance.Database
{
    public class BloggingContext : DbContext
    {
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<User> Users { get; set; } 
    } 
}
