﻿using System.ComponentModel.DataAnnotations;

namespace Capycom.Attributes
{
    public class RequiredNonEmptyStringAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                //ErrorMessage = "Поле не может быть пустым или состоять только из пробелов.";
                return new ValidationResult(ErrorMessage, new List<string> { validationContext.DisplayName });
            }

            return ValidationResult.Success;
        }
    }
}
