using System;
using System.Collections.Generic;

namespace WebApi.Models;

public partial class Contract
{
    public int Id { get; set; }

    public string Contractnumber { get; set; } = null!;

    public DateTime Startdate { get; set; }

    public DateTime Enddate { get; set; }

    public int? ServiceTypeid { get; set; }

    public string Customerid { get; set; } = null!;

    public string? Original { get; set; }

    public bool Customertype { get; set; }

    public string? Constatus { get; set; }

    public virtual ICollection<ContractFile> ContractFiles { get; set; } = new List<ContractFile>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<Requirement> Requirements { get; set; } = new List<Requirement>();

    public virtual ServiceType? ServiceType { get; set; }
}
