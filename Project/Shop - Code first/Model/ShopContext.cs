using Microsoft.EntityFrameworkCore;
using Shop___Code_first.Model.Entities;

namespace Shop___Code_first.Model
{
    public class ShopContext: DbContext
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<Sale> Sales { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Data Source=(localdb)\MSSQLLocalDB; Database=ShopCodeFirstDB");
        }
    }
}
