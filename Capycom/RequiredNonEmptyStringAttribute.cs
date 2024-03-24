using System.ComponentModel.DataAnnotations;

namespace Capycom
{
    public class RequiredNonEmptyStringAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                //ErrorMessage = "Поле не может быть пустым или состоять только из пробелов.";
                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }
    }
}
