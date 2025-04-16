using System;
using System.Collections.Generic;

namespace WebApp.Models;

public partial class SupportType
{
    public int Id { get; set; }

    public string SupportCode { get; set; } = null!;

    public string SupportName { get; set; } = null!;

    public virtual Requirement? Requirement { get; set; }
}
