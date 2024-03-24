using System.ComponentModel.DataAnnotations;

namespace Capycom
{
    public class WordCountAttribute : ValidationAttribute
    {
        private readonly int _maxWords;

        public WordCountAttribute(int maxWords)
        {
            _maxWords = maxWords;
            
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var str = value as string;

            if (str != null && str.Split(' ').Length > _maxWords)
            {
                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }
    }
}
