﻿using System.ComponentModel.DataAnnotations;

namespace Capycom.Attributes
{
    public class FutureDateAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }

            if (value is DateTime dateTime && dateTime < DateTime.Now)
            {
                return new ValidationResult("Дата должна быть больше или равна текущей дате");
            }

            return ValidationResult.Success;
        }
    }
}
