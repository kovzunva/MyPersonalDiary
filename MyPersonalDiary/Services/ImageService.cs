using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using MyPersonalDiary.Interfaces;

namespace MyPersonalDiary.Services
{
    public class ImageService : IImageService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ImageService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<string> SaveImage(IFormFile imageFile)
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

        public string? GetImage(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
            {
                return null;
            }   

            string fullPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", imagePath);
            return fullPath;
        }


        public async Task DeleteImage(string imagePath)
        {
            if (!string.IsNullOrEmpty(imagePath) && System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }
        }
    }
}
