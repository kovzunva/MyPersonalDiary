using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyPersonalDiary.Models;
using MyPersonalDiary.ViewModels;

namespace MyPersonalDiary.Interfaces
{
    public interface ICaptchaService
    {
        Task<(string, FileContentResult)> GenerateCaptchaAsync();
    }
}
