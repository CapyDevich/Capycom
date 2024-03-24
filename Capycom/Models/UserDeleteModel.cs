using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Capycom.Models
{
    public class UserDeleteModel
    {
        [Display(Name = "Мой Nickname")]
        //[Required(ErrorMessage = "Не указан Nickname")]
        [Remote(action: "CheckNickName", controller: "User", ErrorMessage = "Nickname уже занят", HttpMethod = "Post")]
        public string? CpcmUserNickName { get; set; }

        [Required]
        [HiddenInput]
        public Guid CpcmUserId { get; set; }
    }
}