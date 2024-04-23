using System.ComponentModel.DataAnnotations;

namespace Capycom.Attributes
{
    public class MaxFileCountAttribute : ValidationAttribute
    {
        private readonly int _maxFileCount;

        public MaxFileCountAttribute(int maxFileCount)
        {
            _maxFileCount = maxFileCount;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var files = value as IList<IFormFile>;
            if (files != null && files.Count > _maxFileCount)
            {
                return new ValidationResult(GetErrorMessage(), new List<string> { validationContext.DisplayName });
            }

            return ValidationResult.Success;
        }

        private string GetErrorMessage()
        {
            return $"Вы не можете загрузить более {_maxFileCount} файл(а)ов!";
        }
    }

}
