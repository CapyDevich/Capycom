using Capycom.Attributes;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Capycom.Models
{
    public class UserEditAboutDataModel
    {
        [Required]
        [HiddenInput]
        public Guid CpcmUserId { get; set; }

        [Display(Name = "Обо мне")]
        [MaxLength(300, ErrorMessage ="В данное поле можно указать не более 300 символов")]
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

        [Display(Name = "Моё имя")]
        [Required(ErrorMessage = "Укажите ваше имя")]
        [WordCount(1, ErrorMessage = "Имя не может состоять из более чем 1 слова")]
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
