namespace Capycom.Models
{
    public class PostModel
    {
        public CpcmPost Post {  get; set; }
		[Obsolete("Более не используется, обращаться напрямую к посту")]
		public CpcmUser? UserOwner { get; set; }
		[Obsolete("Более не используется, обращаться напрямую к посту")]
		public CpcmGroup? GroupOwner { get; set; }

		public long LikesCount { get; set; }

        public long RepostsCount { get; set; }

        public List<CpcmComment> TopLevelComments { get; set; }
    }
}
