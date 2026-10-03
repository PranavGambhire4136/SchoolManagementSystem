using System.ComponentModel.DataAnnotations;

namespace SchoolManagementSystem.Models
{
    public class Class
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int Standard { get; set; }

        public ICollection<Student> Students { get; set; } = new List<Student>();

        public ICollection<ClassSubject> ClassSubjects { get; set; }
            = new List<ClassSubject>();
    }
}
