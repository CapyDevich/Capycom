using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Capycom;

public partial class CpcmGroupfollower
{
    public Guid CpcmGroupId { get; set; }

    public Guid CpcmUserId { get; set; }

    public int CpcmUserRole { get; set; }

    public virtual CpcmGroup CpcmGroup { get; set; } = null!;

    public virtual CpcmUser CpcmUser { get; set; } = null!;

    public virtual CpcmGroupRole CpcmUserRoleNavigation { get; set; } = null!;
	[NotMapped]
	public static readonly int FollowerRole = 2;
	[NotMapped]
	public static readonly int AuthorRole = 0;
	[NotMapped]
	public static readonly int AdminRole = 1;
}
