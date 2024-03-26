namespace Capycom.Models
{
    public class PostModel
    {
        public CpcmPost Post {  get; set; }

        public CpcmUser? UserOwner { get; set; }

        public CpcmGroup? GroupOwner { get; set; }

        public long LikesCount { get; set; }

        public long RepostsCount { get; set; }
    }
}
