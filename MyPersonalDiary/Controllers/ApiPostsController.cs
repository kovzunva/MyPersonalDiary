using System;
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
    [Route("api/posts")]
    [ApiController]
    public class ApiPostsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly EncryptionService encryptionService;

        public ApiPostsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment,
            IConfiguration configuration)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            encryptionService = new EncryptionService(configuration);
        }

        [HttpGet]
        public ActionResult<IEnumerable<Post>> GetPosts([FromHeader(Name = "api_key")] string api_key)
        {
            var user = _context.Users.FirstOrDefault(u => u.ApiKey == api_key);
            if (user == null)
            {
                return Unauthorized();
            }

            var posts = _context.Posts.Where(post => post.UserId == user.Id).ToList();
            foreach (var post in posts)
            {
                post.Content = encryptionService.Decrypt(post.Content);
                post.ImagePath = encryptionService.Decrypt(post.ImagePath);
            }
            return posts;
        }

        [HttpGet("{id}")]
        public ActionResult<Post> GetPost([FromHeader(Name = "api_key")] string api_key, int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.ApiKey == api_key);
            if (user == null)
            {
                return Unauthorized();
            }

            var post = _context.Posts.Find(id);

            if (post == null || post.UserId != user.Id)
            {
                return NotFound();
            }

            post.Content = encryptionService.Decrypt(post.Content);
            post.ImagePath = encryptionService.Decrypt(post.ImagePath);

            return post;
        }

        [HttpPost]
        public ActionResult<Post> CreatePost([FromHeader(Name = "api_key")] string api_key, [FromBody] Post post, IFormFile? imageFile)
        {
            var user = _context.Users.FirstOrDefault(u => u.ApiKey == api_key);
            if (user == null)
            {
                return Unauthorized();
            }

            if (post == null || string.IsNullOrWhiteSpace(post.Content))
            {
                ModelState.AddModelError(string.Empty, "Не заповнені обов'язкові поля");
                return BadRequest(ModelState);
            }

            if (post.Content.Length > 500)
            {
                ModelState.AddModelError("post.Content", "Пост повинен бути не більше 500 символів");
                return BadRequest(ModelState);
            }

            post.UserId = user.Id;
            post.CreatedAt = DateTime.Now;
            if (imageFile != null && imageFile.Length > 0)
            {
                post.ImagePath = SaveImage(imageFile);
            }

            // Шифрування
            post.Content = encryptionService.Encrypt(post.Content);
            post.ImagePath = encryptionService.Encrypt(post.ImagePath);

            _context.Posts.Add(post);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetPost), new { id = post.Id }, post);
        }

        private string SaveImage(IFormFile imageFile)
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
                    image.Save(fileStream, new JpegEncoder { Quality = 80 });
                }
            }

            return uniqueFileName;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> EditPost([FromHeader(Name = "api_key")] string api_key, int id, [FromBody] Post post, IFormFile? imageFile)
        {
            var user = _context.Users.FirstOrDefault(u => u.ApiKey == api_key);
            if (user == null)
            {
                return Unauthorized();
            }

            if (post == null || id != post.Id || post.UserId != user.Id)
            {
                return BadRequest();
            }

            var existingPost = await _context.Posts.FirstOrDefaultAsync(p => p.Id == id);

            if (existingPost == null)
            {
                return NotFound();
            }

            if (DateTime.Now.Subtract(existingPost.CreatedAt).Days > 2)
            {
                ModelState.AddModelError(string.Empty, "Не можна редагувати пост, який старший двох днів");
                return BadRequest(ModelState);
            }

            if (post.Content.Length > 500)
            {
                ModelState.AddModelError("post.Content", "Пост повинен бути не більше 500 символів");
                return BadRequest(ModelState);
            }

            try
            {
                // Видалення старої картинки
                if (!string.IsNullOrEmpty(existingPost.ImagePath) && imageFile != null && imageFile.Length > 0)
                {
                    var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads",
                        encryptionService.Decrypt(existingPost.ImagePath));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                // Оновлення і збереження нової картинки, якщо вона надійшла
                if (imageFile != null && imageFile.Length > 0)
                {
                    var newImagePath = SaveImage(imageFile);
                    post.ImagePath = encryptionService.Encrypt(newImagePath);
                    existingPost.ImagePath = post.ImagePath;
                }
                else
                {
                    _context.Entry(existingPost).Property(x => x.ImagePath).IsModified = false;
                }

                existingPost.Content = encryptionService.Encrypt(post.Content);

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }
            return Content("Пост успішно відредагованр");
        }


        [HttpDelete("{id}")]
        public IActionResult DeletePost([FromHeader(Name = "api_key")] string api_key, int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.ApiKey == api_key);
            if (user == null)
            {
                return Unauthorized();
            }

            var post = _context.Posts.Find(id);

            if (post == null || post.UserId != user.Id)
            {
                return NotFound();
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
            _context.SaveChanges();

            return Content("Пост успішно видалено");
        }
    }
}
