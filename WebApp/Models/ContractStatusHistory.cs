using System;
using System.Collections.Generic;

namespace WebApp.Models;

public partial class ContractStatusHistory
{
    public int Id { get; set; }

    public string Contractnumber { get; set; } = null!;

    public string? OldStatus { get; set; }

    public string? NewStatus { get; set; }

    public DateTime? ChangedAt { get; set; }

    public string? ChangedBy { get; set; }

    public virtual Contract ContractnumberNavigation { get; set; } = null!;
}
