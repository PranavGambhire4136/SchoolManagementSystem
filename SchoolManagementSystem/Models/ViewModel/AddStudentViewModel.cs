namespace SchoolManagementSystem.Models.ViewModel
{
    public class AddStudentViewModel
    {
        public Student Student { get; set; } = new Student();

        public List<Class> Classes { get; set; } = new List<Class>();

    }
}
