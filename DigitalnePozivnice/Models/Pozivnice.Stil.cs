namespace DigitalnePozivnice.Models.Generated;

public partial class Pozivnice
{
    /// <summary>glatka, gradijent, aurora, tackice, linije, papir, sjaj, vinjeta</summary>
    public string StilPozadine { get; set; } = "glatka";

    /// <summary>Jezik pozivnice: sr, hr, bs, sl, de, en.</summary>
    public string Jezik { get; set; } = "sr";

    /// <summary>null = podrazumijevana muzika, "lib:kljuc" = biblioteka, "moja" = vlastiti MP3, "none" = bez muzike.</summary>
    public string? Muzika { get; set; }
}
