(from c in Customers
 where c.City == "London"
 from o in c.Orders
 from od in o.Order_Details
 select od.Product.ProductName)
 .Union(
 from e in Employees
 where e.City == "London"
 from o in e.Orders
 from od in o.Order_Details
 select od.Product.ProductName).Distinct()