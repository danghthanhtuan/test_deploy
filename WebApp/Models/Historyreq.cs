using System;
using System.Collections.Generic;

namespace WebApp.Models;

public partial class Historyreq
{
    public int Id { get; set; }

    public string Requirementsid { get; set; } = null!;

    public string Descriptionofrequest { get; set; } = null!;

    public DateTime? Dateofupdate { get; set; }

    public string Beforstatus { get; set; } = null!;

    public string Apterstatus { get; set; } = null!;

    public string Staffid { get; set; } = null!;

    public virtual Requirement Requirements { get; set; } = null!;

    public virtual Staff Staff { get; set; } = null!;
}
