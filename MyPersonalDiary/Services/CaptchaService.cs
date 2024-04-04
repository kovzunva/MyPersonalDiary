//using SixLabors.ImageSharp;
using System.Drawing.Drawing2D;
using System.Drawing;
using Microsoft.AspNetCore.Mvc;
using System.Drawing.Imaging;
using MyPersonalDiary.Interfaces;

namespace MyPersonalDiary.Services
{
    public class CaptchaService : ICaptchaService
    {

        public CaptchaService() { }

        public async Task<(string, FileContentResult)> GenerateCaptchaAsync()
        {
            // Генерація випадкового тексту для капчі
            string captchaText = GenerateRandomText(6);

            // Генерація зображення капчі
            Bitmap bitmap = GenerateCaptchaImage(captchaText);

            MemoryStream stream = new MemoryStream();
            bitmap.Save(stream, ImageFormat.Png);

            FileContentResult fileContentResult = new FileContentResult(stream.ToArray(), "image/png");

            // Повернення зображення капчі та тексту у відповідь
            return (captchaText, fileContentResult);
        }

        private string GenerateRandomText(int length)
        {
            // Генерація випадкового тексту за заданою довжиною
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private Bitmap GenerateCaptchaImage(string text)
        {
            // Створення зображення для капчі
            int width = 200;
            int height = 50;
            Bitmap bitmap = new Bitmap(width, height);

            // Генерація випадкового фону
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.White);

                // Додавання випадкових ліній на фоні
                Pen pen = new Pen(Color.LightGray, 1);
                Random random = new Random();
                for (int i = 0; i < 10; i++)
                {
                    int x1 = random.Next(0, width);
                    int y1 = random.Next(0, height);
                    int x2 = random.Next(0, width);
                    int y2 = random.Next(0, height);
                    graphics.DrawLine(pen, x1, y1, x2, y2);
                }
            }

            // Додавання тексту на зображення
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                Font font = new Font("Arial", 20, FontStyle.Bold);

                // Отримання розмірів тексту
                SizeF textSize = graphics.MeasureString(text, font);

                // Обчислення позиції для розміщення тексту по центру зображення
                float x = (width - textSize.Width) / 2 + 10;
                float y = (height - textSize.Height) / 2 + 5;

                // Малювання тексту на зображенні
                graphics.DrawString(text, font, Brushes.Black, x, y);
            }

            // Додавання розмиття для підвищення складності капчі
            bitmap = ApplyDistortion(bitmap);

            return bitmap;
        }

        private Bitmap ApplyDistortion(Bitmap image)
        {
            // Коефіцієнти викривлення
            double amplitudeX = 20; // Амплітуда хвилі по горизонталі
            double amplitudeY = 5; // Амплітуда хвилі по вертикалі
            double frequencyX = 0.5; // Частота хвилі по горизонталі
            double frequencyY = 0.1; // Частота хвилі по вертикалі

            // Створення зображення для капчі
            Bitmap distortedImage = new Bitmap(image.Width, image.Height);

            // Застосування викривлення
            Color[,] pixels = new Color[image.Width, image.Height];

            // Заповнення масиву кольорів
            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    pixels[x, y] = image.GetPixel(x, y);
                }
            }

            // Застосування викривлення
            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    int newX = (int)(x + amplitudeX * Math.Sin(2 * Math.PI * frequencyX * y / image.Height));
                    int newY = (int)(y + amplitudeY * Math.Sin(2 * Math.PI * frequencyY * x / image.Width));

                    // Врахування границь зображення
                    newX = Math.Max(0, Math.Min(newX, image.Width - 1));
                    newY = Math.Max(0, Math.Min(newY, image.Height - 1));

                    distortedImage.SetPixel(x, y, pixels[newX, newY]);
                }
            }

            return distortedImage;
        }
    }
}
