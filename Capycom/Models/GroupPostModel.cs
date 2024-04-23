using System.ComponentModel.DataAnnotations;

using Capycom.Attributes;

namespace Capycom.Models
{
	public class GroupPostModel
	{

		[MaxFileCount(2)]
		public IFormFileCollection? Files { get; set; }
		[MaxLength(500, ErrorMessage = "Текст поста не может состоять из более чем 500 символов")]
		public string? Text { get; set; }

		public Guid? PostFatherId { get; set; }

		[FutureDate]
		public DateTime? Published { get; set; }

		public Guid GroupId { get; set; }

	}
}
