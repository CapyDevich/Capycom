using System;
using System.Collections.Generic;

namespace Capycom.Models;

public partial class CpcmGroupfollower
{
    public Guid CpcmGroupId { get; set; }

    public Guid CpcmUserId { get; set; }

    public int CpcmUserRole { get; set; }

    public virtual CpcmGroup CpcmGroup { get; set; } = null!;

    public virtual CpcmUser CpcmUser { get; set; } = null!;

    public virtual CpcmGroupRole CpcmUserRoleNavigation { get; set; } = null!;
}
