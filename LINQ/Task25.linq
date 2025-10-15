from e in Employees
where e.Orders.SelectMany(o => o.Order_Details)
              .Select(od => od.Product.SupplierID)
              .Distinct()
              .Count() > 7
select new { e.FirstName, e.LastName }