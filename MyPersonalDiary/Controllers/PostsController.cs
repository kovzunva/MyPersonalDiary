﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyPersonalDiary.Data;
using MyPersonalDiary.Data.Migrations;
using MyPersonalDiary.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.IO;
using SixLabors.ImageSharp.Formats.Jpeg;
using Microsoft.Extensions.Hosting;
using MyPersonalDiary.Services;

namespace MyPersonalDiary.Controllers
{
    [Authorize]
    public class PostsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly EncryptionService encryptionService;

        public PostsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment,
            IConfiguration configuration)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            encryptionService = new EncryptionService(configuration);
        }

        // GET: Posts
        [Route("/")]
        public async Task<IActionResult> Index(int page = 1, string searchTerm = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            int pageSize = 5;
            DateTime twoDaysAgo = DateTime.Now.AddDays(-2);

            string currentUserName = User.Identity.Name;
            var posts = await _context.Posts
                .Include(p => p.User)
                .Where(p => p.User.UserName == currentUserName)
                .Where(p => fromDate == null || p.CreatedAt >= fromDate)
                .Where(p => toDate == null || p.CreatedAt <= toDate)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            foreach (var post in posts)
            {
                post.Content = encryptionService.Decrypt(post.Content);
                post.ImagePath = encryptionService.Decrypt(post.ImagePath);
                post.CanEditAndDelete = post.CreatedAt >= twoDaysAgo;
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                posts = posts
                .Where(p => p.Content.Contains(searchTerm))
                .ToList();
            }

            var currentUrl = HttpContext.Request.Path + HttpContext.Request.QueryString;
            var paginatedPosts = new PaginatedList<Post>(posts, page, pageSize, currentUrl);
            ViewBag.PaginationString = paginatedPosts.GetPaginationString();

            return View(paginatedPosts);

        }

        private async Task<string> SaveImage(IFormFile imageFile)
        {
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
            var uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                using (var image = Image.Load(imageFile.OpenReadStream()))
                {
                    // Перевіряємо розмір картинки та стискаємо, якщо потрібно
                    if (image.Width > 1024 || image.Height > 1024)
                    {
                        image.Mutate(x => x.Resize(new ResizeOptions
                        {
                            Size = new Size(1024, 1024),
                            Mode = ResizeMode.Max
                        }));
                    }

                    // Зберігаємо оптимізовану версію в файловий потік
                    await image.SaveAsync(fileStream, new JpegEncoder { Quality = 80 });
                }
            }

            return uniqueFileName;
        }

        // GET: Posts/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Posts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Content,ImagePath")] Post post, 
            IFormFile? ImagePath)
        {
            ModelState.Remove("UserId");
            ModelState.Remove("User");

            if (post.Content.Length > 500)
            {
                ModelState.AddModelError("post.Content", "Пост повинен бути не більше 500 символів");
                ViewBag.ValidationErrors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return View(post);
            }

            if (ModelState.IsValid)
            {
                if (ImagePath != null && ImagePath.Length > 0)
                {
                    post.ImagePath = await SaveImage(ImagePath);
                }

                post.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                post.CreatedAt = DateTime.Now;

                // Шифрування
                post.Content = encryptionService.Encrypt(post.Content);
                post.ImagePath = encryptionService.Encrypt(post.ImagePath);

                _context.Add(post);

                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Home");
            }

            // Вивести всі помилки валідації
            ViewBag.ValidationErrors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);

            return View(post);
        }

        // GET: Posts/Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Posts == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == id);
            if (post == null)
            {
                return NotFound();
            }

            string currentUserName = User.Identity.Name;
            if (post.User.UserName != currentUserName)
            {
                var errorMessage = "Ви не маєте прав для редагування цього поста.";
                TempData["ErrorMessage"] = errorMessage;
                return View("Error");
            }

            DateTime twoDaysAgo = DateTime.Now.AddDays(-2);
            if (post.CreatedAt <= twoDaysAgo)
            {
                var errorMessage = "Не можна редагувати пост, який старший двох днів.";
                TempData["ErrorMessage"] = errorMessage;
                return View("Error");
            }

            /* Розшифрування */
            post.Content = encryptionService.Decrypt(post.Content);
            if (post.ImagePath != null) post.ImagePath = encryptionService.Decrypt(post.ImagePath);

            return View(post);
        }

        // POST: Posts/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Content,ImagePath")] Post post,
            IFormFile? ImagePath)
        {
            if (id != post.Id)
            {
                return NotFound();
            }

            var existingPost = await _context.Posts
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (existingPost == null)
            {
                return NotFound();
            }

            string currentUserName = User.Identity.Name;
            if (existingPost.User.UserName != currentUserName)
            {
                var errorMessage = "Ви не маєте прав для редагування цього поста.";
                TempData["ErrorMessage"] = errorMessage;
                return View("Error");
            }

            DateTime twoDaysAgo = DateTime.Now.AddDays(-2);
            if (existingPost.CreatedAt <= twoDaysAgo)
            {
                var errorMessage = "Не можна редагувати пост, який старший двох днів.";
                TempData["ErrorMessage"] = errorMessage;
                return View("Error");
            }

            ModelState.Remove("UserId");
            ModelState.Remove("User");

            if (post.Content.Length > 500)
            {
                ModelState.AddModelError("post.Content", $"Пост повинен бути не більше 500 символів. Поточна довжина: {post.Content.Length}");                // Вивести всі помилки валідації вручну
                ViewBag.ValidationErrors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return View(post);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Видалення старої картинки
                    if (!string.IsNullOrEmpty(existingPost.ImagePath) && ImagePath != null && ImagePath.Length > 0)
                    {
                        var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads",
                            encryptionService.Decrypt(existingPost.ImagePath));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    _context.Entry(post).Property(x => x.UserId).IsModified = false;
                    _context.Entry(post).Property(x => x.CreatedAt).IsModified = false;

                    // Оновлення і збереження нової картинки, якщо вона надійшла
                    if (ImagePath != null && ImagePath.Length > 0)
                    {
                        var newImagePath = await SaveImage(ImagePath);
                        post.ImagePath = encryptionService.Encrypt(newImagePath);
                        _context.Entry(existingPost).Entity.ImagePath = post.ImagePath;
                    }
                    else
                    {
                        _context.Entry(existingPost).Property(x => x.ImagePath).IsModified = false;
                    }

                    // Шифрування
                    post.Content = encryptionService.Encrypt(post.Content);
                    _context.Entry(existingPost).Entity.Content = post.Content;

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostExists(post.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index", "Home");
            }
            return View(post);
        }

        // GET: Posts/Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Posts == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (post == null)
            {
                return NotFound();
            }

            string currentUserName = User.Identity.Name;
            if (post.User.UserName != currentUserName)
            {
                var errorMessage = "Ви не маєте прав для видалення цього поста.";
                TempData["ErrorMessage"] = errorMessage;
                return View("Error");
            }

            DateTime twoDaysAgo = DateTime.Now.AddDays(-2);
            if (post.CreatedAt <= twoDaysAgo)
            {
                var errorMessage = "Не можна видалити пост, який старший двох днів.";
                TempData["ErrorMessage"] = errorMessage;
                return View("Error");
            }

            /* Розшифрування */
            post.Content = encryptionService.Decrypt(post.Content);
            if (post.ImagePath != null) post.ImagePath = encryptionService.Decrypt(post.ImagePath);

            return View(post);
        }

        // POST: Posts/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Posts == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Posts'  is null.");
            }
            var post = await _context.Posts
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == id);
            if (post != null)
            {
                string currentUserName = User.Identity.Name;
                if (post.User.UserName != currentUserName)
                {
                    var errorMessage = "Ви не маєте прав для видалення цього поста.";
                    TempData["ErrorMessage"] = errorMessage;
                    return View("Error");
                }

                DateTime twoDaysAgo = DateTime.Now.AddDays(-2);
                if (post.CreatedAt <= twoDaysAgo)
                {
                    var errorMessage = "Не можна видалити пост, який старший двох днів.";
                    TempData["ErrorMessage"] = errorMessage;
                    return View("Error");
                }

                if (!string.IsNullOrEmpty(post.ImagePath))
                {
                    var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads",
                                encryptionService.Decrypt(post.ImagePath));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }
                _context.Posts.Remove(post);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Home");
        }

        private bool PostExists(int id)
        {
          return (_context.Posts?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
