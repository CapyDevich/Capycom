namespace Capycom.Models
{
    public class UserProfileAndPostsModel
    {
        public CpcmUser User { get; set; }
        public ICollection<PostModel> Posts { get; set; }

    }
}
