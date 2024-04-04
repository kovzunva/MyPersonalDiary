using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPersonalDiary.Interfaces;

namespace MyPersonalDiary.Controllers
{
    [Authorize(Roles = "admin")]
    public class AdminController : Controller
    {
        private readonly IRegistrationCodesService _registrationCodesService;

        public AdminController(IRegistrationCodesService registrationCodesService)
        {
            _registrationCodesService = registrationCodesService;
        }

        public async Task<IActionResult> Index()
        {
            var registrationCodes = await _registrationCodesService.GetRegistrationCodesAsync();
            return View(registrationCodes);
        }

        [HttpPost]
        public async Task<IActionResult> Create()
        {
            string newCode = await _registrationCodesService.CreateRegistrationCodeAsync();
            string newLink = Url.Action("Register", "Account", new { registrationCode = newCode }, "https");

            return Json(new { code = newCode, link = newLink });
        }

    }
}
