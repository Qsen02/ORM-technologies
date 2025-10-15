from p in Products
group p by p.Category.CategoryName into g
select new { Category = g.Key, AveragePrice = g.Average(p => p.UnitPrice) }