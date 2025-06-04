using System;
using System.Collections.Generic;

namespace WebApi.Models;

public partial class Payment
{
    public int Id { get; set; }

    public string Contractnumber { get; set; } = null!;

    public decimal Amount { get; set; }

    public bool Paymentstatus { get; set; }

    public virtual Contract ContractnumberNavigation { get; set; } = null!;

    public virtual ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new List<PaymentTransaction>();
}
