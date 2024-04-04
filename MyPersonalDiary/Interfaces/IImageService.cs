namespace MyPersonalDiary.Interfaces
{
    public interface IImageService
    {
        Task<string> SaveImage(IFormFile imageFile);
        string? GetImage(string imagePath);
        Task DeleteImage(string imagePath);
    }
}
