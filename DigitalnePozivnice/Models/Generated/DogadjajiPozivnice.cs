using System;
using System.Collections.Generic;

namespace DigitalnePozivnice.Models.Generated;

public partial class DogadjajiPozivnice
{
    public int DogadjajId { get; set; }

    public int PozivnicaId { get; set; }

    public string VrstaDogadjaja { get; set; } = null!;

    public string Naziv { get; set; } = null!;

    public DateTimeOffset? DatumVreme { get; set; }

    public string? NazivLokacije { get; set; }

    public string? Adresa { get; set; }

    public int Redosled { get; set; }

    /// <summary>Ključ ikonice iz InvitationIcons (NULL = automatski izbor prema nazivu i mjestu).</summary>
    public string? Ikona { get; set; }

    public virtual Pozivnice Pozivnica { get; set; } = null!;
}
