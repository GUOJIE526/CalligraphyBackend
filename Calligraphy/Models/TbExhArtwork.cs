using System;
using System.Collections.Generic;

namespace Calligraphy.Models;

public partial class TbExhArtwork
{
    public Guid ArtworkId { get; set; }

    public Guid? ExhibitionId { get; set; }

    public string Title { get; set; } = null!;

    public Guid? Writer { get; set; }

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    public DateTime CreatedYear { get; set; }

    public string? Style { get; set; }

    public string? Material { get; set; }

    public string? Dimensions { get; set; }

    public bool IsVisible { get; set; }

    public DateTimeOffset CreateDate { get; set; }

    public string? CreateFrom { get; set; }

    public string? Creator { get; set; }

    public DateTimeOffset? ModifyDate { get; set; }

    public string? ModifyFrom { get; set; }

    public string? Modifier { get; set; }

    public virtual TbExhExhibition? Exhibition { get; set; }

    public virtual ICollection<TbExhComment> TbExhComment { get; set; } = new List<TbExhComment>();

    public virtual ICollection<TbExhLike> TbExhLike { get; set; } = new List<TbExhLike>();

    public virtual TbExhUser? WriterNavigation { get; set; }
}
