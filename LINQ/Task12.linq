from e in Employees
from o in e.Orders
where o.Customer.City == e.City
select new { Employee = e.FirstName + " " + e.LastName, e.City }