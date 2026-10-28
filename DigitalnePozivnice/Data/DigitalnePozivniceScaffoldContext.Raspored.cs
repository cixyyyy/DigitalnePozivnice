using DigitalnePozivnice.Models.Generated;
using Microsoft.EntityFrameworkCore;

namespace DigitalnePozivnice.Data.Generated;

// Dodatak generisanom kontekstu (ne gubi se pri ponovnom scaffold-u)
public partial class DigitalnePozivniceScaffoldContext
{
    public virtual DbSet<Stolovi> Stolovis { get; set; }

    public virtual DbSet<Narudzba> Narudzbe { get; set; }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Stolovi>(entity =>
        {
            entity.HasKey(e => e.StoId);
            entity.ToTable("Stolovi");

            entity.Property(e => e.Naziv).HasMaxLength(60);
            entity.Property(e => e.Oblik).HasMaxLength(20).HasDefaultValue("okrugli");
            entity.Property(e => e.Skala).HasColumnType("decimal(4, 2)");
            entity.Property(e => e.SkalaV).HasColumnType("decimal(4, 2)").HasDefaultValue(1m);
            entity.Property(e => e.PozX).HasColumnType("decimal(6, 2)");
            entity.Property(e => e.PozY).HasColumnType("decimal(6, 2)");

            entity.HasOne(d => d.Pozivnica).WithMany()
                .HasForeignKey(d => d.PozivnicaId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Stolovi_Pozivnice");
        });

        modelBuilder.Entity<Narudzba>(entity =>
        {
            entity.HasKey(e => e.NarudzbaId);
            entity.ToTable("Narudzbe");

            entity.Property(e => e.KorisnikId).HasMaxLength(450);
            entity.Property(e => e.Iznos).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Valuta).HasMaxLength(3).HasDefaultValue("BAM");
            entity.Property(e => e.NacinPlacanja).HasMaxLength(20).HasDefaultValue("uplatnica");
            entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("CekaUplatu");
            entity.Property(e => e.PotvrdioKorisnik).HasMaxLength(256);
            entity.Property(e => e.Napomena).HasMaxLength(500);
            entity.Property(e => e.VanjskiId).HasMaxLength(100);

            entity.HasOne(d => d.Pozivnica).WithMany()
                .HasForeignKey(d => d.PozivnicaId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_Narudzbe_Pozivnice");

            entity.HasOne(d => d.Paket).WithMany()
                .HasForeignKey(d => d.PaketId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_Narudzbe_Paketi");
        });

        modelBuilder.Entity<Pozivnice>(entity =>
        {
            entity.Property(e => e.StilPozadine).HasMaxLength(20).HasDefaultValue("glatka");
            entity.Property(e => e.Jezik).HasMaxLength(5).HasDefaultValue("sr");
            entity.Property(e => e.Muzika).HasMaxLength(60);
        });

        modelBuilder.Entity<Gosti>(entity =>
        {
            entity.Property(e => e.JeRucni).HasDefaultValue(false);

            entity.HasOne(d => d.Sto).WithMany(p => p.Gosti)
                .HasForeignKey(d => d.StoId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_Gosti_Stolovi");
        });
    }
}
