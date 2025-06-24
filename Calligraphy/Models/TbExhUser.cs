using System;
using System.Collections.Generic;

namespace Calligraphy.Models;

public partial class TbExhUser
{
    public Guid UserId { get; set; }

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string DisplayName { get; set; } = null!;

    public string Role { get; set; } = null!;

    public bool IsActive { get; set; }

    public DateTimeOffset CreateDate { get; set; }

    public string? CreateFrom { get; set; }

    public string? Creator { get; set; }

    public DateTimeOffset? ModifyDate { get; set; }

    public string? ModifyFrom { get; set; }

    public string? Modifier { get; set; }

    public bool MailConfirm { get; set; }

    public string? MailConfirmcode { get; set; }

    public DateTime? MailConfirmdate { get; set; }

    public string? RestpwdToken { get; set; }

    public DateTime? RestpwdLimitdate { get; set; }

    public bool RestpwdConfirm { get; set; }

    public virtual ICollection<TbExhArtwork> TbExhArtwork { get; set; } = new List<TbExhArtwork>();

    public virtual ICollection<TbExhLine> TbExhLine { get; set; } = new List<TbExhLine>();
}
