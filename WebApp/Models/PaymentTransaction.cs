using System;
using System.Collections.Generic;

namespace WebApp.Models;

public partial class PaymentTransaction
{
    public int Id { get; set; }

    public string? TransactionCode { get; set; }

    public decimal? Amount { get; set; }

    public string? BankCode { get; set; }

    public string? BankTransactionCode { get; set; }

    public string? CardType { get; set; }

    public string? OrderInfo { get; set; }

    public DateTime? PaymentDate { get; set; }

    public string? ResponseCode { get; set; }

    public string? TmnCode { get; set; }

    public string? PaymentMethod { get; set; }

    public int? PaymentResult { get; set; }

    public string? Email { get; set; }

    public int? PaymentId { get; set; }

    public virtual Payment? Payment { get; set; }
}
