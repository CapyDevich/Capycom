using System;
using System.Collections.Generic;

namespace Capycom.Models;

public partial class CpcmRole
{
    public int CpcmRoleId { get; set; }

    public string CpcmRoleName { get; set; } = null!;

    public bool CpcmCanEditUsers { get; set; }

    public bool CpcmCanEditGroups { get; set; }

    public bool CpcmCanEditRoles { get; set; }

    public bool CpcmCanDelUsersPosts { get; set; }

    public bool CpcmCanDelUsersComments { get; set; }

    public bool CpcmCanDelGroupsPosts { get; set; }

    public bool CpcmCanAddPost { get; set; }

    public bool CpcmCanAddGroups { get; set; }

    public bool CpcmCanAddComments { get; set; }

    public virtual ICollection<CpcmUser> CpcmUsers { get; set; } = new List<CpcmUser>();
}
