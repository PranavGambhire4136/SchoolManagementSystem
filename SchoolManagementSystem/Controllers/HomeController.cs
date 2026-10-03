using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Models;
using SchoolManagementSystem.Models.ViewModel;

namespace SchoolManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(
            ApplicationDbContext dbContext,
            UserManager<ApplicationUser> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var model = new HomeViewModel
            {
                TotalStudents = await _dbContext.Students.CountAsync(),

                TotalTeachers = await _dbContext.Teachers
                    .CountAsync(t => t.IsWorkingHere),

                TotalClasses = await _dbContext.Classes.CountAsync(),

                TotalSubjects = await _dbContext.Subjects.CountAsync(),

                RecentStudents = await _dbContext.Students
                    .Include(s => s.User)
                    .Include(s => s.Class)
                    .OrderByDescending(s => s.Id)
                    .Take(5)
                    .ToListAsync(),

                RecentTeachers = await _dbContext.Teachers
                    .Include(s => s.User)
                    .Include(s => s.Subject)
                    .OrderByDescending(s => s.Id)
                    .Take(5)
                    .ToListAsync()
            };

            // Logged-in user information
            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);

                if (user != null)
                {
                    model.UserName = user.Name;

                    var roles = await _userManager.GetRolesAsync(user);

                    if (roles.Any())
                    {
                        model.Role = roles.First();
                    }

                    // Teacher information
                    if (model.Role == "Teacher")
                    {
                        var teacher = await _dbContext.Teachers
                            .Include(t => t.Subject)
                            .FirstOrDefaultAsync(t => t.UserId == user.Id);

                        if (teacher != null)
                        {
                            model.TeacherSubject =
                                teacher.Subject?.Name ?? "Not Assigned";

                            model.TeacherClassCount =
                                await _dbContext.ClassSubjects
                                    .CountAsync(cs =>
                                        cs.SubjectId == teacher.SubjectId);
                        }
                    }
                }
            }

            return View(model);
        }
    }
}