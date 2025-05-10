using System;
using System.Collections.Generic;

namespace Calligraphy.Models;

public partial class TbExhLog
{
    public Guid LogId { get; set; }

    public Guid UserId { get; set; }

    public string Action { get; set; } = null!;

    public string? TargetType { get; set; }

    public string? TargetId { get; set; }

    public string? Message { get; set; }

    public string? IpAddress { get; set; }

    public DateTime CreateDate { get; set; }

    public string? CreateFrom { get; set; }

    public string? Creator { get; set; }

    public DateTime? ModifyDate { get; set; }

    public string? ModifyFrom { get; set; }

    public string? Modifier { get; set; }

    public virtual TbExhUser User { get; set; } = null!;
}
