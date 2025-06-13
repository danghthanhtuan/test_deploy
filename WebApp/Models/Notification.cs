using System;
using System.Collections.Generic;

namespace WebApp.Models;

public partial class Notification
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    public int Typenoti { get; set; }

    public long? ReferenceId { get; set; }

    public bool IsRead { get; set; }

    public string? Data { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
