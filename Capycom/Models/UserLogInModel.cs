using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Capycom.Models
{
    public class UserLogInModel
    {
        [Display(Name = "Адрес электронной почты")]
        [Required(ErrorMessage = "Некорректный адрес")]
        [EmailAddress(ErrorMessage = "Некорректный адрес")]
        public string CpcmUserEmail { get; set; } = null!;

        [Display(Name = "Пароль")]
        [Required(ErrorMessage = "Не указан пароль")]
        [RequiredNonEmptyString(ErrorMessage = "Поле не может быть пустым или состоять только из пробелов")]
        public string CpcmUserPwd { get; set; } = null!;
    }
}
