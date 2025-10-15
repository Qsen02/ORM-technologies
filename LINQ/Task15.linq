from p in Products
where Employees.All(e =>
    e.Orders.SelectMany(o => o.Order_Details).Any(od => od.ProductID == p.ProductID))
select p.ProductName