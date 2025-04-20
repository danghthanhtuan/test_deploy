using System;
using System.Collections.Generic;

namespace WebApi.Models;

public partial class Endow
{
    public int Id { get; set; }

    public int Duration { get; set; }

    public double Discount { get; set; }

    public string? ServiceGroupid { get; set; }

    public DateTime? Startdate { get; set; }

    public DateTime? Enddate { get; set; }

    public virtual ServiceGroup? ServiceGroup { get; set; }
}
