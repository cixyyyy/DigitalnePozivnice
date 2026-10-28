using System;
using System.Collections.Generic;

namespace DigitalnePozivnice.Models.Generated;

public partial class MogucnostiPaketa
{
    public int MogucnostPaketaId { get; set; }

    public int PaketId { get; set; }

    public string NazivMogucnosti { get; set; } = null!;

    public virtual Paketi Paket { get; set; } = null!;
}
