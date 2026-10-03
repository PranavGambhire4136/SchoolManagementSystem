namespace SchoolManagementSystem.Models.ViewModel
{
    public class RemoveSubjectViewModel
    {
        public int ClassId { get; set; }
        public int SubjectId { get; set; }

        public List<Class> Classes { get; set; } = new();
        public List<Subject> Subjects { get; set; } = new();

    }
}
