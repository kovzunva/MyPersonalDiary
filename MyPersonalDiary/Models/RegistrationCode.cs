using MyPersonalDiary.Data;
using System;
using System.Linq;

namespace MyPersonalDiary.Models
{
    public class RegistrationCode
    {
        public int Id { get; set; }
        public string Code { get; set; }

        // ����� ��� ��������� ���������� ����
        public static string GenerateRegistrationCode()
        {
            string registrationCode = Guid.NewGuid().ToString("N").Substring(0, 8);
            return registrationCode;
        }

        // ����� ��� �������� ���������� ���� � ��� �����
        public static bool IsRegistrationCodeValid(string registrationCode, ApplicationDbContext dbContext)
        {
            return dbContext.RegistrationCodes.Any(code => code.Code == registrationCode);
        }

    }
}
