from c in Customers
let lazyk = (from o in Orders where o.CustomerID == "LAZYK" from od in o.Order_Details select od.ProductID).Distinct().OrderBy(x => x)
let products = c.Orders.SelectMany(o => o.Order_Details).Select(od => od.ProductID).Distinct().OrderBy(x => x)
where lazyk.SequenceEqual(products)
select c.CompanyName