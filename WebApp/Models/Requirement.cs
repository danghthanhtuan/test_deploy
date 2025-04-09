using System;
using System.Collections.Generic;

namespace WebApp.Models;

public partial class Requirement
{
    public int Id { get; set; }

    public string Requirementsid { get; set; } = null!;

    public string Requirementsstatus { get; set; } = null!;

    public DateTime? Dateofrequest { get; set; }

    public string Descriptionofrequest { get; set; } = null!;

    public string Customerid { get; set; } = null!;

    public string SupportName { get; set; } = null!;

    public string? Staffid { get; set; }

    public virtual Company Customer { get; set; } = null!;

    public virtual ICollection<Historyreq> Historyreqs { get; set; } = new List<Historyreq>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual Staff? Staff { get; set; }

    public virtual SupportType SupportNameNavigation { get; set; } = null!;
}
