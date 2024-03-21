using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing;

namespace MyPersonalDiary.Models
{
    public class Captcha
    {
        public string? CaptchaText { get; set; }
        public byte[]? CaptchaImage { get; set; }

        public Captcha()
        {
            // ��������� ����������� ������ ��� �����
            CaptchaText = GenerateRandomText(6);

            // ��������� ���������� �����
            Bitmap bitmap = GenerateCaptchaImage(CaptchaText);

            // ���� ��� ���������� ����������
            MemoryStream stream = new MemoryStream();
            bitmap.Save(stream, ImageFormat.Png);

            // ���������� ���������� ����� � �������
            CaptchaImage = stream.ToArray();
        }

        private string GenerateRandomText(int length)
        {
            // ��������� ����������� ������ �� ������� ��������
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private Bitmap GenerateCaptchaImage(string text)
        {
            // ��������� ���������� ��� �����
            int width = 200;
            int height = 50;
            Bitmap bitmap = new Bitmap(width, height);

            // ��������� ����������� ����
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.White);

                // ��������� ���������� ��� �� ���
                Pen pen = new Pen(Color.LightGray, 1);
                Random random = new Random(); // ��������� ���������� ����� Random
                for (int i = 0; i < 10; i++)
                {
                    int x1 = random.Next(0, width); // ������������ ���������� ����� Random ��� ��������� ���������� �����
                    int y1 = random.Next(0, height);
                    int x2 = random.Next(0, width);
                    int y2 = random.Next(0, height);
                    graphics.DrawLine(pen, x1, y1, x2, y2);
                }
            }

            // ��������� ������ �� ����������
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                Font font = new Font("Arial", 20, FontStyle.Bold);

                // ��������� ������ ������
                SizeF textSize = graphics.MeasureString(text, font);

                // ���������� ������� ��� ��������� ������ �� ������ ����������
                float x = (width - textSize.Width) / 2 + 10;
                float y = (height - textSize.Height) / 2 + 5;

                // ��������� ������ �� ���������
                graphics.DrawString(text, font, Brushes.Black, x, y);
            }

            // ��������� �������� ��� ��������� ��������� �����
            bitmap = ApplyDistortion(bitmap);

            return bitmap;
        }

        private Bitmap ApplyDistortion(Bitmap image)
        {
            // ����������� �����������
            double amplitudeX = 20; // �������� ���� �� ����������
            double amplitudeY = 5; // �������� ���� �� ��������
            double frequencyX = 0.5; // ������� ���� �� ����������
            double frequencyY = 0.1; // ������� ���� �� ��������

            // ��������� ���������� ��� �����
            Bitmap distortedImage = new Bitmap(image.Width, image.Height);

            // ������������ �����������
            Color[,] pixels = new Color[image.Width, image.Height];

            // ���������� ������ �������
            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    pixels[x, y] = image.GetPixel(x, y);
                }
            }

            // ������������ �����������
            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    int newX = (int)(x + amplitudeX * Math.Sin(2 * Math.PI * frequencyX * y / image.Height));
                    int newY = (int)(y + amplitudeY * Math.Sin(2 * Math.PI * frequencyY * x / image.Width));

                    // ���������� ������� ����������
                    newX = Math.Max(0, Math.Min(newX, image.Width - 1));
                    newY = Math.Max(0, Math.Min(newY, image.Height - 1));

                    distortedImage.SetPixel(x, y, pixels[newX, newY]);
                }
            }

            return distortedImage;
        }
    }
}