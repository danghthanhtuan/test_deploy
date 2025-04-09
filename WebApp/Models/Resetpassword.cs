using System;
using System.Collections.Generic;

namespace WebApp.Models;

public partial class Resetpassword
{
    public int Id { get; set; }

    public string Customerid { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string Passwordclient { get; set; } = null!;

    public virtual Company Customer { get; set; } = null!;
}
