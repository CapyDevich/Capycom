using System;
using System.Collections.Generic;

namespace Capycom;

public partial class CpcmGroup
{
    public Guid CpcmGroupId { get; set; }

    public string CpcmGroupName { get; set; } = null!;

    public string? CpcmGroupAbout { get; set; }

    public string? CpcmGroupImage { get; set; }

    public string? CpcmGroupCovet { get; set; }

    public int CpcmGroupSubject { get; set; }

    public Guid? CpcmGroupCity { get; set; }

    public string? CpcmGroupTelNum { get; set; }

    public string? CpcmGroupNickName { get; set; }

    public bool CpcmGroupBanned { get; set; }

    public bool CpcmIsDeleted { get; set; }

    public virtual CpcmGroupsubject CpcmGroupSubjectNavigation { get; set; } = null!;

    public virtual ICollection<CpcmGroupfollower> CpcmGroupfollowers { get; set; } = new List<CpcmGroupfollower>();
}
