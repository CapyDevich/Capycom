using System;
using System.Collections.Generic;

namespace Capycom;

public partial class CpcmComment
{
    public Guid CpcmCommentId { get; set; }

    public Guid CpcmPostId { get; set; }

    public string? CpcmCommentText { get; set; }

    public Guid CpcmUserId { get; set; }

    public Guid? CpcmCommentFather { get; set; }

    public DateTime CpcmCommentCreationDate { get; set; }

    public bool CpcmCommentBanned { get; set; }

    public bool CpcmIsDeleted { get; set; }

    public virtual CpcmComment? CpcmCommentFatherNavigation { get; set; }

    public virtual ICollection<CpcmImage> CpcmImages { get; set; } = new List<CpcmImage>();

    public virtual CpcmPost CpcmPost { get; set; } = null!;

    public virtual ICollection<CpcmComment> InverseCpcmCommentFatherNavigation { get; set; } = new List<CpcmComment>();
}
