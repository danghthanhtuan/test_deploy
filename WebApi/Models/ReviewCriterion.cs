using System;
using System.Collections.Generic;

namespace WebApi.Models;

public partial class ReviewCriterion
{
    public int Id { get; set; }

    public string CriteriaName { get; set; } = null!;

    public virtual ICollection<ReviewDetail> ReviewDetails { get; set; } = new List<ReviewDetail>();
}
