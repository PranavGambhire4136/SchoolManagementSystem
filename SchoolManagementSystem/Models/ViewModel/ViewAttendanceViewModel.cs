namespace SchoolManagementSystem.Models.ViewModel
{
        public class ViewAttendanceViewModel
        {
            public List<Subject> Subjects { get; set; }

            public int SubjectId { get; set; }

            public List<Attendance> Attendances { get; set; } = new();
    }

}
