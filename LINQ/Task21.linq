(from e in Employees
select new { e.EmployeeID, Name = e.FirstName + " " + e.LastName, Orders = e.Orders.Count })
.OrderBy(x => x.EmployeeID)