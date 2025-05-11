using System;
using System.Collections.Generic;

namespace WebApi.Models;

public partial class Passwordresettoken
{
    public int Id { get; set; }

    public string Email { get; set; } = null!;

    public string Otp { get; set; } = null!;

    public DateTime Expirytime { get; set; }

    public bool Isused { get; set; }
}
