using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Models;
using SchoolManagementSystem.Models.ViewModel;

namespace SchoolManagementSystem.Controllers
{
    public class SubjectController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<ApplicationUser> _logger;

        public SubjectController(ApplicationDbContext dbContext, ILogger<ApplicationUser> _logger)
        {
            this._dbContext = dbContext;
            this._logger = _logger;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddSubject()
        {

            var addSubjectViewModel = new AddSubjectViewModel
            {
                Classes = await _dbContext.Classes.OrderBy(c => c.Standard).ToListAsync()
            };
            return View(addSubjectViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSubject(AddSubjectViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Classes = await _dbContext.Classes
                    .OrderBy(c => c.Standard)
                    .ToListAsync();

                return View(model);
            }

            var subject = new Subject
            {
                Name = model.Name
            };

            _dbContext.Subjects.Add(subject);

            await _dbContext.SaveChangesAsync();

            foreach (var classId in model.ClassIds)
            {
                var classSubject = new ClassSubject
                {
                    ClassId = classId,
                    SubjectId = subject.Id
                };

                _dbContext.ClassSubjects.Add(classSubject);
            }

            await _dbContext.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> RemoveSubject(int? classId)
        {
            _logger.LogInformation("Casjklfhisergbkqweghuiwhfjkash ClassId => " + classId);
            var data = new RemoveSubjectViewModel
            {
                ClassId = classId ?? 0,
                Classes = await _dbContext.Classes.OrderBy(c => c.Standard).ToListAsync()
            };


            if (classId != null)
            {
                data.Subjects = await _dbContext.ClassSubjects
                .Where(cs => cs.ClassId == classId.Value)
                .Include(cs => cs.Subject)
                .Select(cs => cs.Subject)
                .OrderBy(s => s.Name)
                .ToListAsync();

                _logger.LogInformation("Class info" + data.Subjects[0].Name);
            }

            return View(data);
        }

        [HttpPost]
        public async Task<IActionResult> RemoveSubject(int ClassId, int SubjectId)
        {
            var data = await _dbContext.ClassSubjects.FirstOrDefaultAsync(cs => cs.ClassId == ClassId && cs.SubjectId == SubjectId);

            if (data == null)
            {
                ViewData["NotFound"] = "subject with class not found";
                return View();
            }

            _dbContext.ClassSubjects.Remove(data);
            await _dbContext.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }
    }
}
