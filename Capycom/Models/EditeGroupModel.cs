using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Capycom.Models
{
	public class EditeGroupModel
	{
		[Display(Name = "Название группы")]
		[Required(ErrorMessage = "Укажите название группы")]
		public string CpcmGroupName { get; set; } = null!;
		[Display(Name = "О группе")]
		[MaxLength(300, ErrorMessage = "Описание группы не может состоять из более чем 300 символов")]
		public string? CpcmGroupAbout { get; set; }

		//public string? CpcmGroupImage { get; set; }

		//public string? CpcmGroupCovet { get; set; }
		[Display(Name = "Тема группы")]
		[Required(ErrorMessage = "Укажите тематику группы")]
		public int CpcmGroupSubject { get; set; }
		[Display(Name = "Город группы")]
		public Guid? CpcmGroupCity { get; set; }
		[Display(Name = "Телефон группы")]
		[RegularExpression(@"^\+[1-9]\d{10,14}$", ErrorMessage = "Телефон должен быть записан в формате +ХХХХХХХХХХХХХХХ, при этом первой цифрой не может быть 0, от 11 до 14 цифр.")]
		public string? CpcmGroupTelNum { get; set; }

		[Display(Name = "NickName группы")]
		[Remote(action: "CheckCreateNickName", controller: "Group", HttpMethod = "Post", AdditionalFields = nameof(GroupId))]
		[RegularExpression(@"^[a-zA-Z0-9_\-]*$", ErrorMessage = "Nickname может содержать только буквы латиницы, цифры, подчеркивания и дефисы.")]
		[MaxLength(30, ErrorMessage = "Nickname не может состоять из более чем 30 символов")]
		public string? CpcmGroupNickName { get; set; }
		[Display(Name = "Аватарка группы")]
		public IFormFile? CpcmGroupImage { get; set; }
		[Display(Name = "Фон группы")]
		public IFormFile? CpcmGroupCovet { get; set; }
		[HiddenInput]
		public Guid? GroupId { get; set; }
	}
}
