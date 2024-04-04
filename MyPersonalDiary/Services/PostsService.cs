using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using MyPersonalDiary.Data;
using MyPersonalDiary.Interfaces;
using MyPersonalDiary.Models;
using MyPersonalDiary.Validators;

namespace MyPersonalDiary.Services
{
    public class PostsService : IPostsService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEncryptionService _encryptionService;
        private readonly IImageService _imageService;
        private readonly IValidator<Post> _postValidator;

        public PostsService(ApplicationDbContext context, IEncryptionService encryptionService, 
            IImageService imageService, IValidator<Post> postValidator)
        {
            _context = context;
            _encryptionService = encryptionService;
            _imageService = imageService;
            _postValidator = postValidator;
        }
        public Post EncryptPost(Post post)
        {
            post.Content = _encryptionService.Encrypt(post.Content);
            post.ImagePath = _encryptionService.Encrypt(post.ImagePath);
            return post;
        }
        public Post DecryptPost(Post post)
        {
            post.Content = _encryptionService.Decrypt(post.Content);
            post.ImagePath = _encryptionService.Decrypt(post.ImagePath);
            return post;
        }

        public async Task<List<Post>> GetPostsAsync(User user)
        {
            var currentUserName = user.UserName;
            var posts = await _context.Posts
                .Include(p => p.User)
                .Where(p => p.User.UserName == currentUserName)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            posts = DecryptAndCheckPosts(posts);

            return posts;
        }
        public async Task<List<Post>> GetPostsAsync(User user, string? searchTerm, DateTime? fromDate, DateTime? toDate)
        {
            string currentUserName = user.UserName;
            var posts = await _context.Posts
                .Include(p => p.User)
                .Where(p => p.User.UserName == currentUserName)
                .Where(p => fromDate == null || p.CreatedAt >= fromDate)
                .Where(p => toDate == null || p.CreatedAt <= toDate)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            posts = DecryptAndCheckPosts(posts);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                posts = posts
                .Where(p => p.Content.Contains(searchTerm))
                .ToList();
            }

            return posts;
        }

        public List<Post> DecryptAndCheckPosts(List<Post> posts)
        {
            DateTime twoDaysAgo = DateTime.Now.AddDays(-2);
            foreach (var post in posts)
            {
                post.Content = _encryptionService.Decrypt(post.Content);
                post.ImagePath = _encryptionService.Decrypt(post.ImagePath);
                post.CanEditAndDelete = post.CreatedAt >= twoDaysAgo;
            }

            return posts;
        }

        public List<string> ValidatePost(Post post)
        {
            ValidationResult validationResult = _postValidator.Validate(post);

            List<string> errors = new List<string>();
            foreach (ValidationFailure failure in validationResult.Errors)
            {
                errors.Add(failure.ErrorMessage);
            }

            return errors;
        }

        public async Task CreatePostAsync(User user, Post post, IFormFile? ImagePath)
        {
            if (ImagePath != null && ImagePath.Length > 0)
            {
                post.ImagePath = await _imageService.SaveImage(ImagePath);
            }

            post.UserId = user.Id;
            post.CreatedAt = DateTime.Now;
            post = EncryptPost(post);

            _context.Add(post);
            await _context.SaveChangesAsync();
        }

        public async Task<(Post? post, string? errorMessage)> GetPostAsync(User user, int? id)
        {
            if (id == null || _context.Posts == null)
            {
                return (null, "Пост не знайдено.");
            }

            var post = await _context.Posts
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (post == null)
            {
                return (null, "Пост не знайдено.");
            }

            string currentUserName = user.UserName;
            if (post.User.UserName != currentUserName)
            {
                return (null, "Ви не маєте прав для перегляду цього поста.");
            }

            DateTime twoDaysAgo = DateTime.Now.AddDays(-2);
            if (post.CreatedAt <= twoDaysAgo)
            {
                return (null, "Не можна змінювати пост, який старший двох днів.");
            }

            post = DecryptPost(post);

            return (post, null);
        }

        public async Task EditPostAsync(Post existingPost, Post post, IFormFile? ImagePath)
        {
            // Видалення старої картинки
            if (!string.IsNullOrEmpty(existingPost.ImagePath) && ImagePath != null && ImagePath.Length > 0)
            {                
                var oldImagePath = _imageService.GetImage(existingPost.ImagePath);
                await _imageService.DeleteImage(oldImagePath);
            }

            _context.Entry(post).Property(x => x.UserId).IsModified = false;
            _context.Entry(post).Property(x => x.CreatedAt).IsModified = false;

            // Оновлення і збереження нової картинки, якщо вона надійшла
            if (ImagePath != null && ImagePath.Length > 0)
            {
                var newImagePath = await _imageService.SaveImage(ImagePath);
                post.ImagePath = _encryptionService.Encrypt(newImagePath);
                _context.Entry(existingPost).Entity.ImagePath = post.ImagePath;
            }
            else
            {
                _context.Entry(existingPost).Property(x => x.ImagePath).IsModified = false;
            }

            // Шифрування
            post.Content = _encryptionService.Encrypt(post.Content);
            _context.Entry(existingPost).Entity.Content = post.Content;

            await _context.SaveChangesAsync();
        }

        public async Task DeletePostAsync(Post post)
        {
            if (!string.IsNullOrEmpty(post.ImagePath))
            {
                var ImagePath = _imageService.GetImage(post.ImagePath);
                await _imageService.DeleteImage(ImagePath);
            }
            _context.Posts.Remove(post);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteUserPostsAsync(User user)
        {
            var userPosts = await _context.Posts
                    .Include(p => p.User)
                    .Where(p => p.UserId == user.Id)
                    .ToListAsync();
            foreach (var post in userPosts)
            {
                await DeletePostAsync(post);
            }
        }
    }
}
