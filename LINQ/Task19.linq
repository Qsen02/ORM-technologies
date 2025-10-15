from c in Categories
select new { c.CategoryName, AvgPrice = c.Products.Average(p => p.UnitPrice) }