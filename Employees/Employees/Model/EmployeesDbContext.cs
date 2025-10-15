namespace Employees.Model
{ 
    using MiniORM;
    using Entities;
    internal class EmployeesDbContext:DbContext
    {
        public EmployeesDbContext(string connectionString):base(connectionString) { 
    
        }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<EmployeeProject> EmployeeProjects { get; set; }
    }
}
