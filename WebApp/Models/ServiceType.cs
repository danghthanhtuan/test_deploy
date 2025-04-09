using System;
using System.Collections.Generic;

namespace WebApp.Models;

public partial class ServiceType
{
    public int Id { get; set; }

    public string ServiceGroupid { get; set; } = null!;

    public string ServiceTypename { get; set; } = null!;

    public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();

    public virtual ServiceGroup ServiceGroup { get; set; } = null!;
}
