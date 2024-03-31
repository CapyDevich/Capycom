namespace Capycom.Models
{
	public class GroupPostModel
	{

		[MaxFileCount(4)]
		public IFormFileCollection? Files { get; set; }

		public string? Text { get; set; }

		public Guid? PostFatherId { get; set; }

		[FutureDate]
		public DateTime? Published { get; set; }

		public Guid GroupId { get; set; }

	}
}
