using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPersonalDiary.Models;
using MyPersonalDiary.Interfaces;

namespace MyPersonalDiary.Controllers
{
    [Authorize]
    public class PostsController : Controller
    {
        private readonly IPostsService _postsService;
        private readonly IAccountService _accountService;

        public PostsController(IPostsService postsService, IAccountService accountService)
        {
            _postsService = postsService;
            _accountService = accountService;
        }

        // GET: Posts
        [Route("/")]
        public async Task<IActionResult> Index(int page = 1, string? searchTerm = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            User currentUser = await _accountService.GetCurrentUserAsync();
            List<Post> posts = await _postsService.GetPostsAsync(currentUser, searchTerm, fromDate, toDate);

            int pageSize = 5;
            var currentUrl = HttpContext.Request.Path + HttpContext.Request.QueryString;
            var paginatedPosts = new PaginatedList<Post>(posts, page, pageSize, currentUrl);
            ViewBag.PaginationString = paginatedPosts.GetPaginationString();

            return View(paginatedPosts);
        }

        // GET: Posts/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Posts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Post post, IFormFile? ImagePath)
        {
            List<string> errors = _postsService.ValidatePost(post);
            if (errors.Any())
            {
                ViewBag.ValidationErrors = errors;
                return View(post);
            }

            User currentUser = await _accountService.GetCurrentUserAsync();
            await _postsService.CreatePostAsync(currentUser, post, ImagePath);
            return RedirectToAction("Index", "Home");
        }

        // GET: Posts/Edit
        public async Task<IActionResult> Edit(int? id)
        {
            User currentUser = await _accountService.GetCurrentUserAsync();
            var (post, errorMessage) = await _postsService.GetPostAsync(currentUser, id);
            if (errorMessage != null)
            {
                TempData["ErrorMessage"] = errorMessage;
                return View("Error");
            }

            return View(post);
        }

        // POST: Posts/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Post post, IFormFile? ImagePath)
        {
            User currentUser = await _accountService.GetCurrentUserAsync();
            var (existingPost, errorMessage) = await _postsService.GetPostAsync(currentUser, id);
            if (errorMessage != null)
            {
                TempData["ErrorMessage"] = errorMessage;
                return View("Error");
            }

            List<string> errors = _postsService.ValidatePost(post);
            if (errors.Any())
            {
                ViewBag.ValidationErrors = errors;
                return View(post);
            }

            await _postsService.EditPostAsync(existingPost, post, ImagePath);
            
            return RedirectToAction("Index", "Home");
        }

        // GET: Posts/Delete
        public async Task<IActionResult> Delete(int? id)
        {
            User currentUser = await _accountService.GetCurrentUserAsync();
            var (post, errorMessage) = await _postsService.GetPostAsync(currentUser, id);
            if (errorMessage != null)
            {
                TempData["ErrorMessage"] = errorMessage;
                return View("Error");
            }

            return View(post);
        }

        // POST: Posts/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            User currentUser = await _accountService.GetCurrentUserAsync();
            var (post, errorMessage) = await _postsService.GetPostAsync(currentUser, id);
            if (errorMessage != null)
            {
                TempData["ErrorMessage"] = errorMessage;
                return View("Error");
            }

            await _postsService.DeletePostAsync(post);

            return RedirectToAction("Index", "Home");
        }
    }
}
