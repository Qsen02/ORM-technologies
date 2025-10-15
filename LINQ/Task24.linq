from e in Employees
where e.Orders.SelectMany(o => o.Order_Details)
              .Select(od => od.ProductID)
              .Distinct()
              .Count() > 70
select new {
    e.EmployeeID,
    Name = e.FirstName + " " + e.LastName,
    TotalSales = e.Orders.SelectMany(o => o.Order_Details)
                         .Sum(od => od.Quantity * od.UnitPrice * (decimal)(1 - od.Discount))
}