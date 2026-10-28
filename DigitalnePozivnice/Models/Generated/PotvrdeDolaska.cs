using System;
using System.Collections.Generic;

namespace DigitalnePozivnice.Models.Generated;

public partial class PotvrdeDolaska
{
    public int PotvrdaId { get; set; }

    public int GostId { get; set; }

    public string StatusDolaska { get; set; } = null!;

    public int BrojOsobaPratnje { get; set; }

    public string? Poruka { get; set; }

    public DateTime DatumOdgovora { get; set; }

    public virtual Gosti Gost { get; set; } = null!;
}
