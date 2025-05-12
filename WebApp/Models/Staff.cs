using System;
using System.Collections.Generic;

namespace WebApp.Models;

public partial class Staff
{
    public int Id { get; set; }

    public string Staffid { get; set; } = null!;

    public string Staffemail { get; set; } = null!;

    public string Staffname { get; set; } = null!;

    public DateTime? Staffdate { get; set; }

    public bool? Staffgender { get; set; }

    public string? Staffaddress { get; set; }

    public string Staffphone { get; set; } = null!;

    public string Department { get; set; } = null!;

    public virtual ICollection<Assign> Assigns { get; set; } = new List<Assign>();

    public virtual ICollection<Historyreq> Historyreqs { get; set; } = new List<Historyreq>();

    public virtual Loginadmin? Loginadmin { get; set; }
}
