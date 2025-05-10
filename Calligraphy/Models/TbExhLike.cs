using System;
using System.Collections.Generic;

namespace Calligraphy.Models;

public partial class TbExhLike
{
    public Guid LikeId { get; set; }

    public Guid ArtworkId { get; set; }

    public string IpAddress { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string? CreateFrom { get; set; }

    public string? Creator { get; set; }

    public DateTime? ModifyDate { get; set; }

    public string? ModifyFrom { get; set; }

    public string? Modifier { get; set; }

    public virtual TbExhArtwork Artwork { get; set; } = null!;
}
