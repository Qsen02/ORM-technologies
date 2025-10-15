from c in Customers
where c.City == "London"
from o in c.Orders
from od in o.Order_Details
where od.Product.Supplier.CompanyName == "Pavlova, Ltd." || od.Product.Supplier.CompanyName == "Karkki Oy"
select new { Customer = c.CompanyName, Product = od.Product.ProductName, Supplier = od.Product.Supplier.CompanyName }