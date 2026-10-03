using System.ComponentModel.DataAnnotations;

namespace SchoolManagementSystem.Models.ViewModel
{
    public class AddTeacherViewModel
    {

        [Required]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }


        public int SubjectId { get; set; }

        public List<Subject> Subjects { get; set; } = new List<Subject>();

    }
}
