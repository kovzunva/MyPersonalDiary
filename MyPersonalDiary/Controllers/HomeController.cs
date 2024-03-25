using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using MyPersonalDiary.Data;
using MyPersonalDiary.Models;
using MyPersonalDiary.Services;
using System.Diagnostics;

namespace MyPersonalDiary.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Authorize]
        public IActionResult NotFoundError()
        {
            return View("NotFound");
        }
        public IActionResult NotFoundGuestError()
        {
            return View("NotFoundGuest");
        }
    }

}