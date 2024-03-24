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

        [Display(Name = "Моё имя")]
        [Required(ErrorMessage = "Укажите ваше имя")]
        [WordCount(1, ErrorMessage = "Имя не может состоять из более чем 1 слова")]
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
