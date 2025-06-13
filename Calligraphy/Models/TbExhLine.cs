using System;
using System.Collections.Generic;

namespace Calligraphy.Models;

public partial class TbExhLine
{
    public Guid LineId { get; set; }

    public string? LineUserId { get; set; }

    public Guid UserId { get; set; }

    public DateTimeOffset? CreateDate { get; set; }

    public string? CreateFrom { get; set; }

    public string? Creator { get; set; }

    public DateTimeOffset? ModifyDate { get; set; }

    public string? ModifyFrom { get; set; }

    public string? Modifier { get; set; }

    public virtual TbExhUser User { get; set; } = null!;
}
