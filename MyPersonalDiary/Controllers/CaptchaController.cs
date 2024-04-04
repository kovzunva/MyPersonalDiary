using Microsoft.AspNetCore.Mvc;
using MyPersonalDiary.Interfaces;

namespace MyPersonalDiary.Controllers
{
    public class CaptchaController : Controller
    {
        private readonly ICaptchaService _captchaService; 
        public CaptchaController(ICaptchaService captchaService)
        {
            _captchaService = captchaService;
        }

        public async Task<IActionResult> GenerateCaptcha()
        {
            var (captchaText, captchaImage) = await _captchaService.GenerateCaptchaAsync();

            // Зберігання тексту капчі в сеансі користувача
            HttpContext.Session.SetString("Captcha", captchaText);

            return captchaImage;
        }        
    }
}