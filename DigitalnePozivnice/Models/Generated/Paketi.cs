using System;
using System.Collections.Generic;

namespace DigitalnePozivnice.Models.Generated;

public partial class Paketi
{
    public int PaketId { get; set; }

    public string Naziv { get; set; } = null!;

    public string Oznaka { get; set; } = null!;

    public string? Opis { get; set; }

    public decimal Cena { get; set; }

    public bool Aktivan { get; set; }

    public virtual ICollection<MogucnostiPaketa> MogucnostiPaketa { get; set; } = new List<MogucnostiPaketa>();

    public virtual ICollection<Pozivnice> Pozivnices { get; set; } = new List<Pozivnice>();
}
