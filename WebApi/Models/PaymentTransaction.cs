using System;
using System.Collections.Generic;

namespace WebApi.Models;

public partial class PaymentTransaction
{
    public int Id { get; set; }

    public int? PaymentId { get; set; }

    public string? TransactionCode { get; set; }

    public DateTime? PaymentDate { get; set; }

    public string? PaymentMethod { get; set; }

    public bool? PaymentResult { get; set; }

    public decimal? Amount { get; set; }

    public virtual Payment? Payment { get; set; }
}
