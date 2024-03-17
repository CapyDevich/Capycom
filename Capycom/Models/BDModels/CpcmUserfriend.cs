using System;
using System.Collections.Generic;

namespace Capycom;

public partial class CpcmUserfriend
{
    public Guid CmcpUserId { get; set; }

    public Guid CmcpFriendId { get; set; }

    public bool CpcmFriendRequestStatus { get; set; }

    public virtual CpcmUser CmcpFriend { get; set; } = null!;

    public virtual CpcmUser CmcpUser { get; set; } = null!;
}
