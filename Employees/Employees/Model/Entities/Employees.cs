using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Employees.Model.Entities
{
    public class Employee
    {
        public Employee() {
            this.EmployeeProjects = new List<EmployeeProject>();
        }

        public Employee(string name, string family, int department, byte isEmployed) : this() { 
            this.Name = name;
            this.Family = family;
            this.DepartmentId = department;
            this.IsEmployed = isEmployed;
        }

        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
        [Required]
        public string Family { get; set; }
        public byte IsEmployed { get; set; }

        [ForeignKey(nameof(Department))]
        public int DepartmentId { get; set; }
        public Department Department { get; set; }
        public ICollection<EmployeeProject> EmployeeProjects { get; set; }
    }
}
