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

        [HttpGet("{user_id}")]
        public IEnumerable<Post> GetPosts(string user_id)
        {
            var posts = _context.Posts.Where(post => post.UserId == user_id).ToList();
            foreach (var post in posts)
            {
                post.Content = encryptionService.Decrypt(post.Content);
                post.ImagePath = encryptionService.Decrypt(post.ImagePath);
            }
            return posts;
        }

        [HttpGet("{user_id}/{id}")]
        public ActionResult<Post> GetPost(string user_id, int id)
        {
            var post = _context.Posts.Find(id);

            if (post == null || post.UserId != user_id)
            {
                return NotFound();
            }

            post.Content = encryptionService.Decrypt(post.Content);
            post.ImagePath = encryptionService.Decrypt(post.ImagePath);

            return post;
        }

        [HttpPost]
        public ActionResult<Post> CreatePost(Post post)
        {
            if (string.IsNullOrWhiteSpace(post.UserId) || string.IsNullOrWhiteSpace(post.Content))
            {
                ModelState.AddModelError(string.Empty, "Не заповнені обов'язкові поля");
                return BadRequest(ModelState);
            }

            if (post.Content.Length > 500)
            {
                ModelState.AddModelError("post.Content", "Пост повинен бути не більше 500 символів");
                return BadRequest(ModelState);
            }

            post.CreatedAt = DateTime.Now;
            post.Content = encryptionService.Encrypt(post.Content);
            post.ImagePath = encryptionService.Encrypt(post.ImagePath);
            _context.Posts.Add(post);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetPost), new { id = post.Id }, post);
        }

        [HttpPut("{user_id}/{id}")]
        public IActionResult UpdatePost(string user_id, int id, Post post)
        {
            if (id != post.Id || post.UserId != user_id)
            {
                return BadRequest();
            }

            if (DateTime.Now.Subtract(post.CreatedAt).Days > 2)
            {
                ModelState.AddModelError(string.Empty, "Не можна оновлювати пост, який старший двох днів");
                return BadRequest(ModelState);
            }

            if (string.IsNullOrWhiteSpace(post.UserId) || string.IsNullOrWhiteSpace(post.Content))
            {
                ModelState.AddModelError(string.Empty, "Не заповнені обов'язкові поля");
                return BadRequest(ModelState);
            }

            if (post.Content.Length > 500)
            {
                ModelState.AddModelError("post.Content", "Пост повинен бути не більше 500 символів");
                return BadRequest(ModelState);
            }

            post.Content = encryptionService.Encrypt(post.Content);
            post.ImagePath = encryptionService.Encrypt(post.ImagePath);
            _context.Entry(post).State = EntityState.Modified;
            _context.SaveChanges();

            return NoContent();
        }

        [HttpDelete("{user_id}/{id}")]
        public IActionResult DeletePost(string user_id, int id)
        {
            var post = _context.Posts.Find(id);

            if (post == null || post.UserId != user_id)
            {
                return NotFound();
            }

            if (DateTime.Now.Subtract(post.CreatedAt).Days > 2)
            {
                ModelState.AddModelError(string.Empty, "Не можна видаляти пост, який старший двох днів");
                return BadRequest(ModelState);
            }

            _context.Posts.Remove(post);
            _context.SaveChanges();

            return NoContent();
        }
    }
}
