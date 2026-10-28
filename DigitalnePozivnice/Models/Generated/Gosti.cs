using System;
using System.Collections.Generic;

namespace DigitalnePozivnice.Models.Generated;

public partial class Gosti
{
    public int GostId { get; set; }

    public int PozivnicaId { get; set; }

    public string? ImePrezime { get; set; }

    public string HashPristupnogTokena { get; set; } = null!;

    public int NajviseOsobaPratnje { get; set; }

    public DateTime DatumKreiranja { get; set; }

    public virtual PotvrdeDolaska? PotvrdeDolaska { get; set; }

    public virtual Pozivnice Pozivnica { get; set; } = null!;
}
