namespace Capycom.Models
{
    public class UserPostModel
    {
        public IFormFileCollection? Files { get; set; }

        public string? Text { get; set; }

        public Guid? PostFatherId { get; set; }

        public DateTime? Published { get; set; }
    }


}
