<Query Kind="Expression">
  <Connection>
    <ID>e548607a-bc9a-459a-9d78-612f2fd51cff</ID>
    <Persist>true</Persist>
    <Driver>EntityFrameworkDbContext</Driver>
    <CustomAssemblyPath>F:\Yasen\ORM technologies\Northwind\Northwind\bin\Debug\Northwind.dll</CustomAssemblyPath>
    <CustomTypeName>Northwind.NorthwindEntities</CustomTypeName>
    <AppConfigPath>F:\Yasen\ORM technologies\Northwind\Northwind\bin\Debug\Northwind.dll.config</AppConfigPath>
    <DisplayName>Northwind</DisplayName>
  </Connection>
</Query>

from E in Employees
select new { E.FirstName, E.LastName, E.Address, E.City, E.Region }