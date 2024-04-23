using Capycom.Attributes;

namespace Capycom.Models
{
	public class UserPostModel
	{
		[MaxFileCount(2)]
		public IFormFileCollection? Files { get; set; }

		public string? Text { get; set; }

		public Guid? PostFatherId { get; set; }

		[FutureDate]
		public DateTime? Published { get; set; }
	}


}

