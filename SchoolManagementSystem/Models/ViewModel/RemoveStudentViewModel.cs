namespace SchoolManagementSystem.Models.ViewModel
{
    public class RemoveStudentViewModel
    {
        public int? classId { get; set; }

        public Student UserId { get; set; }

        public List<Class> Classes { get; set; }

        public List<Student> Users { get; set; }
    }
}
