(from e in Employees
select new {
    e.EmployeeID,
    Name = e.FirstName + " " + e.LastName,
    DistinctProducts = e.Orders.SelectMany(o => o.Order_Details)
                               .Select(od => od.ProductID)
                               .Distinct()
                               .Count()
}).OrderBy(x => x.EmployeeID)