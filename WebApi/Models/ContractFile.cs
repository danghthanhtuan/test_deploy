using System;
using System.Collections.Generic;

namespace WebApi.Models;

public partial class ContractFile
{
    public int Id { get; set; }

    public int Contractnumber { get; set; }

    public string ConfileName { get; set; } = null!;

    public string FilePath { get; set; } = null!;

    public DateTime? UploadedAt { get; set; }

    public bool? IsSigned { get; set; }

    public virtual Contract ContractnumberNavigation { get; set; } = null!;
}
