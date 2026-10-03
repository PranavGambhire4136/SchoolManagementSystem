using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SchoolManagementSystem.Models;
using SchoolManagementSystem.Models.ViewModel;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace SchoolManagementSystem.Controllers
{
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext dbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public StudentController(ILogger<ApplicationUser> logger, 
            ApplicationDbContext dbContext, UserManager<ApplicationUser> _userManager)
        {
            this.logger = logger;
            this.dbContext = dbContext;
            this._userManager = _userManager;
        }

        public ILogger<ApplicationUser> logger { get; }

        [Authorize(Roles = "Admin")]
        public IActionResult AddStudent()
        {
            var model = new AddStudentViewModel
            {
                Classes = dbContext.Classes.ToList()
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddStudent(AddStudentViewModel model)
        {
            if (!ModelState.IsValid)
            {

                model.Classes = dbContext.Classes.ToList();

                return View(model);
            }

            // Create ApplicationUser
            var user = new ApplicationUser
            {
                Name = model.Student.User.Name,
                Email = model.Student.User.Email,
                UserName = model.Student.User.Email
            };

            var result = await _userManager.CreateAsync(user, model.Student.User.Email);

            if (!result.Succeeded)
            {

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                model.Classes = dbContext.Classes.ToList();

                return View(model);
            }

            // Generate Roll No. for selected class
            var lastRollNo = dbContext.Students
                .Where(s => s.ClassId == model.Student.ClassId)
                .OrderByDescending(s => s.RollNo)
                .Select(s => (int?)s.RollNo)
                .FirstOrDefault();

            int rollNo = (lastRollNo ?? 0) + 1;

            // Create Student
            var student = new Student
            {
                RollNo = rollNo,
                ClassId = model.Student.ClassId,
                UserId = user.Id
            };



            await _userManager.AddToRoleAsync(user, "Student");

            dbContext.Students.Add(student);
            await dbContext.SaveChangesAsync();
            return RedirectToAction("Index", "Home");
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveStudent(int? classId, int? studentId)
        {
            var data = await dbContext.Classes.OrderBy(c => c.Standard).ToListAsync();

            var model = new RemoveStudentViewModel
            {
                classId = classId,
                Classes = data
            };

            if (classId.HasValue)
            {
                model.Users = await dbContext.Students.
                                    Include(s => s.User)
                                    .Where(s => s.ClassId == classId)
                                    .OrderBy(s => s.RollNo)
                                    .ToListAsync();
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmRemoveStudent(int? ClassId, int? studentId)
        {
            var student = await dbContext.Students
                .Include(s => s.User)
                .Include(s => s.Class)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            var student = await dbContext.Students
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null)
            {
                return NotFound();
            }

            // Find Identity user
            var user = await _userManager.FindByIdAsync(student.UserId);

            // Delete Student
            dbContext.Students.Remove(student);

            // Delete ApplicationUser
            if (user != null)
            {
                var result = await _userManager.DeleteAsync(user);

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }

                    return View("ConfirmRemoveStudent", student);
                }
            }

            await dbContext.SaveChangesAsync();

            return RedirectToAction(nameof(RemoveStudent));
        }

        [Authorize(Roles = "Student")]
        public async Task<IActionResult> ViewMarks()
        {
            var user = await _userManager.GetUserAsync(User);

            var student = await dbContext.Students.FirstOrDefaultAsync(s => s.User.Id == user.Id);

            var result = await dbContext.Marks.Where(m => m.StudentId == student.Id).Include(m => m.Subject).ToListAsync();

            Console.WriteLine(
                System.Text.Json.JsonSerializer.Serialize(
                    result,
                    new System.Text.Json.JsonSerializerOptions
                    {
                        WriteIndented = true
                    }
                )
            );

            return View(result);
        }

        [HttpGet]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> ViewAttendance(int? subjectId)
        {

            logger.LogInformation("Request Started");
            var user = await _userManager.GetUserAsync(User);

            if (user == null) return Unauthorized();

            var student = await dbContext.Students.FirstOrDefaultAsync(s => s.User.Id == user.Id);

            if (student == null) return NotFound("Student Record Not found");

            var attendanceViewModel = new ViewAttendanceViewModel
            {
                Subjects = await dbContext.Subjects.ToListAsync(),
                SubjectId = subjectId ?? 0
            };

            if (subjectId.HasValue)
            {
                logger.LogInformation("Request entered in If block", subjectId, subjectId.Value);
                var attendance = await dbContext.Attendances
                    .AsNoTracking()
                    .Where(a =>
                        a.StudentId == student.Id &&
                        a.SubjectId == subjectId.Value)
                    .Include(a => a.Subject)
                    .Include(a => a.Teacher)
                        .ThenInclude(t => t.User)
                    .OrderByDescending(a => a.Date)
                    .ToListAsync();

                attendanceViewModel.Attendances = attendance;
            }

            //Console.WriteLine(
            //        System.Text.Json.JsonSerializer.Serialize(
            //            attendanceViewModel,
            //            new System.Text.Json.JsonSerializerOptions
            //            {
            //                WriteIndented = true
            //            }
            //        )
            //    );

            return View(attendanceViewModel);
        }
    
    }
}
