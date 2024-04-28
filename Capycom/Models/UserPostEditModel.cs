using Capycom.Attributes;

namespace Capycom.Models
{
    public class UserPostEditModel
    {
        public Guid Id { get; set; }
        public Guid? UserId { get; set; }

        public string? Text { get; set; }

        public Guid? PostFatherId { get; set; }

        public List<Guid>? FilesToDelete { get; set; } = new List<Guid>();

        [MaxFileCount(2)]
        public List<IFormFile>? NewFiles { get; set; } = new List<IFormFile>();

        public ICollection<CpcmImage>? CpcmImages { get; set; }
        [FutureDate]
        public DateTime? NewPublishDate { get; set; }

	}
}
