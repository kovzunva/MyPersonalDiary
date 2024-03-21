using System.ComponentModel.DataAnnotations;

namespace MyPersonalDiary.Models
{
    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Електронна пошта")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Нікнейм")]
        public string NickName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Підтвердження паролю")]
        [Compare("Password", ErrorMessage = "Пароль та його підтвердження не співпадають.")]
        public string ConfirmPassword { get; set; }

        [Required]
        [Display(Name = "Капча")]
        public string? UserCaptcha { get; set; }
    }
}