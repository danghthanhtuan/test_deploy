using System;
using System.Collections.Generic;

namespace WebApp.Models;

public partial class Endow
{
    public int Id { get; set; }

    public string Endowid { get; set; } = null!;

    public string ServiceGroupid { get; set; } = null!;

    public double Discount { get; set; }

    public DateTime? Startdate { get; set; }

    public DateTime? Enddate { get; set; }

    public int? Duration { get; set; }

    public string? Descriptionendow { get; set; }

    public virtual ServiceGroup ServiceGroup { get; set; } = null!;
}
