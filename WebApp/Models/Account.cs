using System;
using System.Collections.Generic;

namespace WebApp.Models;

public partial class Account
{
    public string Customerid { get; set; } = null!;

    public string Rootaccount { get; set; } = null!;

    public string Rootname { get; set; } = null!;

    public string Rphonenumber { get; set; } = null!;

    public DateTime Dateofbirth { get; set; }

    public bool Gender { get; set; }

    public bool? IsActive { get; set; }

    public virtual Company Customer { get; set; } = null!;
}
