using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPersonalDiary.Data;
using MyPersonalDiary.Models;
using System.Diagnostics;

namespace MyPersonalDiary.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly EncryptionService encryptionService;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context,
            IConfiguration configuration)
        {
            _logger = logger;
            _context = context;
            encryptionService = new EncryptionService(configuration);
        }

        [Authorize]
        public async Task<IActionResult> Index(int page = 1)
        {
            int pageSize = 5;
            DateTime twoDaysAgo = DateTime.Now.AddDays(-2);

            string currentUserName = User.Identity.Name;
            var postsQuery = _context.Posts.Include(p => p.User)
                .Where(p => p.User.UserName == currentUserName)
                .OrderByDescending(p => p.CreatedAt);

            var posts = new PaginatedList<Post>(postsQuery, page, pageSize, "");
            ViewBag.PaginationString = posts.GetPaginationString();

            // Розшифрування
            foreach (var post in posts)
            {
                post.Content = encryptionService.Decrypt(post.Content);
                if (post.ImagePath != null) post.ImagePath = encryptionService.Decrypt(post.ImagePath);

                post.CanEditAndDelete = post.CreatedAt >= twoDaysAgo;
            }

            return View(posts);
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
    }
}