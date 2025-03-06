using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace IoNote.DatabaseModels;

public partial class ionoteContext : DbContext
{
    public ionoteContext()
    {
    }

    public ionoteContext(DbContextOptions<ionoteContext> options)
        : base(options)
    {
    }

    public virtual DbSet<folder> folders { get; set; }

    public virtual DbSet<image> images { get; set; }

    public virtual DbSet<note> notes { get; set; }

    public virtual DbSet<user> users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Database=ionote;Username=postgres;Password=postgres;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<folder>(entity =>
        {
            entity.HasKey(e => e.folderid).HasName("folders_pkey");

            entity.Property(e => e.createdat).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.parentfolder).WithMany(p => p.Inverseparentfolder)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("folders_parentfolderid_fkey");
        });

        modelBuilder.Entity<image>(entity =>
        {
            entity.HasKey(e => e.imageid).HasName("images_pkey");

            entity.Property(e => e.createdat).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.note).WithMany(p => p.images).HasConstraintName("images_noteid_fkey");
        });

        modelBuilder.Entity<note>(entity =>
        {
            entity.HasKey(e => e.noteid).HasName("notes_pkey");

            entity.Property(e => e.background).HasDefaultValue(0);
            entity.Property(e => e.createdat).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.folder).WithMany(p => p.notes)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("notes_folderid_fkey");
        });

        modelBuilder.Entity<user>(entity =>
        {
            entity.HasKey(e => e.userid).HasName("users_pkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
