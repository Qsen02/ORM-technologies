using Shop___Code_first.Model;
using Shop___Code_first.Model.Entities;

namespace Shop___Code_first
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string command = "";
            while (command != "end")
            {
                Console.WriteLine("Choose one of these commands:");
                Console.WriteLine("get products, get sales,get sales for product, add product, make sale, edit product, delete product or end for exit.");
                Console.Write("Enter command:");
                command = Console.ReadLine();
                CRUDOperations crud = new CRUDOperations();
                using (var db = new ShopContext())
                {
                    switch (command)
                    {
                        case "get products":
                            List<Product> products = crud.GetAllProducts(db);
                            if (products.Count > 0)
                            {
                                foreach (Product product in products)
                                {
                                    Console.WriteLine("Name: {0}, Description: {1}, Price: {2}", product.Name, product.Description, product.Price);
                                }
                            }
                            else
                            {
                                Console.WriteLine("No products yet.");
                            }
                            break;
                        case "get sales":
                            List<Sale> sales = crud.GetAllSales(db);
                            if (sales.Count > 0)
                            {
                                foreach (Sale sale in sales)
                                {
                                    Console.WriteLine("Quantity: {0}, Client name: {1}, Email: {2}, Phone: {3}, Date: {4}", sale.Quantity, sale.Client, sale.Email, sale.Phone, sale.Ordered);
                                }
                            }
                            else
                            {
                                Console.WriteLine("No sales yet.");
                            }
                            break;
                        case "get sales for product":
                            Console.Write("Enter product id:");
                            string prIdAsStr = Console.ReadLine();
                            int prId = int.Parse(prIdAsStr);
                            List<Sale> productSales = crud.GetSalesForProduct(db, prId);
                            if (productSales.Count > 0)
                            {
                                foreach (Sale sale in productSales)
                                {
                                    Console.WriteLine("Quantity: {0}, Client name: {1}, Email: {2}, Phone: {3}, Date: {4}", sale.Quantity, sale.Client, sale.Email, sale.Phone, sale.Ordered);
                                }
                            }
                            else
                            {
                                Console.WriteLine("No sales for this product yet.");
                            }
                            break;
                        case "add product":
                            Console.Write("Enter name:");
                            string name = Console.ReadLine();
                            Console.Write("Enter description:");
                            string description = Console.ReadLine();
                            Console.Write("Enter price:");
                            string priceAsStr = Console.ReadLine();
                            if (double.TryParse(priceAsStr, out double price))
                            {
                                crud.AddProduct(db, name, description, price);
                                Console.WriteLine("Product added successfully!");
                            }
                            else
                            {
                                Console.WriteLine("Invalid price!");
                            }
                            break;
                        case "make sale":
                            Console.Write("Enter product id:");
                            string productIdAsStr = Console.ReadLine();
                            int productId = int.Parse(productIdAsStr);
                            Console.Write("Enter qunatity:");
                            string quantityAsStr = Console.ReadLine();
                            int quantity = int.Parse(quantityAsStr);
                            Console.Write("Enter client name:");
                            string client = Console.ReadLine();
                            Console.Write("Enter email:");
                            string email = Console.ReadLine();
                            Console.Write("Enter phone:");
                            string phone = Console.ReadLine();
                            crud.MakeSale(db, productId, client, email, phone, quantity);
                            Console.WriteLine("Sale made successfully!");
                            break;
                        case "edit product":
                            Console.Write("Enter product id:");
                            string idAsStr = Console.ReadLine();
                            int id = int.Parse(idAsStr);
                            Console.Write("Enter name:");
                            string productName = Console.ReadLine();
                            Console.Write("Enter description:");
                            string productDescription = Console.ReadLine();
                            Console.Write("Enter price:");
                            string productPriceAsStr = Console.ReadLine();
                            if (double.TryParse(productPriceAsStr, out double productPrice))
                            {
                                crud.UpdateProduct(db, id, productName, productDescription, productPrice);
                                Console.WriteLine("Product edited successfully!");
                            }
                            else
                            {
                                Console.WriteLine("Invalid price!");
                            }
                            break;
                        case "delete product":
                            Console.Write("Enter product id:");
                            string idForproductAsStr = Console.ReadLine();
                            int idForProduct = int.Parse(idForproductAsStr);
                            crud.DeleteProduct(db, idForProduct);
                            Console.WriteLine("Product deleted successfully!");
                            break;
                        case "end":
                            Console.WriteLine("Goodbye!");
                            break;
                        default:
                            Console.WriteLine("Unknown command!");
                            break;
                    }
                }
            }
        }
    }
}
