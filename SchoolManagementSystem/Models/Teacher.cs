using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManagementSystem.Models
{
    public class Teacher
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; } = string.Empty;

        public ApplicationUser? User { get; set; } = null!;

        [Required]
        public string EmployeeNumber { get; set; } = string.Empty;

        public ICollection<Attendance> Attendances { get; set; }
            = new List<Attendance>();

        public bool IsWorkingHere { get; set; }


        [Required]
        [ForeignKey("Subject")]
        public int SubjectId { get; set; }

        public Subject Subject { get; set; } = null!;
    }
}