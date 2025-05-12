using System;
using System.Collections.Generic;

namespace Calligraphy.Models;

public partial class TbExhComment
{
    public Guid CommentId { get; set; }

    public Guid ArtworkId { get; set; }

    public string UserName { get; set; } = null!;

    public string Message { get; set; } = null!;

    public bool IsApproved { get; set; }

    public DateTimeOffset CreateDate { get; set; }

    public string? CreateFrom { get; set; }

    public string? Creator { get; set; }

    public DateTimeOffset? ModifyDate { get; set; }

    public string? ModifyFrom { get; set; }

    public string? Modifier { get; set; }

    public virtual TbExhArtwork Artwork { get; set; } = null!;
}
