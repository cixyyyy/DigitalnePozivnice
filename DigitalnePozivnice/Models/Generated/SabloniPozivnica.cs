using System;
using System.Collections.Generic;

namespace DigitalnePozivnice.Models.Generated;

public partial class SabloniPozivnica
{
    public int SablonId { get; set; }

    public string Naziv { get; set; } = null!;

    public string Vrsta { get; set; } = null!;

    public string? SlikaPregleda { get; set; }

    public bool Aktivan { get; set; }

    // Kljuc dizajna u kodu: "romanticni-vrt", "klasicna", "prirodna"
    public string? Kljuc { get; set; }

    public virtual ICollection<Pozivnice> Pozivnices { get; set; } = new List<Pozivnice>();
}
