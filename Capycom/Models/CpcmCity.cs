using System;
using System.Collections.Generic;

namespace Capycom.Models;

public partial class CpcmCity
{
    public Guid CpcmCityId { get; set; }

    public string CpcmCityName { get; set; } = null!;

    public virtual ICollection<CpcmUser> CpcmUsers { get; set; } = new List<CpcmUser>();
}
