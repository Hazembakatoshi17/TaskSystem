using System.ComponentModel.DataAnnotations.Schema;

namespace TaskSystem.Models
{
    [Table("ProjectEmployees")]
    public class ProjectEmployee
    {
        public int Proj_Id { get; set; }
        public Project Project { get; set; }

        public int Emp_Id { get; set; }
        public Employee Employee { get; set; }

        public DateTime JoinedDate { get; set; } = DateTime.UtcNow;
    }
}
