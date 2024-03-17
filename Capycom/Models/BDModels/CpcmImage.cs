using System;
using System.Collections.Generic;

namespace Capycom;

public partial class CpcmImage
{
    public Guid CpcmImageId { get; set; }

    public Guid? CpcmCommentId { get; set; }

    public Guid? CpcmPostId { get; set; }

    public string CpcmImagePath { get; set; } = null!;

    public int CpcmImageOrder { get; set; }

    public virtual CpcmComment? CpcmComment { get; set; }

    public virtual CpcmPost? CpcmPost { get; set; }
}
