using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskSystem.Models
{
    [Table("TaskAssignments")]
    public class TaskAssignment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Assignment_Id { get; set; }

        // FK → Task
        public int Task_Id { get; set; }
        [ForeignKey("Task_Id")]
        public TaskItem Task { get; set; }

        // FK → Employee
        public int Emp_Id { get; set; }
        [ForeignKey("Emp_Id")]
        public Employee Employee { get; set; }

        public DateTime AssignedDate { get; set; } = DateTime.UtcNow;
    }
}
