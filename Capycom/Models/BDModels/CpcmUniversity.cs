using System;
using System.Collections.Generic;

namespace Capycom;

public partial class CpcmUniversity
{
    public Guid CpcmUniversityId { get; set; }

    public string CpcmUniversityName { get; set; } = null!;

    public virtual ICollection<CpcmUser> CpcmUsers { get; set; } = new List<CpcmUser>();
}
