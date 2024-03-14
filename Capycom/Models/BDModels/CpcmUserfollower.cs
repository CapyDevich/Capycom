using System;
using System.Collections.Generic;

namespace Capycom;

public partial class CpcmUserfollower
{
    public Guid CpcmFollowersId { get; set; }

    public Guid CpcmUserId { get; set; }

    public Guid CpcmFollowerId { get; set; }

    public virtual CpcmUser CpcmFollower { get; set; } = null!;

    public virtual CpcmUser CpcmUser { get; set; } = null!;
}
