from c in Customers
from o in c.Orders
from od in o.Order_Details
where od.Quantity > 5 * (from od2 in Order_Details where od2.ProductID == od.ProductID group od2 by od2.ProductID into g select g.Average(x => x.Quantity)).FirstOrDefault()
select new { c.CompanyName, od.Product.ProductName, od.Quantity }