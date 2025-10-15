(from e in Employees
select new {
    e.EmployeeID,
    Name = e.FirstName + " " + e.LastName,
    TotalSales = e.Orders.SelectMany(o => o.Order_Details)
                         .Sum(od => od.Quantity * od.UnitPrice * (decimal)(1 - od.Discount))
}).OrderBy(x => x.EmployeeID)