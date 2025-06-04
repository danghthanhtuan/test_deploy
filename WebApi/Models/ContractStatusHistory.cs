using System;
using System.Collections.Generic;

namespace WebApi.Models;

public partial class ContractStatusHistory
{
    public int Id { get; set; }

    public string Contractnumber { get; set; } = null!;

    public int? OldStatus { get; set; }

    public int? NewStatus { get; set; }

    public DateTime? ChangedAt { get; set; }

    public string? ChangedBy { get; set; }

    public virtual Contract ContractnumberNavigation { get; set; } = null!;
}
