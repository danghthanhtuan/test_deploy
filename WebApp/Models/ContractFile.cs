using System;
using System.Collections.Generic;

namespace WebApp.Models;

public partial class ContractFile
{
    public int Id { get; set; }

    public string Contractnumber { get; set; } = null!;

    public string ConfileName { get; set; } = null!;

    public string FilePath { get; set; } = null!;

    public DateTime? UploadedAt { get; set; }

    public int? FileStatus { get; set; }

    public virtual Contract ContractnumberNavigation { get; set; } = null!;
}
