using SchoolManagementSystem.Models;

namespace SchoolManagementSystem.Models.ViewModel
{
    public class HomeViewModel
    {
        // Statistics
        public int TotalStudents { get; set; }
        public int TotalTeachers { get; set; }
        public int TotalClasses { get; set; }
        public int TotalSubjects { get; set; }

        // Logged-in user
        public string UserName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;

        // Teacher information
        public string TeacherSubject { get; set; } = string.Empty;
        public int TeacherClassCount { get; set; }

        // Attendance
        public int TodayAttendanceCount { get; set; }

        // Recent students
        public List<Student> RecentStudents { get; set; } = new();

        public List<Teacher> RecentTeachers { get; set; } = new();
    }
}