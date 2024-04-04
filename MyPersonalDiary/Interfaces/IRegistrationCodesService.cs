using Microsoft.AspNetCore.Identity;
using MyPersonalDiary.Models;
using MyPersonalDiary.ViewModels;

namespace MyPersonalDiary.Interfaces
{
    public interface IRegistrationCodesService
    {
        bool CheckRegistrationCode(string registrationCode);
        Task<List<RegistrationCode>> GetRegistrationCodesAsync();
        Task<string> CreateRegistrationCodeAsync();
    }
}
