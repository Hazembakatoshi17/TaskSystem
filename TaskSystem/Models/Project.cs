using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskSystem.Models
{
    [Table("Projects")]
    public class Project
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Proj_Id { get; set; }

        [Required]
        public string Proj_Name { get; set; }

        public string? Proj_Description { get; set; }

        public DateTime Proj_StartDate { get; set; }

        public DateTime? Proj_EndDate { get; set; }

        // Navigation
        public ICollection<ProjectEmployee> ProjectEmployees { get; set; } = new List<ProjectEmployee>();
        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    }
}
