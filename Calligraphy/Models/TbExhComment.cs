using System;
using System.Collections.Generic;

namespace Calligraphy.Models;

public partial class TbExhComment
{
    public Guid CommentId { get; set; }

    public Guid ArtworkId { get; set; }

    public string? UserName { get; set; }

    public string? Message { get; set; }

    public DateTimeOffset CreateDate { get; set; }

    public string? CreateFrom { get; set; }

    public string? Creator { get; set; }

    public DateTimeOffset? ModifyDate { get; set; }

    public string? ModifyFrom { get; set; }

    public string? Modifier { get; set; }

    public string? Reply { get; set; }

    public DateTimeOffset? ReplyTime { get; set; }

    public virtual TbExhArtwork Artwork { get; set; } = null!;
}
