using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Capycom.Models
{
    public class CreateEditGroupModel
    {
		[Display(Name = "Название группы")]
		[Required(ErrorMessage = "Укажите название группы")]
		public string CpcmGroupName { get; set; } = null!;
		[Display(Name = "О группе")]
		public string? CpcmGroupAbout { get; set; }

		//public string? CpcmGroupImage { get; set; }

		//public string? CpcmGroupCovet { get; set; }
		[Display(Name = "Тема группы")]
		[Required(ErrorMessage = "Укажите тематику группы")]
		public int CpcmGroupSubject { get; set; }
		[Display(Name = "Город группы")]
		public Guid? CpcmGroupCity { get; set; }
		[Display(Name = "Телефон группы")]
		public string? CpcmGroupTelNum { get; set; }

		[Display(Name = "NickName группы")]
		[Remote(action: "CheckCreateNickName", controller: "Group", ErrorMessage = "Nickname уже занят", HttpMethod = "Post")]
		public string? CpcmGroupNickName { get; set; }
		[Display(Name = "Аватарка группы")]
		public IFormFile? CpcmGroupImage { get; set; }
		[Display(Name = "Фон группы")]
		public IFormFile? CpcmGroupCovet { get; set; }
		[HiddenInput]
		public Guid? GroupId { get; set; }
	}
}