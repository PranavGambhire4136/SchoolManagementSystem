namespace SchoolManagementSystem.Models.ViewModel
{
    public class StudentAttendanceViewModel
    {
        public int StudentId { get; set; }

        public string StudentName { get; set; } = string.Empty;

        public int RollNo { get; set; }

        public bool IsPresent { get; set; }
    }
}
