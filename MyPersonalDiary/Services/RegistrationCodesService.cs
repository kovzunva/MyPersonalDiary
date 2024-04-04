using Microsoft.EntityFrameworkCore;
using MyPersonalDiary.Data;
using MyPersonalDiary.Interfaces;
using MyPersonalDiary.Models;

namespace MyPersonalDiary.Services
{
    public class RegistrationCodesService : IRegistrationCodesService
    {
        private readonly ApplicationDbContext _context;

        public RegistrationCodesService(ApplicationDbContext context)
        {
            _context = context;
        }

        public bool CheckRegistrationCode(string registrationCode)
        {
            return _context.RegistrationCodes.Any(code => code.Code == registrationCode);
        }

        public async Task<List<RegistrationCode>> GetRegistrationCodesAsync()
        {
            return await _context.RegistrationCodes.ToListAsync();
        }

        public async Task<string> CreateRegistrationCodeAsync()
        {
            string newCode = RegistrationCode.GenerateRegistrationCode();
            var newRegistrationCode = new RegistrationCode { Code = newCode };
            _context.RegistrationCodes.Add(newRegistrationCode);
            await _context.SaveChangesAsync();

            return newCode;
        }
    }
}
