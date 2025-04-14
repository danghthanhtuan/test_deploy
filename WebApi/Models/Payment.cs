using System;
using System.Collections.Generic;

namespace WebApi.Models;

public partial class Payment
{
    public int Id { get; set; }

    public string Contractnumber { get; set; } = null!;

    public string Customerid { get; set; } = null!;

    public decimal Amount { get; set; }

    public DateTime? PaymentDate { get; set; }

    public string? PaymentMethod { get; set; }

    public bool Paymentstatus { get; set; }

    public string? TransactionCode { get; set; }

    public virtual Contract ContractnumberNavigation { get; set; } = null!;

    public virtual Company Customer { get; set; } = null!;
}
