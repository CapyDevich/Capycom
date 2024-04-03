using System.ComponentModel.DataAnnotations;

namespace Capycom.Models
{
	public class GroupFilterModel
	{
		public Guid? UserId { get; set; }
		[MaxLength(30, ErrorMessage = "Nickname не может состоять из более чем 30 символов")]
		[RegularExpression(@"^[a-zA-Z0-9_\-]*$", ErrorMessage = "Nickname может содержать только буквы латиницы, цифры, подчеркивания и дефисы.")]
		public string? NickName { get; set; }
		public Guid? GroupId { get; set; }
		public string? Name { get; set; }
		public int? SubjectID { get; set; }
		public Guid? CityId { get; set; }
				
		public Guid? lastId { get; set; }

	}
}
