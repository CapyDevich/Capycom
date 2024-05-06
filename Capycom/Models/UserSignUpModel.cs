using Capycom.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace Capycom.Models
{
    public class UserSignUpModel
    {
        public static readonly int BaseUserRole = 1;

        [Display(Name = "Адрес электронной почты")]
        [Required(ErrorMessage = "Некорректный адрес")]
        [EmailAddress(ErrorMessage = "Некорректный адрес")]
        [Remote(action:"CheckEmail",controller: "UserSignUp", HttpMethod = "Post")]
        public string CpcmUserEmail { get; set; } = null!;

        [Display(Name = "Номер телефона")]
        [Required(ErrorMessage = "Некорректный номер телефона")]
		//[Phone(ErrorMessage = "Некорректный номер телефона")] 
		[RegularExpression(@"^\+[1-9]\d{10,14}$", ErrorMessage = "Телефон должен быть записан в формате +ХХХХХХХХХХХХХХХ, при этом первой цифрой не может быть 0, от 11 до 14 цифр.")] //Стандарт E.164 ^\+[1-9]{1}[0-9]{3,14}$
		[Remote(action: "CheckPhone", controller: "UserSignUp", HttpMethod = "Post")]
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
		[MaxLength(300, ErrorMessage = "В данное поле можно указать не более 300 символов")]
		public string? CpcmUserAbout { get; set; }

        [Display(Name = "Город")]
        public Guid? CpcmUserCity { get; set; }

        [Display(Name = "Мой сайт")]
		[MaxLength(100, ErrorMessage = "В данное поле можно указать не более 100 символов")]
		public string? CpcmUserSite { get; set; }

        [Display(Name = "Мои любимые книги")]
		[MaxLength(100, ErrorMessage = "В данное поле можно указать не более 100 символов")]
		public string? CpcmUserBooks { get; set; }

        [Display(Name = "Мои любимые фильмы")]
		[MaxLength(100, ErrorMessage = "В данное поле можно указать не более 100 символов")]
		public string? CpcmUserFilms { get; set; }

        [Display(Name = "Моя любимая музыка")]
		[MaxLength(100, ErrorMessage = "В данное поле можно указать не более 100 символов")]
		public string? CpcmUserMusics { get; set; }

        [Display(Name = "Школа")]
        public Guid? CpcmUserSchool { get; set; }

        [Display(Name = "Университет")]
        public Guid? CpcmUserUniversity { get; set; }

        //public string? CpcmUserImagePath { get; set; }

        //public string? CpcmUserCoverPath { get; set; }

        [Display(Name = "Мой Nickname")]
        //[Required(ErrorMessage = "Не указан Nickname")]
        [Remote(action: "CheckNickName", controller: "UserSignUp",HttpMethod ="Post")]
		[RegularExpression(@"^[a-zA-Z0-9_\-]*$", ErrorMessage = "Nickname может содержать только буквы латиницы, цифры, подчеркивания и дефисы.")]
		[MaxLength(30, ErrorMessage = "Nickname не может состоять из более чем 30 символов")]
		public string? CpcmUserNickName { get; set; }

        [Display(Name = "Моё имя")]
        [Required(ErrorMessage ="Укажите ваше имя")]
        [WordCount(1, ErrorMessage ="Имя не может состоять из более чем 1 слова")]
		[MaxLength(30, ErrorMessage = "Имя не может быть состоять из более чем 30 символов")]
		[RegularExpression(@"^[a-zA-Z0-9_\-а-яА-ЯёЁ]*$", ErrorMessage = "Имя может содержать только буквы (латиницу и кириллицу), цифры, подчеркивания и дефисы.")]
		public string CpcmUserFirstName { get; set; } = null!;

        [Display(Name = "Моя фамилия")]
        [Required(ErrorMessage = "Укажите вашу фамилию")]
        [WordCount(1, ErrorMessage = "Фамилия не может состоять из более чем 1 слова")]
		[MaxLength(30, ErrorMessage = "Фамилия не может быть состоять из более чем 30 символов")]
		[RegularExpression(@"^[a-zA-Z0-9_\-а-яА-ЯёЁ]*$", ErrorMessage = "Фамилия может содержать только буквы (латиницу и кириллицу), цифры, подчеркивания и дефисы.")]
		public string CpcmUserSecondName { get; set; } = null!;

        [Display(Name = "Моё отчество")]
        [WordCount(1, ErrorMessage = "Отчество не может состоять из более чем 1 слова")]
		[MaxLength(30, ErrorMessage = "Отчество не может быть состоять из более чем 30 символов")]
		[RegularExpression(@"^[a-zA-Z0-9_\-а-яА-ЯёЁ]*$", ErrorMessage = "Отчество может содержать только буквы (латиницу и кириллицу), цифры, подчеркивания и дефисы.")]
		public string? CpcmUserAdditionalName { get; set; }

        public IFormFile? CpcmUserImage { get; set; }

        public IFormFile? CpcmUserCoverImage { get; set; }

    }
}
