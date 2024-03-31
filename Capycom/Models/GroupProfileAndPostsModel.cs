namespace Capycom.Models
{
    public class GroupProfileAndPostsModel
    {
        public CpcmGroup Group { get; set; }
        public ICollection<CpcmPost> Posts { get; set; }
    }
}
