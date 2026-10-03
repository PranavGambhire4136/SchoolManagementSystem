using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Models;
using SchoolManagementSystem.Models.ViewModel;

namespace SchoolManagementSystem.Controllers
{
    public class TeacherController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ILogger<ApplicationUser> logger;

        public TeacherController(ApplicationDbContext dbContext, 
            UserManager<ApplicationUser> _userManager, ILogger<ApplicationUser> logger)
        {
            _dbContext = dbContext;
            userManager = _userManager;
            this.logger = logger;
        }

        public ApplicationDbContext _dbContext { get; }

        [Authorize(Roles = "Admin")]
        public IActionResult AddTeacher()
        {
            var model = new AddTeacherViewModel
            {
                Subjects = _dbContext.Subjects.ToList()
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddTeacher(AddTeacherViewModel model)
        {
            logger.LogInformation("started");
            if (!ModelState.IsValid)
            {
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        logger.LogInformation(
                            "Property: {Property}, Error: {Error}",
                            state.Key,
                            error.ErrorMessage
                        );
                    }
                }
                logger.LogInformation("Model state is Invalid");
                model.Subjects = await _dbContext.Subjects.ToListAsync();
                return View(model);
            }

            var user = new ApplicationUser
            {
                Name = model.Name,
                Email = model.Email,
                UserName = model.Email
            };

            var result = await userManager.CreateAsync(user, model.Email);

            if (!result.Succeeded)
            {
                logger.LogInformation("creating user is failed");
                foreach (var error in result.Errors)
                {
                    logger.LogInformation(
                        "Identity Error - Code: {Code}, Description: {Description}",
                        error.Code,
                        error.Description
                    );

                    ModelState.AddModelError("", error.Description);
                }
                
                return View(model);
            }

            var previousEmployeeNumber = await _dbContext.Teachers
            .OrderByDescending(t => t.Id)
            .Select(t => t.EmployeeNumber)
            .FirstOrDefaultAsync();

            int nextNumber = 1;

            if (!string.IsNullOrEmpty(previousEmployeeNumber))
            {
                nextNumber = int.Parse(previousEmployeeNumber.Replace("EMP", "")) + 1;
            }

            var employeeNumber = $"EMP{nextNumber:D3}";

            var teacher = new Teacher
            {
                IsWorkingHere = true,
                User = user,
                EmployeeNumber = employeeNumber,
                SubjectId = model.SubjectId
            };

            await userManager.AddToRoleAsync(user, "Teacher");

            _dbContext.Teachers.Add(teacher);
            await _dbContext.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }


        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveTeacher()
        {
            var data = await _dbContext.Teachers.
                Where(t => t.IsWorkingHere == true).
                Include(t => t.User).
                Include(t => t.Subject).
                ToListAsync();

            return View(data);
        }

        [HttpPost]
        public async Task<IActionResult> RemoveTeacher(int id)
        {
            var teacher = await _dbContext.Teachers
                .Include(t => t.Subject)
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == id && !t.IsWorkingHere);

            teacher.IsWorkingHere = false;

