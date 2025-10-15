using Microsoft.EntityFrameworkCore;

namespace CodeFirstApp.Model
{
    public class BloggingContext: DbContext
    {
        public DbSet<Blog> Blogs { get; set; }

        public DbSet<Post> Posts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlServer(@"Server=(localdb)\MSSQLLocalDB; Database=Blogging");
    }
}
