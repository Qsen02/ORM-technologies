from e in Employees
where e.Orders.Any(o =>
    o.Order_Details.Any(od =>
        od.Product.ProductName == "Gravad Lax" || od.Product.ProductName == "Mishi Kobe Niku"))
select new { e.Title, e.FirstName, e.LastName }