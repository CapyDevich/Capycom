using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Capycom;

public partial class CpcmPost
{
    public Guid CpcmPostId { get; set; }

    public Guid? CpcmUserId { get; set; }

    public Guid? CpcmGroupId { get; set; }

    public string? CpcmPostText { get; set; }

    public Guid? CpcmPostFather { get; set; }

    public DateTime CpcmPostCreationDate { get; set; }

    public DateTime? CpcmPostPublishedDate { get; set; }

    public bool CpcmPostBanned { get; set; }

    public bool CpcmIsDeleted { get; set; }
	[NotMapped]
	public CpcmUser? User { get; set; }
	[NotMapped]
	public CpcmGroup? Group { get; set; }
	[NotMapped]
	public bool IsLiked { get; set; }

    public virtual ICollection<CpcmComment> CpcmComments { get; set; } = new List<CpcmComment>();

    public virtual ICollection<CpcmImage> CpcmImages { get; set; } = new List<CpcmImage>();

    public virtual CpcmPost? CpcmPostFatherNavigation { get; set; }

    public virtual ICollection<CpcmPost> InverseCpcmPostFatherNavigation { get; set; } = new List<CpcmPost>();

    public virtual ICollection<CpcmUser> CpcmUsers { get; set; } = new List<CpcmUser>();

    public virtual ICollection<CpcmUser> CpcmUsersNavigation { get; set; } = new List<CpcmUser>();
}
