using Microsoft.AspNetCore.Identity;
using MyPersonalDiary.Models;
using MyPersonalDiary.ViewModels;

namespace MyPersonalDiary.Interfaces
{
    public interface IAccountService
    {
        Task<User> GetCurrentUserAsync();
        User GetUserByApiKey(string api_key);
        Task<IdentityResult> CreateUserAsync(string registrationCode, RegisterViewModel model);
        Task<SignInResult> LoginUserAsync(LoginViewModel model);
        Task MarkAccountToDeleteAsync(User user);
        Task CancelDeleteAccountAsync(User user);
        Task<bool> DeleteAccountAsync(User user);
        Task<List<User>> GetUsersForDeletionAsync();
    }
}
