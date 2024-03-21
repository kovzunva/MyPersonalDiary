using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using MyPersonalDiary.Models;
using MyPersonalDiary.Data;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using MyPersonalDiary.Services;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing;

namespace MyPersonalDiary.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ApplicationDbContext _context;

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager,
            ApplicationDbContext dbContext)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = dbContext;
        }

        [HttpGet]
        public IActionResult Register(string registrationCode)
        {
            // Перевірка, чи існує такий код реєстрації
            if (_context.RegistrationCodes.Any(code => code.Code == registrationCode))
            {
                // Код реєстрації валідний, виведення сторінки реєстрації
                var model = new RegisterViewModel();
                return View(model);
            }
            else
            {
                var errorMessage = "Неправильний код реєстрації.";
                TempData["ErrorMessage"] = errorMessage;
                return View("Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Register(string registrationCode, RegisterViewModel model)
        {
            // Перевірка, чи існує такий код реєстрації
            if (RegistrationCode.IsRegistrationCodeValid(registrationCode, _context))
            {
                // Перевірка правильності введеної капчі
                string captchaTextFromSession = HttpContext.Session.GetString("Captcha");
                if (captchaTextFromSession != model.UserCaptcha)
                {
                    ModelState.AddModelError(string.Empty, "Неправильний код капчі.");
                    return View(model);
                }

                // Код реєстрації валідний, обробка реєстрації
                if (ModelState.IsValid)
                {
                    var user = new User { UserName = model.Email, Email = model.Email, NickName = model.NickName };
                    var result = await _userManager.CreateAsync(user, model.Password);

                    if (result.Succeeded)
                    {
                        // Видалення реєстраційного коду
                        var codeToDelete = await _context.RegistrationCodes.FirstOrDefaultAsync(c => c.Code == registrationCode);
                        if (codeToDelete != null)
                        {
                            _context.RegistrationCodes.Remove(codeToDelete);
                            await _context.SaveChangesAsync();
                        }

                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return RedirectToAction("Index", "Home");
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }

                return View(model);
            }
            else
            {
                var errorMessage = "Неправильний код реєстрації.";
                TempData["ErrorMessage"] = errorMessage;
                return View("Error");
            }
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Не вдалося ввійти.");
                    return View(model);
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Delete()
        {
            return View();
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            user.DeleteAt = DateTime.Now.AddDays(2);
            await _userManager.UpdateAsync(user);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> CancelDelete()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            if (user.DeleteAt != null) return View(user);
            else return RedirectToAction("Index", "Home");
        }

        [HttpPost, ActionName("CancelDelete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelDeleteConfirmed()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            user.DeleteAt = null;
            await _userManager.UpdateAsync(user);

            return RedirectToAction("Index", "Home");
        }
    }
}