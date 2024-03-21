using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPersonalDiary.Data;
using MyPersonalDiary.Models;


namespace MyPersonalDiary.Controllers
{
    [Authorize(Roles = "admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var registrationCodes = await _context.RegistrationCodes.ToListAsync();
            return View(registrationCodes);
        }

        [HttpPost]
        public async Task<IActionResult> Create()
        {
            string newCode = RegistrationCode.GenerateRegistrationCode();
            var newRegistrationCode = new RegistrationCode { Code = newCode };
            _context.RegistrationCodes.Add(newRegistrationCode);
            await _context.SaveChangesAsync();

            string link = Url.Action("Register", "Account", new { registrationCode = newCode }, "https");

            return Json(new { code = newCode, link = link });
        }

    }
}
