using System;
using System.Collections.Generic;

namespace WebApi.Models;

public partial class Historyreq
{
    public int Id { get; set; }

    public string Requirementsid { get; set; } = null!;

    public string Descriptionofrequest { get; set; } = null!;

    public DateTime? Dateofupdate { get; set; }

    public string Staffid { get; set; } = null!;

    public int? Beforstatus { get; set; }

    public int? Apterstatus { get; set; }

    public virtual Requirement Requirements { get; set; } = null!;

    public virtual Staff Staff { get; set; } = null!;
}
