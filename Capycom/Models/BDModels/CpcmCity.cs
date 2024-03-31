using System;
using System.Collections.Generic;

namespace Capycom;

public partial class CpcmCity
{
    public Guid CpcmCityId { get; set; }

    public string CpcmCityName { get; set; } = null!;

    public virtual ICollection<CpcmGroup> CpcmGroups { get; set; } = new List<CpcmGroup>();

    public virtual ICollection<CpcmUser> CpcmUsers { get; set; } = new List<CpcmUser>();
}
