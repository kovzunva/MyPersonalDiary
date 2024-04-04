using Microsoft.AspNetCore.Identity;
using MyPersonalDiary.Models;
using MyPersonalDiary.ViewModels;

namespace MyPersonalDiary.Interfaces
{
    public interface IPostsService
    {
        Post EncryptPost(Post post);
        Post DecryptPost(Post post);
        Task<List<Post>> GetPostsAsync(User user);
        Task<List<Post>> GetPostsAsync(User user, string? searchTerm, DateTime? fromDate, DateTime? toDate);
        List<string> ValidatePost(Post post);
        Task CreatePostAsync(User user, Post post, IFormFile? ImagePath);
        Task<(Post? post, string? errorMessage)> GetPostAsync(User user, int? id);
        Task EditPostAsync(Post existingPost, Post post, IFormFile? ImagePath);
        Task DeletePostAsync(Post post);
        Task DeleteUserPostsAsync(User user);
    }
}
