namespace DigitalnePozivnice.Models.Generated;

/// <summary>Sto u rasporedu sjedenja (Exclusive paket).</summary>
public partial class Stolovi
{
    public int StoId { get; set; }

    public int PozivnicaId { get; set; }

    public string Naziv { get; set; } = null!;

    public int BrojMjesta { get; set; }

    /// <summary>Položaj u sali, 0–100 (% širine).</summary>
    public decimal PozX { get; set; }

    /// <summary>Položaj u sali, 0–100 (% visine).</summary>
    public decimal PozY { get; set; }

    /// <summary>okrugli / kvadratni / pravougaoni</summary>
    public string Oblik { get; set; } = "okrugli";

    /// <summary>0 ili 90 stepeni (samo pravougaoni).</summary>
    public int Rotacija { get; set; }

    /// <summary>Veličina stola, 0.7 – 1.6.</summary>
    public decimal Skala { get; set; } = 1m;

    /// <summary>Visina (širina) pravougaonog stola, 0.5 – 3. Skala je dužina.</summary>
    public decimal SkalaV { get; set; } = 1m;

    public virtual Pozivnice Pozivnica { get; set; } = null!;

    public virtual ICollection<Gosti> Gosti { get; set; } = new List<Gosti>();
}
