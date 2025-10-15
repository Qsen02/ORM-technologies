from c in Customers
where (from p in Products where p.UnitPrice < 5 select p.ProductID)
      .All(pid => c.Orders.SelectMany(o => o.Order_Details).Any(od => od.ProductID == pid))
select c.CompanyName