            await _dbContext.SaveChangesAsync();
            return RedirectToAction("Index", "Home");
        }


        [Authorize(Roles = "Teacher")]
        [HttpGet]
        public async Task<IActionResult> MarkAttendance(
            int classId = 0,
            DateTime? date = null)
        {
            var user = await userManager.GetUserAsync(User);

            if (user == null)
                return Unauthorized();

            var teacher = await _dbContext.Teachers
                .FirstOrDefaultAsync(t => t.UserId == user.Id);

            if (teacher == null)
                return NotFound("Teacher record not found.");

            var attendanceDate = date?.Date ?? DateTime.Today;

            var model = new MarkAttendanceViewModel
            {
                ClassId = classId,
                Date = attendanceDate,

                Classes = await _dbContext.ClassSubjects
                    .Where(cs => cs.SubjectId == teacher.SubjectId)
                    .Select(cs => cs.Class)
                    .OrderBy(c => c.Standard)
                    .ToListAsync()
            };

            if (classId != 0)
            {
                // Check whether teacher's subject is assigned to this class
                var classAssigned = await _dbContext.ClassSubjects
                    .AnyAsync(cs =>
                        cs.ClassId == classId &&
                        cs.SubjectId == teacher.SubjectId);

                if (!classAssigned)
                    return Forbid();

                // Get students of selected class
                var students = await _dbContext.Students
                    .Where(s => s.ClassId == classId)
                    .Include(s => s.User)
                    .OrderBy(s => s.RollNo)
                    .ToListAsync();

                // Get attendance for selected date
                var attendance = await _dbContext.Attendances
                    .Where(a =>
                        a.TeacherId == teacher.Id &&
                        a.SubjectId == teacher.SubjectId &&
                        a.Date.Date == attendanceDate)
                    .ToListAsync();

                model.Students = students
                    .Select(s =>
                    {
                        var existingAttendance = attendance
                            .FirstOrDefault(a => a.StudentId == s.Id);

                        return new StudentAttendanceViewModel
                        {
                            StudentId = s.Id,
                            StudentName = s.User!.Name,
                            RollNo = s.RollNo,

                            // Checked only if attendance exists and student was present
                            IsPresent = existingAttendance?.IsPresent ?? false
                        };
                    })
                    .ToList();
            }

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> MarkAttendance(MarkAttendanceViewModel model)
        {
            var user = await userManager.GetUserAsync(User);

            logger.LogInformation("Logged in User Id: {UserId}", user.Id);
            logger.LogInformation("Logged in User Email: {Email}", user.Email);

            var teacher = await _dbContext.Teachers.FirstOrDefaultAsync(t => t.UserId == user.Id);

            if (teacher == null)
            {
                logger.LogInformation("No Teacher found for UserId: {UserId}", user.Id);

                return NotFound("Teacher not found");
            }

            var classAssigned = await _dbContext.ClassSubjects.AnyAsync(cs =>
                        cs.ClassId == model.ClassId && cs.SubjectId == teacher.SubjectId);

            if (!classAssigned) return Forbid();

            foreach (var student in model.Students)
            {
                var attendance = new Attendance
                {
                    IsPresent = student.IsPresent,
                    Date = model.Date,
                    StudentId = student.StudentId,
                    SubjectId = teacher.SubjectId,
                    TeacherId = teacher.Id
                };

                _dbContext.Add(attendance);
            }

            await _dbContext.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }

        [Authorize(Roles = "Teacher")]
        [HttpGet]
        public async Task<IActionResult> AssignMark(int? ClassId, int? StudentId)
        {
            var user = await userManager.GetUserAsync(User);

            if (user == null)
            {
                return Unauthorized();
            }

            var teacher = await _dbContext.Teachers.FirstOrDefaultAsync(t => t.UserId == user.Id);

            if (teacher == null)
            {
                return NotFound("Teacher record not found");
            }

            var model = new AssignMarkViewModel
            {
                TeacherId = teacher,
                ClassId = await _dbContext.Classes.OrderBy(c => c.Standard).ToListAsync()
            };

            if (ClassId != null)
            {
                model.StudentId = await _dbContext.Students.
                                        Where(s => s.ClassId == ClassId).
                                        Include(s => s.User).ToListAsync();
            }

            if (StudentId.HasValue)
            {
                var existingMarks = await _dbContext.Marks
                    .FirstOrDefaultAsync(m =>
                        m.StudentId == StudentId.Value &&
                        m.SubjectId == teacher.SubjectId &&
                        m.TeacherId == teacher.Id);

                if (existingMarks != null)
                {
                    model.MarksObtained = existingMarks.MarksObtained;
                    model.MaxMarks = existingMarks.MaxMarks;
                }
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AssignMark(AssignMarkViewModel model)
        {
            var user = await userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound("Teacher not found");
            }

            logger.LogInformation("Logged in User Id: {UserId}", user.Id);
            logger.LogInformation("Logged in User Email: {Email}", user.Email);

            var teacher = await _dbContext.Teachers.FirstOrDefaultAsync(t => t.UserId == user.Id);

            if (teacher == null)
            {
                return NotFound("Teacher Not found");
            }
            var existingMarks = await _dbContext.Marks
                        .FirstOrDefaultAsync(m =>
                            m.StudentId == model.SingleStudentId &&
                            m.SubjectId == teacher.SubjectId &&
                            m.TeacherId == teacher.Id);

            if (existingMarks == null)
            {
                // No marks exist → create
                var marks = new Marks
                {
                    MarksObtained = model.MarksObtained,
                    MaxMarks = model.MaxMarks,
                    StudentId = model.SingleStudentId,
                    SubjectId = teacher.SubjectId,
                    TeacherId = teacher.Id
                };

                _dbContext.Marks.Add(marks);
            }
            else
            {
                // Marks already exist → update
                existingMarks.MarksObtained = model.MarksObtained;
                existingMarks.MaxMarks = model.MaxMarks;
            }

            await _dbContext.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }
    
    
    }
}
