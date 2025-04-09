using System;
using System.Collections.Generic;

namespace WebApi.Models;

public partial class Loginadmin
{
    public string Staffid { get; set; } = null!;

    public string Usernamead { get; set; } = null!;

    public string Passwordad { get; set; } = null!;

    public virtual Staff Staff { get; set; } = null!;
}
