using System;
using System.Collections.Generic;

namespace WebApp.Models;

public partial class Contact
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string? Email { get; set; }

    public string? Subject { get; set; }

    public string? Message { get; set; }

    public int? Status { get; set; }

    public DateTime? CreatedDate { get; set; }
}
