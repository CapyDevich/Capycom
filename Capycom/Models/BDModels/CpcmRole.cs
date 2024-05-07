using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;

namespace Capycom;

public partial class CpcmRole
{
	[ValidateNever]
	public int CpcmRoleId { get; set; }

    public string CpcmRoleName { get; set; } = null!;

    public bool CpcmCanEditUsers { get; set; }

    public bool CpcmCanEditGroups { get; set; }

    public bool CpcmCanEditRoles { get; set; }

    public bool CpcmCanEditGroupRoles { get; set; }

    public bool CpcmCanEditUsersPost { get; set; }

    public bool CpcmCanEditGroupsPost { get; set; }

    public bool CpcmCanBanUsers { get; set; }

    public bool CpcmCanBanUsersPost { get; set; }

    public bool CpcmCanBanUsersComment { get; set; }

    public bool CpcmCanBanGroupsPost { get; set; }
    public bool CpcmCanBanGroups { get; set; }

	public virtual ICollection<CpcmUser> CpcmUsers { get; set; } = new List<CpcmUser>();
}
