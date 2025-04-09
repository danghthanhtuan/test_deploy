using System;
using System.Collections.Generic;

namespace WebApi.Models;

public partial class SupportType
{
    public int Id { get; set; }

    public string SupportCode { get; set; } = null!;

    public string SupportName { get; set; } = null!;

    public virtual ICollection<Requirement> Requirements { get; set; } = new List<Requirement>();
}
