using System.ComponentModel.DataAnnotations;

namespace Capycom
{
    public class FutureDateAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if(value == null)
            {
                return ValidationResult.Success;
            }

            if (value is DateTime dateTime && dateTime < DateTime.UtcNow)
            {
                return new ValidationResult("Дата должна быть больше или равна текущей дате");
            }

            return ValidationResult.Success;
        }
    }
}
