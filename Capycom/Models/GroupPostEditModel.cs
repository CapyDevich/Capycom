using System.ComponentModel.DataAnnotations;

using Capycom.Attributes;

namespace Capycom.Models
{
	public class GroupPostEditModel
	{
		public Guid Id { get; set; }
		public Guid? GroupId { get; set; }
		[MaxLength(500,ErrorMessage ="Текст поста не может состоять из более чем 500 символов")]
		public string? Text { get; set; }

		public Guid? PostFatherId { get; set; }

		public List<Guid>? FilesToDelete { get; set; } = new List<Guid>();

		[MaxFileCount(4)]
		public List<IFormFile>? NewFiles { get; set; } = new List<IFormFile>();

		public ICollection<CpcmImage>? CpcmImages { get; set; }
		[FutureDate]
		public DateTime? NewPublishDate { get; set; }
	}
}
