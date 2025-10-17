using Shop___Code_first.Model;
using Shop___Code_first.Model.Entities;

namespace Shop___Code_first
{
    public class CRUDOperations
    {
        public CRUDOperations() 
        {
        
        }
        public List<Product> GetAllProducts(ShopContext db) 
        { 
            List<Product> products = db.Products.OrderBy(el=>el.Name).ToList();
            return products;
        }

        public List<Sale> GetAllSales(ShopContext db)
        {
            List<Sale> sales = db.Sales.ToList();
            return sales;
        }
        public List<Sale> GetSalesForProduct(ShopContext db, int productId) 
        { 
            List<Sale> sales=db.Sales.Where(el=>el.ProductId==productId).ToList();
            return sales;
        }

        public void AddProduct(ShopContext db,string name,string description,double price)
        {
            db.Products.Add(new Product
            {
                Name = name,
                Description = description,
                Price = price,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            });
            db.SaveChanges();
        }
        public void MakeSale(ShopContext db, int productId, string client, string email, string phone, int quantity) 
        {
            var sale=db.Sales.Add(new Sale
            {
                ProductId=productId,
                Client=client,
                Email=email,
                Phone=phone,
                Quantity=quantity,
                Ordered=DateTime.Now
            });
            Sale newSale = (Sale)db.Sales.Where(el => el.Id == sale.Entity.Id);
            Product product = (Product)db.Products.Where(el => el.Id == productId);
            if (product != null)
            {
                product.Sales.Add(newSale);
            }
            db.SaveChanges();
        }
        public void UpdateProduct(ShopContext db, int productId, string name, string description, double price) 
        {
            Product product = (Product)db.Products.Where(el => (int)el.Id == productId);
            product.Name = name;
            product.Description = description;
            product.Price = price;
            db.SaveChanges();
        }
        public void DeleteProduct(ShopContext db, int productId)
        {
            Product product = (Product)db.Products.Where(el => el.Id == productId);
            if (product != null) 
            {
                db.Products.Remove(product);
                db.SaveChanges();
            }
        }
    }
}
