using System;
using System.Collections.Generic;

namespace WebApp.Models;

public partial class ReviewDetail
{
    public int Id { get; set; }

    public int ReviewId { get; set; }

    public int CriteriaId { get; set; }

    public int Star { get; set; }

    public virtual ReviewCriterion Criteria { get; set; } = null!;

    public virtual Review Review { get; set; } = null!;
}
