using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace Capycom.Models
{
    public class UserSignUpModel
    {
        public static readonly int BaseUserRole = 0;

        [Display(Name = "Адрес электронной почты")]
        [Required(ErrorMessage = "Некорректный адрес")]
        [EmailAddress(ErrorMessage = "Некорректный адрес")]
        [Remote(action:"CheckEmail",controller: "UserSignUp",ErrorMessage ="Email уже занят", HttpMethod = "Post")]
        public string CpcmUserEmail { get; set; } = null!;

        [Display(Name = "Номер телефона")]
        [Required(ErrorMessage = "Некорректный номер телефона")]
        [Phone(ErrorMessage = "Некорректный номер телефона")] // [RegularExpression(@"^\+[1-9]{1}[0-9]{3,14}$", ErrorMessage="Телефон должен быть записан в формате +7")]
        [Remote(action: "CheckPhone", controller: "UserSignUp", ErrorMessage = "Телефон уже занят", HttpMethod = "Post")]
        public string CpcmUserTelNum { get; set; } = null!;

        [Display(Name = "Пароль")]
        [Required(ErrorMessage = "Не указан пароль")]// TODO: добавить регулярку
        [RegularExpression(@"^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{8,}$",ErrorMessage = "Ваш пароль должен быть минимум из 8 символов, " +
            "содержать хотя бы 1 символ в нижнем регистре, 1 символ в верхнем регистре, 1 цифру, 1 специальный символ (#?!@$%^&*-) ")]
        public string CpcmUserPwd { get; set; } = null!;

        [Display(Name = "Подтвердите пароль")]
        [Required(ErrorMessage = "Это поле должно быть заполнено")]
        [Compare("CpcmUserPwd", ErrorMessage = "Пароли не совпадают")]
        public string CpcmUserPwdConfirm { get; set; } = null!;

        [Display(Name = "Обо мне")]
        public string? CpcmUserAbout { get; set; }

        [Display(Name = "Город")]
        public Guid? CpcmUserCity { get; set; }

        [Display(Name = "Мой сайт")]
        public string? CpcmUserSite { get; set; }

        [Display(Name = "Мои любимые книги")]
        public string? CpcmUserBooks { get; set; }

        [Display(Name = "Мои любимые фильмы")]
        public string? CpcmUserFilms { get; set; }

        [Display(Name = "Моя любимая музыка")]
        public string? CpcmUserMusics { get; set; }

        [Display(Name = "Школа")]
        public Guid? CpcmUserSchool { get; set; }

        [Display(Name = "Университет")]
        public Guid? CpcmUserUniversity { get; set; }

        //public string? CpcmUserImagePath { get; set; }

        //public string? CpcmUserCoverPath { get; set; }

        [Display(Name = "Мой Nickname")]
        //[Required(ErrorMessage = "Не указан Nickname")]
        [Remote(action: "CheckNickName", controller: "UserSignUp", ErrorMessage = "Nickname уже занят",HttpMethod ="Post")]     
        public string? CpcmUserNickName { get; set; }

        [Display(Name = "Моё имя")]
        [Required(ErrorMessage ="Укажите ваше имя")]
        [WordCount(1, ErrorMessage ="Имя не может состоять из более чем 1 слова")]
        public string CpcmUserFirstName { get; set; } = null!;

        [Display(Name = "Моя фамилия")]
        [Required(ErrorMessage = "Укажите вашу фамилию")]
        [WordCount(1, ErrorMessage = "Фамилия не может состоять из более чем 1 слова")]
        public string CpcmUserSecondName { get; set; } = null!;

        [Display(Name = "Моё отчество")]
        [WordCount(1, ErrorMessage = "Отчество не может состоять из более чем 1 слова")]
        public string? CpcmUserAdditionalName { get; set; }

        public IFormFile? CpcmUserImage { get; set; }

        public IFormFile? CpcmUserCoverImage { get; set; }

    }
}
