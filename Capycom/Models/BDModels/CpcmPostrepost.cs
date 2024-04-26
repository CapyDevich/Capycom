using System;
using System.Collections.Generic;

namespace Capycom;

public partial class CpcmPostrepost
{
    public Guid CpcmPostId { get; set; }

    public Guid CpcmUserId { get; set; }

    public bool CpcmIsDeleted { get; set; }

    public virtual CpcmPost CpcmPost { get; set; } = null!;

    public virtual CpcmUser CpcmUser { get; set; } = null!;
}
