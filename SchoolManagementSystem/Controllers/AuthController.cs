using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SchoolManagementSystem.Models;
using SchoolManagementSystem.Models.ViewModel;

namespace SchoolManagementSystem.Controllers
{
    public class AuthController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AuthController> logger;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AuthController(UserManager<ApplicationUser> _userManager, ILogger<AuthController> logger,
            SignInManager<ApplicationUser> signInManager)
        {
            this._userManager = _userManager;
            this.logger = logger;
            this._signInManager = signInManager;
        }
        
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                TempData["Message"] = "User already LoggedIn";
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            //var user = await _userManager.FindByEmailAsync(model.Email);
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ViewData["UserProblem"] = "User Not Found, Contact Admin To Add You";
                return View(model);
            }
            logger.LogInformation("User found successfully");
            var result = await _userManager.CheckPasswordAsync(user , model.Password);
            logger.LogInformation(result + "result");
            if (result)
            {
                logger.LogInformation("login success");
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }
            ViewData["UserProblem"] = "Invalid Password";
            return View(model);
        }
    
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Auth");
            }
            logger.LogInformation(user.Id);
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction("Index", "Home");
        }


        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
