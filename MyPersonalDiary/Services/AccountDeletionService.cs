using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using MyPersonalDiary.Models;
using MyPersonalDiary.Data;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

namespace MyPersonalDiary.Services
{
    public class AccountDeletionService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ApplicationDbContext _dbContext;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly EncryptionService encryptionService;

        public AccountDeletionService(UserManager<User> userManager, SignInManager<User> signInManager,
            ApplicationDbContext dbContext, IWebHostEnvironment webHostEnvironment,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _dbContext = dbContext;
            _webHostEnvironment = webHostEnvironment;
            encryptionService = new EncryptionService(configuration);
        }

        public async Task<bool> DeleteAccount(User user)
        {
            if (user == null)
                return false;

            if (user.DeleteAt != null && DateTime.Now >= user.DeleteAt)
            {
                var userPosts = await _dbContext.Posts
                    .Include(p => p.User)
                    .Where(p => p.UserId == user.Id)
                    .ToListAsync();

                foreach (var post in userPosts)
                {
                    if (!string.IsNullOrEmpty(post.ImagePath))
                    {
                        var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads",
                            encryptionService.Decrypt(post.ImagePath));
                        if (System.IO.File.Exists(imagePath))
                        {
                            System.IO.File.Delete(imagePath);
                        }
                    }
                    _dbContext.Posts.Remove(post);
                }

                await _dbContext.SaveChangesAsync();

                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    await _signInManager.SignOutAsync();
                    return true;
                }
            }

            return false;
        }
    }

}
