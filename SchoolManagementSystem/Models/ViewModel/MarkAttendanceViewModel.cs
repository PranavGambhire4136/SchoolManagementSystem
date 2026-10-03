namespace SchoolManagementSystem.Models.ViewModel
{
    public class MarkAttendanceViewModel
    {
        public int ClassId { get; set; }

        public DateTime Date { get; set; } = DateTime.Today;

        public List<Class> Classes { get; set; } = new();

        public List<StudentAttendanceViewModel> Students { get; set; } = new();
    }
}
