using System;
using System.Collections.Generic;

namespace WebApi.Models;

public partial class Contract
{
    public int Id { get; set; }

    public string Contractnumber { get; set; } = null!;

    public DateTime Startdate { get; set; }

    public DateTime Enddate { get; set; }

    public string ServiceTypename { get; set; } = null!;

    public string Customerid { get; set; } = null!;

    public virtual Company Customer { get; set; } = null!;

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ServiceType ServiceTypenameNavigation { get; set; } = null!;
}
