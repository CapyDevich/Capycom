using System;
using System.Collections.Generic;

namespace Capycom;

public partial class CpcmGroupRole
{
    public int CpcmRoleId { get; set; }

    public string CpcmRoleName { get; set; } = null!;

    public bool CpcmCanEditGroup { get; set; }

    public bool CpcmCanMakePost { get; set; }

    public bool CpcmCanDelPost { get; set; }

    public bool CpcmCanEditPost { get; set; }

    public virtual ICollection<CpcmGroupfollower> CpcmGroupfollowers { get; set; } = new List<CpcmGroupfollower>();
}
