using System;

namespace DigitalnePozivnice.Models.Generated;

public static class NarudzbaStatus
{
    public const string CekaUplatu = "CekaUplatu";
    public const string Placeno = "Placeno";
    public const string Otkazano = "Otkazano";
    /// <summary>Otkazao admin: korisnik ne moze ponovo platiti ni objaviti ovu pozivnicu (osim ako admin vrati narudzbu).</summary>
    public const string Ponisteno = "Ponisteno";

    public static bool JeOtkazano(string? s) => s == Otkazano || s == Ponisteno;

    public static string Naziv(string? s) => s switch
    {
        CekaUplatu => "Čeka uplatu",
        Placeno => "Plaćeno",
        Otkazano => "Otkazano",
        Ponisteno => "Otkazano (admin)",
        _ => s ?? ""
    };

    public static string Css(string? s) => s switch
    {
        CekaUplatu => "ap-st-ceka",
        Placeno => "ap-st-placeno",
        Otkazano => "ap-st-otkazano",
        Ponisteno => "ap-st-otkazano",
        _ => ""
    };
}

/// <summary>Narudzba paketa za jednu pozivnicu. Pozivnica se objavljuje tek kad je narudzba placena.</summary>
public partial class Narudzba
{
    public int NarudzbaId { get; set; }

    public string KorisnikId { get; set; } = null!;

    public int PozivnicaId { get; set; }

    public int PaketId { get; set; }

    /// <summary>Iznos u KM (BAM).</summary>
    public decimal Iznos { get; set; }

    public string Valuta { get; set; } = "BAM";

    /// <summary>uplatnica, kartica, rucno</summary>
    public string NacinPlacanja { get; set; } = "uplatnica";

    public string Status { get; set; } = NarudzbaStatus.CekaUplatu;

    public DateTime DatumKreiranja { get; set; }

    public DateTime? DatumPlacanja { get; set; }

    /// <summary>Ko je potvrdio uplatu (email admina).</summary>
    public string? PotvrdioKorisnik { get; set; }

    public string? Napomena { get; set; }

    /// <summary>Referenca kod platnog provajdera (za karticu).</summary>
    public string? VanjskiId { get; set; }

    public virtual Pozivnice Pozivnica { get; set; } = null!;

    public virtual Paketi Paket { get; set; } = null!;

    /// <summary>Poziv na broj / broj narudzbe, npr. 000123.</summary>
    public string PozivNaBroj => NarudzbaId.ToString("D6");
}
