using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManagementSystem.Models
{
    public class Student
    {
        [Key]
        public int Id { get; set; }

        public int RollNo { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; } = string.Empty;

        public ApplicationUser? User { get; set; } = null!;

        public ICollection<Attendance> Attendances { get; set; }
            = new List<Attendance>();

        [ForeignKey("Class")]
        public int ClassId { get; set; }

        public Class? Class { get; set; } = null!;
    }
}