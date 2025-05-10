using System;
using System.Collections.Generic;

namespace Calligraphy.Models;

public partial class TbExhExhibition
{
    public Guid ExhibitionId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string? BannerImageUrl { get; set; }

    public DateTime CreateDate { get; set; }

    public string? CreateFrom { get; set; }

    public string? Creator { get; set; }

    public DateTime? ModifyDate { get; set; }

    public string? ModifyFrom { get; set; }

    public string? Modifier { get; set; }

    public virtual ICollection<TbExhArtwork> TbExhArtwork { get; set; } = new List<TbExhArtwork>();
}
