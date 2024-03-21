using System.ComponentModel.DataAnnotations;

namespace MyPersonalDiary.Models
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "�����'����� ����?")]
        public bool RememberMe { get; set; }
    }

}