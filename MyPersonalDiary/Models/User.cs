using Microsoft.AspNetCore.Identity;

namespace MyPersonalDiary.Models
{
    public class User : IdentityUser
    {
        public string NickName { get; set; }
        public DateTime? DeleteAt { get; set; }
    }
}