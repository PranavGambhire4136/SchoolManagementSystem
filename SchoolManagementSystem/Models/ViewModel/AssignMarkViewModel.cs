using System.ComponentModel.DataAnnotations;

namespace SchoolManagementSystem.Models.ViewModel
{
    public class AssignMarkViewModel
    {
        [Required]
        public Teacher TeacherId { get; set; }

        public List<Student> StudentId { get; set; }

        public List<Class> ClassId { get; set; }

        public List<Subject> SubjectId { get; set; }

        public int SingleStudentId { get; set; }

        public int MaxMarks { get; set; }

        public int MarksObtained { get; set; }
    }
}
