using System;
using System.Collections.Generic;

namespace WebApp.Models;

public partial class Review
{
    public int Id { get; set; }

    public string Requirementsid { get; set; } = null!;

    public string Customerid { get; set; } = null!;

    public string Comment { get; set; } = null!;

    public DateTime? Dateofupdate { get; set; }

    public string Staffid { get; set; } = null!;

    public virtual Company Customer { get; set; } = null!;

    public virtual Requirement Requirements { get; set; } = null!;

    public virtual ICollection<ReviewDetail> ReviewDetails { get; set; } = new List<ReviewDetail>();

    public virtual Staff Staff { get; set; } = null!;
}
