from s in Suppliers
where s.Products.Count() > 3
select new { s.SupplierID, s.CompanyName }