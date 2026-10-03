using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManagementSystem.Models
{
    public class Marks
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int MarksObtained { get; set; }

        [Required]
        public int MaxMarks { get; set; }

        [Required]
        [ForeignKey("Student")]
        public int StudentId { get; set; }

        public Student Student { get; set; } = null!;

        [Required]
        [ForeignKey("Subject")]
        public int SubjectId { get; set; }

        public Subject Subject { get; set; } = null!;

        [Required]
        [ForeignKey("Teacher")]
        public int TeacherId { get; set; }

        public Teacher Teacher { get; set; } = null!;
    }
}