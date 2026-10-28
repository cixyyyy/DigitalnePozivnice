using System;
using System.Collections.Generic;
using DigitalnePozivnice.Models.Generated;
using Microsoft.EntityFrameworkCore;

namespace DigitalnePozivnice.Data.Generated;

public partial class DigitalnePozivniceScaffoldContext : DbContext
{
    public DigitalnePozivniceScaffoldContext(DbContextOptions<DigitalnePozivniceScaffoldContext> options)
        : base(options)
    {
    }

    public virtual DbSet<DogadjajiPozivnice> DogadjajiPozivnices { get; set; }

    public virtual DbSet<Gosti> Gostis { get; set; }

    public virtual DbSet<MedijiPozivnice> MedijiPozivnices { get; set; }

    public virtual DbSet<MogucnostiPaketa> MogucnostiPaketa { get; set; }

    public virtual DbSet<Paketi> Paketis { get; set; }

    public virtual DbSet<PotvrdeDolaska> PotvrdeDolaskas { get; set; }

    public virtual DbSet<Pozivnice> Pozivnices { get; set; }

    public virtual DbSet<SabloniPozivnica> SabloniPozivnicas { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DogadjajiPozivnice>(entity =>
        {
            entity.HasKey(e => e.DogadjajId).HasName("PK__Dogadjaj__060E75B1E16625C8");

            entity.ToTable("DogadjajiPozivnice");

            entity.Property(e => e.Adresa).HasMaxLength(300);
            entity.Property(e => e.Naziv).HasMaxLength(150);
            entity.Property(e => e.NazivLokacije).HasMaxLength(200);
            entity.Property(e => e.VrstaDogadjaja).HasMaxLength(50);

            entity.HasOne(d => d.Pozivnica).WithMany(p => p.DogadjajiPozivnices)
                .HasForeignKey(d => d.PozivnicaId)
                .HasConstraintName("FK_DogadjajiPozivnice_Pozivnice");
        });

        modelBuilder.Entity<Gosti>(entity =>
        {
            entity.HasKey(e => e.GostId).HasName("PK__Gosti__A6C83EF67D268EB0");

            entity.ToTable("Gosti");

            entity.HasIndex(e => e.PozivnicaId, "IX_Gosti_PozivnicaId");

            entity.HasIndex(e => e.HashPristupnogTokena, "UQ_Gosti_HashTokena").IsUnique();

            entity.Property(e => e.DatumKreiranja).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.HashPristupnogTokena)
                .HasMaxLength(64)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.ImePrezime).HasMaxLength(200);

            entity.HasOne(d => d.Pozivnica).WithMany(p => p.Gostis)
                .HasForeignKey(d => d.PozivnicaId)
                .HasConstraintName("FK_Gosti_Pozivnice");
        });

        modelBuilder.Entity<MedijiPozivnice>(entity =>
        {
            entity.HasKey(e => e.MedijId).HasName("PK__MedijiPo__21F7E37BFCB9B571");

            entity.ToTable("MedijiPozivnice");

            entity.Property(e => e.Opis).HasMaxLength(200);
            entity.Property(e => e.PutanjaFajla).HasMaxLength(500);

            entity.HasOne(d => d.Pozivnica).WithMany(p => p.MedijiPozivnices)
                .HasForeignKey(d => d.PozivnicaId)
                .HasConstraintName("FK_MedijiPozivnice_Pozivnice");
        });

        modelBuilder.Entity<MogucnostiPaketa>(entity =>
        {
            entity.HasKey(e => e.MogucnostPaketaId).HasName("PK__Mogucnos__C6D1794358ACF91A");

            entity.HasIndex(e => new { e.PaketId, e.NazivMogucnosti }, "UQ_MogucnostiPaketa").IsUnique();

            entity.Property(e => e.NazivMogucnosti).HasMaxLength(100);

            entity.HasOne(d => d.Paket).WithMany(p => p.MogucnostiPaketa)
                .HasForeignKey(d => d.PaketId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MogucnostiPaketa_Paketi");
        });

        modelBuilder.Entity<Paketi>(entity =>
        {
            entity.HasKey(e => e.PaketId).HasName("PK__Paketi__2F326F998B9CCDD4");

            entity.ToTable("Paketi");

            entity.HasIndex(e => e.Oznaka, "UQ__Paketi__F642A30E2FC545E5").IsUnique();

            entity.Property(e => e.Aktivan).HasDefaultValue(true);
            entity.Property(e => e.Cena).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Naziv).HasMaxLength(100);
            entity.Property(e => e.Opis).HasMaxLength(500);
            entity.Property(e => e.Oznaka).HasMaxLength(50);
        });

        modelBuilder.Entity<PotvrdeDolaska>(entity =>
        {
            entity.HasKey(e => e.PotvrdaId).HasName("PK__PotvrdeD__86CB3CC28E743000");

            entity.ToTable("PotvrdeDolaska");

            entity.HasIndex(e => e.GostId, "UQ_PotvrdeDolaska_Gost").IsUnique();

            entity.Property(e => e.DatumOdgovora).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Poruka).HasMaxLength(1000);
            entity.Property(e => e.StatusDolaska).HasMaxLength(20);

            entity.HasOne(d => d.Gost).WithOne(p => p.PotvrdeDolaska)
                .HasForeignKey<PotvrdeDolaska>(d => d.GostId)
                .HasConstraintName("FK_PotvrdeDolaska_Gosti");
        });

        modelBuilder.Entity<Pozivnice>(entity =>
        {
            entity.HasKey(e => e.PozivnicaId).HasName("PK__Pozivnic__7ABFAF30E7FD8891");

            entity.ToTable("Pozivnice");

            entity.HasIndex(e => e.KorisnikId, "IX_Pozivnice_KorisnikId");

            entity.HasIndex(e => e.AdresaPozivnice, "UQ_Pozivnice_Adresa").IsUnique();

            entity.Property(e => e.AdresaPozivnice).HasMaxLength(150);
            entity.Property(e => e.BojaPozadine)
                .HasMaxLength(7)
                .IsUnicode(false)
                .HasDefaultValue("#FFFFFF")
                .IsFixedLength();
            entity.Property(e => e.DatumKreiranja).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.DodatnaBoja)
                .HasMaxLength(7)
                .IsUnicode(false)
                .HasDefaultValue("#B89B68")
                .IsFixedLength();
            entity.Property(e => e.ImenaDomacina).HasMaxLength(200);
            entity.Property(e => e.Naslov).HasMaxLength(200);
            entity.Property(e => e.NazivFonta).HasMaxLength(100);
            entity.Property(e => e.OsnovnaBoja)
                .HasMaxLength(7)
                .IsUnicode(false)
                .HasDefaultValue("#FFFFFF")
                .IsFixedLength();
            entity.Property(e => e.UvodniTekst).HasMaxLength(1000);

            entity.HasOne(d => d.Paket).WithMany(p => p.Pozivnices)
                .HasForeignKey(d => d.PaketId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Pozivnice_Paketi");

            entity.HasOne(d => d.Sablon).WithMany(p => p.Pozivnices)
                .HasForeignKey(d => d.SablonId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Pozivnice_Sabloni");
        });

        modelBuilder.Entity<SabloniPozivnica>(entity =>
        {
            entity.HasKey(e => e.SablonId).HasName("PK__SabloniP__5283F13EE81FBA34");

            entity.ToTable("SabloniPozivnica");

            entity.Property(e => e.Aktivan).HasDefaultValue(true);
            entity.Property(e => e.Kljuc).HasMaxLength(50);
            entity.Property(e => e.Naziv).HasMaxLength(150);
            entity.Property(e => e.SlikaPregleda).HasMaxLength(500);
            entity.Property(e => e.Vrsta).HasMaxLength(30);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
