using System;
using System.Collections.Generic;

namespace Capycom;

public partial class CpcmGroupsubject
{
    public int CpcmSubjectId { get; set; }

    public string CpcmSubjectName { get; set; } = null!;

    public virtual ICollection<CpcmGroup> CpcmGroups { get; set; } = new List<CpcmGroup>();
}
