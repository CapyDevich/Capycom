namespace Capycom.Models
{
    public class UserPostEditModel
    {
        public Guid? Id { get; set; }
        public Guid? UserId { get; set; }

        public string? Text { get; set; }

        public Guid? PostFatherId { get; set; }

        public List<Guid>? FilesToDelete { get; set; }

        public List<IFormFile>? NewFiles { get; set; }

        public ICollection<CpcmImage>? CpcmImages { get; set; }
    }
}
