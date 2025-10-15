from e in Employees
where e.HireDate < (from l in Employees where l.City == "London" select l.HireDate).Min()
select e.FirstName + " " + e.LastName