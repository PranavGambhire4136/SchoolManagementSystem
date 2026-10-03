using System.ComponentModel.DataAnnotations;

namespace SchoolManagementSystem.Models
{
    public class Subject
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public ICollection<ClassSubject> ClassSubjects { get; set; }
            = new List<ClassSubject>();

        public ICollection<Teacher> Teachers { get; set; } = new List<Teacher>();
    }
}