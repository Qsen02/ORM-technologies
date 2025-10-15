from p in Products
where p.Order_Details.Any(od =>
    od.Order.Customer.City == "London" ||
    od.Order.Employee.City == "London")
select p.ProductName