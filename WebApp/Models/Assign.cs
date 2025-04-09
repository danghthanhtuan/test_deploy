using System;
using System.Collections.Generic;

namespace WebApp.Models;

public partial class Assign
{
    public int Id { get; set; }

    public string Requirementsid { get; set; } = null!;

    public string Department { get; set; } = null!;

    public string? Staffid { get; set; }

    public virtual Requirement Requirements { get; set; } = null!;

    public virtual Staff? Staff { get; set; }
}
