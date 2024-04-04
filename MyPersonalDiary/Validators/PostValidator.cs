using FluentValidation;
using MyPersonalDiary.Models;

namespace MyPersonalDiary.Validators
{
    public class PostValidator : AbstractValidator<Post>
    {
        public PostValidator()
        {
            RuleFor(post => post.Content)
                .NotEmpty().WithMessage("Контент не може бути пустим.")
                .MaximumLength(500).WithMessage("Контент не може перевищувати 500 символів.");
        }
    }
}
