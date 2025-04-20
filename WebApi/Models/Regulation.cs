using System;
using System.Collections.Generic;

namespace WebApi.Models;

public partial class Regulation
{
    public int Id { get; set; }

    public string ServiceGroupid { get; set; } = null!;

    public decimal Price { get; set; }

    public virtual ServiceGroup ServiceGroup { get; set; } = null!;
}
