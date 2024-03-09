using System;
using System.Collections.Generic;

namespace Capycom.Models;

public partial class CpcmSchool
{
    public Guid CpcmSchooldId { get; set; }

    public string CpcmSchoolName { get; set; } = null!;

    public virtual ICollection<CpcmUser> CpcmUsers { get; set; } = new List<CpcmUser>();
}
