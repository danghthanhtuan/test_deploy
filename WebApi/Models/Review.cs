using System;
using System.Collections.Generic;

namespace WebApi.Models;

public partial class Review
{
    public int Id { get; set; }

    public string Requirementsid { get; set; } = null!;

    public string Comment { get; set; } = null!;

    public DateTime? Dateofupdate { get; set; }

    public virtual Requirement Requirements { get; set; } = null!;

    public virtual ICollection<ReviewDetail> ReviewDetails { get; set; } = new List<ReviewDetail>();
}
