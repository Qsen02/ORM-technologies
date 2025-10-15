using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Employees.Model.Entities
{
    public class Project
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }

        public ICollection<EmployeeProject> EmployeeProjects { get; set; }
    }
}
