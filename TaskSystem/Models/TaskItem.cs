using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskSystem.Models
{
    public enum WorkTaskStatus { Pending, InProgress, Done }
    public enum WorkTaskPriority { Low, Medium, High }

    [Table("Tasks")]
    public class TaskItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Task_Id { get; set; }

        [Required]
        public string Task_Title { get; set; }

        public string? Task_Description { get; set; }

        public WorkTaskStatus Task_Status { get; set; } = WorkTaskStatus.Pending;

        public WorkTaskPriority Task_Priority { get; set; } = WorkTaskPriority.Medium;

        public DateTime Task_DueDate { get; set; }

        // FK → Project
        public int Proj_Id { get; set; }
        [ForeignKey("Proj_Id")]
        public Project Project { get; set; }

        // Navigation
        public ICollection<TaskAssignment> TaskAssignments { get; set; } = new List<TaskAssignment>();
    }
}
