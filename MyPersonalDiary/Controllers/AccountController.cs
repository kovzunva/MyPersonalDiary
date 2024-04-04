using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MyPersonalDiary.ViewModels;
using MyPersonalDiary.Interfaces;

namespace MyPersonalDiary.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly IRegistrationCodesService _registrationCodesService;

        public AccountController(IAccountService accountService, IRegistrationCodesService registrationCodesService)
        {
            _accountService = accountService;
            _registrationCodesService = registrationCodesService;
        }

        [HttpGet]
        public IActionResult Register(string registrationCode)
        {
            // Перевірка, чи існує такий код реєстрації
            if (_registrationCodesService.CheckRegistrationCode(registrationCode))
            {
                // Код реєстрації валідний, виведення сторінки реєстрації
                var model = new RegisterViewModel();
                return View(model);
            }

                var errorMessage = "Неправильний код реєстрації.";
                TempData["ErrorMessage"] = errorMessage;
                return View("ErrorGuest");
        }

        [HttpPost]
        public async Task<IActionResult> Register(string registrationCode, RegisterViewModel model)
        {
            // Перевірка, чи існує такий код реєстрації
            if (!_registrationCodesService.CheckRegistrationCode(registrationCode))
            {
                var errorMessage = "Неправильний код реєстрації.";
                TempData["ErrorMessage"] = errorMessage;
                return View("ErrorGuest");
            }

            // Перевірка правильності введеної капчі
            string captchaTextFromSession = HttpContext.Session.GetString("Captcha");
            if (captchaTextFromSession != model.UserCaptcha)
            {
                ModelState.AddModelError(string.Empty, "Неправильний код капчі.");
                return View(model);
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _accountService.CreateUserAsync(registrationCode, model);
            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
            return View(model);
            }

            var result = await _accountService.LoginUserAsync(model);
            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Не вдалося ввійти.");
            return View(model);
        }

        [HttpGet]
        [Authorize]
        public IActionResult Delete()
        {
            return View();
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed()
        {
            var user = await _accountService.GetCurrentUserAsync();
            if (user == null)
            {
                return NotFound();
            }

            await _accountService.MarkAccountToDeleteAsync(user);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> CancelDelete()
        {
            var user = await _accountService.GetCurrentUserAsync();
            if (user == null)
            {
                return NotFound();
            }

            if (user.DeleteAt != null) return View(user);
            else return RedirectToAction("Index", "Home");
        }

        [HttpPost, ActionName("CancelDelete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> CancelDeleteConfirmed()
        {
            var user = await _accountService.GetCurrentUserAsync();
            if (user == null)
            {
                return NotFound();
            }

            await _accountService.CancelDeleteAccountAsync(user);

            return RedirectToAction("Index", "Home");
        }
    }
}