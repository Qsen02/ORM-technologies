using CodeFirstApp.Model;

namespace CodeFirstApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using (var db = new BloggingContext()) {
                Console.WriteLine("Step 1, create blog...");
                var blog = db.Blogs.Add(new Blog { Name = "My blog" });
                db.SaveChanges();
                int blogId = blog.Entity.BlogId;

                Console.WriteLine("Step 2, add posts...");
                db.Posts.Add(new Post { Title = "1", Content = "One", BlogId = blogId });
                db.Posts.Add(new Post { Title = "2", Content = "Two", BlogId = blogId });
                db.Posts.Add(new Post { Title = "3", Content = "Three", BlogId = blogId });
                db.SaveChanges();

                Console.WriteLine("Step 3, display posts...");
                var posts=db.Posts.Where(post=>post.BlogId==blogId).OrderBy(post=>post.Title).ToList();
                foreach (Post post in posts) {
                    Console.WriteLine("Title: {0}, Content: {1}", post.Title, post.Content);
                }
            }
        }
    }
}
