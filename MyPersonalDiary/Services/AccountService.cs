using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyPersonalDiary.Data;
using MyPersonalDiary.Models;
using MyPersonalDiary.ViewModels;
using MyPersonalDiary.Interfaces;

namespace MyPersonalDiary.Services
{
    public class AccountService : IAccountService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPostsService _postsService;

        public AccountService(ApplicationDbContext context, UserManager<User> userManager,
            IHttpContextAccessor httpContextAccessor, SignInManager<User> signInManager,
            IPostsService postsService)
        {
            _context = context;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _signInManager = signInManager;
            _postsService = postsService;
        }
        public async Task<User> GetCurrentUserAsync()
        {
            var currentUser = _httpContextAccessor.HttpContext?.User;

            if (currentUser != null && currentUser.Identity != null && currentUser.Identity.IsAuthenticated)
            {
                var userId = _userManager.GetUserId(currentUser);
                var user = await _userManager.FindByIdAsync(userId);

                return user;
            }

            return null;
        }

        public User GetUserByApiKey(string api_key)
        { 
            return _context.Users.FirstOrDefault(u => u.ApiKey == api_key);
        }

        public async Task<IdentityResult> CreateUserAsync(string registrationCode, RegisterViewModel model)
        {
            var user = new User
            {
                UserName = model.Email,
                Email = model.Email,
                NickName = model.NickName,
                ApiKey = GenerateApiKey()
            };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Видалення реєстраційного коду
                var codeToDelete = await _context.RegistrationCodes.FirstOrDefaultAsync(c => c.Code == registrationCode);
                if (codeToDelete != null)
                {
                    _context.RegistrationCodes.Remove(codeToDelete);
                    await _context.SaveChangesAsync();
                }

                await _signInManager.SignInAsync(user, isPersistent: false);
            }

            return result;
        }

        public string GenerateApiKey()
        {
            string apiKey = Guid.NewGuid().ToString("N").Substring(0, 32);

            return apiKey;
        }

        public async Task<SignInResult> LoginUserAsync(LoginViewModel model)
        {
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
            return result;
        }

        public async Task MarkAccountToDeleteAsync(User user)
        {
            user.DeleteAt = DateTime.Now.AddDays(2);
            await _userManager.UpdateAsync(user);
        }

        public async Task CancelDeleteAccountAsync(User user)
        {
            user.DeleteAt = null;
            await _userManager.UpdateAsync(user);
        }

        public async Task<bool> DeleteAccountAsync(User user)
        {
            if (user == null)
                return false;

            if (user.DeleteAt == null || DateTime.Now <= user.DeleteAt)
            {            
                return false;
            }

            await _postsService.DeleteUserPostsAsync(user);

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                var devices = await _userManager.GetLoginsAsync(user);
                foreach (var device in devices)
                {
                    await _userManager.RemoveLoginAsync(user, device.LoginProvider, device.ProviderKey);
                }
                return true;
            }
            return false;
        }

        public async Task<List<User>> GetUsersForDeletionAsync()
        {
            return await _context.Users.Where(u => u.DeleteAt != null && u.DeleteAt <= DateTime.Now).ToListAsync();
        }
    }
}
