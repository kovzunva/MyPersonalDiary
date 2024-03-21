using System.ComponentModel.DataAnnotations;

namespace MyPersonalDiary.Models
{
    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "���������� �����")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "ͳ�����")]
        public string NickName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "������")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "ϳ����������� ������")]
        [Compare("Password", ErrorMessage = "������ �� ���� ������������ �� ����������.")]
        public string ConfirmPassword { get; set; }

        [Required]
        [Display(Name = "�����")]
        public string? UserCaptcha { get; set; }
    }
}