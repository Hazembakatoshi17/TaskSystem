using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskSystem.Models
{
    [Table("Employees")]
    public class Employee
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Emp_Id { get; set; }

        public string Emp_Fname { get; set; }
        public string Emp_Lname { get; set; }
        public string Emp_MobileNumber { get; set; }
        public string Emp_Password { get; set; }

        // One admin per department
        public bool IsAdmin { get; set; } = false;

        // FK → Department
        public int? Dept_Id { get; set; }
        [ForeignKey("Dept_Id")]
        public Department? Department { get; set; }

        // Navigation
        public ICollection<ProjectEmployee> ProjectEmployees { get; set; } = new List<ProjectEmployee>();
        public ICollection<TaskAssignment> TaskAssignments   { get; set; } = new List<TaskAssignment>();
    }
}
