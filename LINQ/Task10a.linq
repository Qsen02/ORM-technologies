from e in Employees
where e.BirthDate < (from l in Employees where l.City == "London" select l.BirthDate).Min()
select e.FirstName + " " + e.LastName