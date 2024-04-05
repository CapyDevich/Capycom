using System.ComponentModel.DataAnnotations;

namespace Capycom.Models
{
	public class UserFilterModel
	{
		//Guid id, Guid? cityId, Guid? schoolId, Guid? universityId, string? firstName, string? secondName, string? additionalName

		public Guid? UserId { get; set; }
		public Guid? GroupId { get; set; }
		[RegularExpression(@"^[a-zA-Z0-9_\-]*$", ErrorMessage = "Nickname может содержать только буквы латиницы, цифры, подчеркивания и дефисы.")]
		public string? NickName { get; set; }


		public Guid? CityId { get; set; }
		public Guid? SchoolId { get; set; }
		public Guid? UniversityId { get; set; }
		[RegularExpression(@"^[a-zA-Z0-9_\-а-яА-ЯёЁ]*$", ErrorMessage = "Имя может содержать только буквы (латиницу и кириллицу), цифры, подчеркивания и дефисы.")]
		[MaxLength(30, ErrorMessage = "Имя не может состоять из более чем 30 символов")]
		public string? FirstName { get; set; }
		[RegularExpression(@"^[a-zA-Z0-9_\-а-яА-ЯёЁ]*$", ErrorMessage = "Имя может содержать только буквы (латиницу и кириллицу), цифры, подчеркивания и дефисы.")]
		[MaxLength(30, ErrorMessage = "Фамилия не может состоять из более чем 30 символов")]
		public string? SecondName { get; set; }
		[RegularExpression(@"^[a-zA-Z0-9_\-а-яА-ЯёЁ]*$", ErrorMessage = "Имя может содержать только буквы (латиницу и кириллицу), цифры, подчеркивания и дефисы.")]
		[MaxLength(30, ErrorMessage = "Отчество не может состоять из более чем 30 символов")]
		public string? AdditionalName { get; set; }
		public Guid? lastId { get; set; }

	}
}
