from c in Customers
where (from o in Orders where o.CustomerID == "LAZYK" from od in o.Order_Details select od.ProductID).Distinct()
      .All(pid => c.Orders.SelectMany(o => o.Order_Details).Any(od => od.ProductID == pid))
select c.CompanyName