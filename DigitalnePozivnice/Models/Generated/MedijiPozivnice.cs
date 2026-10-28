using System;
using System.Collections.Generic;

namespace DigitalnePozivnice.Models.Generated;

public partial class MedijiPozivnice
{
    public int MedijId { get; set; }

    public int PozivnicaId { get; set; }

    public string PutanjaFajla { get; set; } = null!;

    public string? Opis { get; set; }

    public int Redosled { get; set; }

    public virtual Pozivnice Pozivnica { get; set; } = null!;
}
