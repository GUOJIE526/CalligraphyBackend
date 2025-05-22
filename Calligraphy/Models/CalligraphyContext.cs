using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Calligraphy.Models;

public partial class CalligraphyContext : DbContext
{
    public CalligraphyContext()
    {
    }

    public CalligraphyContext(DbContextOptions<CalligraphyContext> options)
        : base(options)
    {
    }

    public virtual DbSet<TbExhArtwork> TbExhArtwork { get; set; }

    public virtual DbSet<TbExhComment> TbExhComment { get; set; }

    public virtual DbSet<TbExhExhibition> TbExhExhibition { get; set; }

    public virtual DbSet<TbExhLike> TbExhLike { get; set; }

    public virtual DbSet<TbExhLog> TbExhLog { get; set; }

    public virtual DbSet<TbExhUser> TbExhUser { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TbExhArtwork>(entity =>
        {
            entity.HasKey(e => e.ArtworkId).HasName("PK__TB_EXH_A__9ADB548114C62116");

            entity.ToTable("TB_EXH_ARTWORK");

            entity.Property(e => e.ArtworkId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("ARTWORK_ID");
            entity.Property(e => e.CreateDate).HasColumnName("CREATE_DATE");
            entity.Property(e => e.CreateFrom)
                .HasMaxLength(50)
                .HasColumnName("CREATE_FROM");
            entity.Property(e => e.CreatedYear)
                .HasColumnType("datetime")
                .HasColumnName("CREATED_YEAR");
            entity.Property(e => e.Creator)
                .HasMaxLength(50)
                .HasColumnName("CREATOR");
            entity.Property(e => e.CreatorId).HasColumnName("CREATOR_ID");
            entity.Property(e => e.Description).HasColumnName("DESCRIPTION");
            entity.Property(e => e.Dimensions)
                .HasMaxLength(50)
                .HasColumnName("DIMENSIONS");
            entity.Property(e => e.ExhibitionId).HasColumnName("EXHIBITION_ID");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(255)
                .HasColumnName("IMAGE_URL");
            entity.Property(e => e.IsVisible)
                .HasDefaultValue(true)
                .HasColumnName("IS_VISIBLE");
            entity.Property(e => e.Material)
                .HasMaxLength(100)
                .HasColumnName("MATERIAL");
            entity.Property(e => e.Modifier)
                .HasMaxLength(50)
                .HasColumnName("MODIFIER");
            entity.Property(e => e.ModifyDate).HasColumnName("MODIFY_DATE");
            entity.Property(e => e.ModifyFrom)
                .HasMaxLength(50)
                .HasColumnName("MODIFY_FROM");
            entity.Property(e => e.Style)
                .HasMaxLength(100)
                .HasColumnName("STYLE");
            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .HasColumnName("TITLE");

            entity.HasOne(d => d.CreatorNavigation).WithMany(p => p.TbExhArtwork)
                .HasForeignKey(d => d.CreatorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ARTWORK_CREATOR");

            entity.HasOne(d => d.Exhibition).WithMany(p => p.TbExhArtwork)
                .HasForeignKey(d => d.ExhibitionId)
                .HasConstraintName("FK__TB_EXH_AR__EXHIB__412EB0B6");
        });

        modelBuilder.Entity<TbExhComment>(entity =>
        {
            entity.HasKey(e => e.CommentId).HasName("PK__TB_EXH_C__C55F98D07E115907");

            entity.ToTable("TB_EXH_COMMENT");

            entity.Property(e => e.CommentId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("COMMENT_ID");
            entity.Property(e => e.ArtworkId).HasColumnName("ARTWORK_ID");
            entity.Property(e => e.CreateDate).HasColumnName("CREATE_DATE");
            entity.Property(e => e.CreateFrom)
                .HasMaxLength(50)
                .HasColumnName("CREATE_FROM");
            entity.Property(e => e.Creator)
                .HasMaxLength(50)
                .HasColumnName("CREATOR");
            entity.Property(e => e.Message)
                .HasMaxLength(500)
                .HasColumnName("MESSAGE");
            entity.Property(e => e.Modifier)
                .HasMaxLength(50)
                .HasColumnName("MODIFIER");
            entity.Property(e => e.ModifyDate).HasColumnName("MODIFY_DATE");
            entity.Property(e => e.ModifyFrom)
                .HasMaxLength(50)
                .HasColumnName("MODIFY_FROM");
            entity.Property(e => e.Reply)
                .HasMaxLength(500)
                .HasColumnName("REPLY");
            entity.Property(e => e.ReplyTime).HasColumnName("REPLY_TIME");
            entity.Property(e => e.UserName)
                .HasMaxLength(100)
                .HasColumnName("USER_NAME");

            entity.HasOne(d => d.Artwork).WithMany(p => p.TbExhComment)
                .HasForeignKey(d => d.ArtworkId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TB_EXH_CO__ARTWO__45F365D3");
        });

        modelBuilder.Entity<TbExhExhibition>(entity =>
        {
            entity.HasKey(e => e.ExhibitionId).HasName("PK__TB_EXH_E__72C7FC8E23298682");

            entity.ToTable("TB_EXH_EXHIBITION");

            entity.Property(e => e.ExhibitionId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("EXHIBITION_ID");
            entity.Property(e => e.BannerImageUrl)
                .HasMaxLength(255)
                .HasColumnName("BANNER_IMAGE_URL");
            entity.Property(e => e.CreateDate).HasColumnName("CREATE_DATE");
            entity.Property(e => e.CreateFrom)
                .HasMaxLength(50)
                .HasColumnName("CREATE_FROM");
            entity.Property(e => e.Creator)
                .HasMaxLength(50)
                .HasColumnName("CREATOR");
            entity.Property(e => e.Description).HasColumnName("DESCRIPTION");
            entity.Property(e => e.EndDate).HasColumnName("END_DATE");
            entity.Property(e => e.Modifier)
                .HasMaxLength(50)
                .HasColumnName("MODIFIER");
            entity.Property(e => e.ModifyDate).HasColumnName("MODIFY_DATE");
            entity.Property(e => e.ModifyFrom)
                .HasMaxLength(50)
                .HasColumnName("MODIFY_FROM");
            entity.Property(e => e.StartDate).HasColumnName("START_DATE");
            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .HasColumnName("TITLE");
        });

        modelBuilder.Entity<TbExhLike>(entity =>
        {
            entity.HasKey(e => e.LikeId).HasName("PK__TB_EXH_L__4DA4A00440330A9F");

            entity.ToTable("TB_EXH_LIKE");

            entity.Property(e => e.LikeId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("LIKE_ID");
            entity.Property(e => e.ArtworkId).HasColumnName("ARTWORK_ID");
            entity.Property(e => e.CreateDate).HasColumnName("CREATE_DATE");
            entity.Property(e => e.CreateFrom)
                .HasMaxLength(50)
                .HasColumnName("CREATE_FROM");
            entity.Property(e => e.Creator)
                .HasMaxLength(50)
                .HasColumnName("CREATOR");
            entity.Property(e => e.IpAddress)
                .HasMaxLength(45)
                .IsUnicode(false)
                .HasColumnName("IP_ADDRESS");
            entity.Property(e => e.Modifier)
                .HasMaxLength(50)
                .HasColumnName("MODIFIER");
            entity.Property(e => e.ModifyDate).HasColumnName("MODIFY_DATE");
            entity.Property(e => e.ModifyFrom)
                .HasMaxLength(50)
                .HasColumnName("MODIFY_FROM");

            entity.HasOne(d => d.Artwork).WithMany(p => p.TbExhLike)
                .HasForeignKey(d => d.ArtworkId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TB_EXH_LI__ARTWO__49C3F6B7");
        });

        modelBuilder.Entity<TbExhLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__TB_EXH_L__4364C882D47F5F7D");

            entity.ToTable("TB_EXH_LOG");

            entity.Property(e => e.LogId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("LOG_ID");
            entity.Property(e => e.Action)
                .HasMaxLength(100)
                .HasColumnName("ACTION");
            entity.Property(e => e.CreateDate).HasColumnName("CREATE_DATE");
            entity.Property(e => e.CreateFrom)
                .HasMaxLength(50)
                .HasColumnName("CREATE_FROM");
            entity.Property(e => e.Creator)
                .HasMaxLength(50)
                .HasColumnName("CREATOR");
            entity.Property(e => e.IpAddress)
                .HasMaxLength(45)
                .IsUnicode(false)
                .HasColumnName("IP_ADDRESS");
            entity.Property(e => e.Message)
                .HasMaxLength(500)
                .HasColumnName("MESSAGE");
            entity.Property(e => e.Modifier)
                .HasMaxLength(50)
                .HasColumnName("MODIFIER");
            entity.Property(e => e.ModifyDate).HasColumnName("MODIFY_DATE");
            entity.Property(e => e.ModifyFrom)
                .HasMaxLength(50)
                .HasColumnName("MODIFY_FROM");
            entity.Property(e => e.TargetId)
                .HasMaxLength(50)
                .HasColumnName("TARGET_ID");
            entity.Property(e => e.TargetType)
                .HasMaxLength(50)
                .HasColumnName("TARGET_TYPE");
            entity.Property(e => e.UserId).HasColumnName("USER_ID");

            entity.HasOne(d => d.User).WithMany(p => p.TbExhLog)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TB_EXH_LO__USER___4D94879B");
        });

        modelBuilder.Entity<TbExhUser>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__TB_EXH_U__F3BEEBFF4F937892");

            entity.ToTable("TB_EXH_USER");

            entity.HasIndex(e => e.Email, "UQ__TB_EXH_U__B15BE12E7166DFEC").IsUnique();

            entity.Property(e => e.UserId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("USER_ID");
            entity.Property(e => e.CreateDate).HasColumnName("CREATE_DATE");
            entity.Property(e => e.CreateFrom)
                .HasMaxLength(50)
                .HasColumnName("CREATE_FROM");
            entity.Property(e => e.Creator)
                .HasMaxLength(50)
                .HasColumnName("CREATOR");
            entity.Property(e => e.DisplayName)
                .HasMaxLength(100)
                .HasColumnName("DISPLAY_NAME");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("EMAIL");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("IS_ACTIVE");
            entity.Property(e => e.MailConfirm).HasColumnName("MAIL_CONFIRM");
            entity.Property(e => e.MailConfirmcode)
                .HasMaxLength(50)
                .HasColumnName("MAIL_CONFIRMCODE");
            entity.Property(e => e.MailConfirmdate)
                .HasColumnType("datetime")
                .HasColumnName("MAIL_CONFIRMDATE");
            entity.Property(e => e.Modifier)
                .HasMaxLength(50)
                .HasColumnName("MODIFIER");
            entity.Property(e => e.ModifyDate).HasColumnName("MODIFY_DATE");
            entity.Property(e => e.ModifyFrom)
                .HasMaxLength(50)
                .HasColumnName("MODIFY_FROM");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("PASSWORD_HASH");
            entity.Property(e => e.RestpwdConfirm).HasColumnName("RESTPWD_CONFIRM");
            entity.Property(e => e.RestpwdLimitdate)
                .HasColumnType("datetime")
                .HasColumnName("RESTPWD_LIMITDATE");
            entity.Property(e => e.RestpwdToken)
                .HasMaxLength(50)
                .HasColumnName("RESTPWD_TOKEN");
            entity.Property(e => e.Role)
                .HasMaxLength(50)
                .HasColumnName("ROLE");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
