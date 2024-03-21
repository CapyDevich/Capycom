﻿using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Capycom.Models
{
    public class UserEditIdentityModel
    {
        [Required]
        [HiddenInput]
        public Guid CpcmUserId { get; set; }

        [Display(Name = "Адрес электронной почты")]
        
        [EmailAddress(ErrorMessage = "Некорректный адрес")]
        [Remote(action: "CheckEmail", controller: "User", ErrorMessage = "Email уже занят", HttpMethod = "Post")]
        public string CpcmUserEmail { get; set; } = null!;

        [Display(Name = "Номер телефона")]
        
        [Phone(ErrorMessage = "Некорректный номер телефона")]
        [Remote(action: "CheckPhone", controller: "User", ErrorMessage = "Телефон уже занят", HttpMethod = "Post")]
        public string CpcmUserTelNum { get; set; } = null!;

        [Display(Name = "Пароль")]
        
        [RegularExpression(@"^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{8,}$", ErrorMessage = "Ваш пароль должен быть минимум из 8 символов, " +
            "содержать хотя бы 1 символ в нижнем регистре, 1 символ в верхнем регистре, 1 цифру, 1 специальный символ (#?!@$%^&*-) ")]
        public string CpcmUserPwd { get; set; } = null!;

        [Display(Name = "Подтвердите пароль")]
        
        [Compare("CpcmUserPwd", ErrorMessage = "Пароли не совпадают")]
        public string CpcmUserPwdConfirm { get; set; } = null!;

        [Display(Name = "Мой Nickname")]
        [Required(ErrorMessage = "Не указан Nickname")]
        [Remote(action: "CheckNickName", controller: "User", ErrorMessage = "Nickname уже занят", HttpMethod = "Post")]
        public string? CpcmUserNickName { get; set; }
    }
}
