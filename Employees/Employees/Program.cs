using Employees.Model;
using Employees.Model.Entities;

namespace Employees
{
    class Program
    {
        static void Main(string[] args)
        {
            string connectionString = @"Data Source=(localdb)\\MSSQLLocalDB; Database=Employees; Trusted_Connection=True";
            EmployeesDbContext db = new EmployeesDbContext(connectionString);

            Console.WriteLine("Departments:");
            foreach (Department department in db.Departments) 
            { 
                Console.WriteLine("{0}. {1}",department.Id,department.Name);
            }
            Console.WriteLine();
            foreach (Employee emp in db.Employees)
            {
                Console.WriteLine("{0}. {1} {2}", emp.Id, emp.Name, emp.Family);
            }
        }
    }
}
