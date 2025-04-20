using System;
using System.Collections.Generic;

namespace WebApi.Models;

public partial class ServiceGroup
{
    public int Id { get; set; }

    public string ServiceGroupid { get; set; } = null!;

    public string GroupName { get; set; } = null!;

    public virtual ICollection<Endow> Endows { get; set; } = new List<Endow>();

    public virtual ICollection<Regulation> Regulations { get; set; } = new List<Regulation>();

    public virtual ICollection<ServiceType> ServiceTypes { get; set; } = new List<ServiceType>();
}
