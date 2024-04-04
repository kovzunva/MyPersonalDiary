using Microsoft.AspNetCore.Identity;
using MyPersonalDiary.Models;
using MyPersonalDiary.ViewModels;

namespace MyPersonalDiary.Interfaces
{
    public interface IEncryptionService
    {
        string Encrypt(string plainText);
        string Decrypt(string cipherText);
    }
}
