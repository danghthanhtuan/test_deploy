using System;
using System.Collections.Generic;

namespace WebApi.Models;

public partial class ServiceGroup
{
    public int Id { get; set; }

    public string ServiceGroupid { get; set; } = null!;

    public string GroupName { get; set; } = null!;

    public virtual ICollection<ServiceType> ServiceTypes { get; set; } = new List<ServiceType>();
}
