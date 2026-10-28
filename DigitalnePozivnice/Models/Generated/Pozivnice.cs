using System;
using System.Collections.Generic;

namespace DigitalnePozivnice.Models.Generated;

public partial class Pozivnice
{
    public int PozivnicaId { get; set; }

    public string KorisnikId { get; set; } = null!;

    public int PaketId { get; set; }

    public int SablonId { get; set; }

    public string AdresaPozivnice { get; set; } = null!;

    public string Naslov { get; set; } = null!;

    public string? ImenaDomacina { get; set; }

    public string? UvodniTekst { get; set; }

    public string OsnovnaBoja { get; set; } = null!;

    public string DodatnaBoja { get; set; } = null!;

    public string BojaPozadine { get; set; } = null!;

    public string? NazivFonta { get; set; }

    public bool Objavljena { get; set; }

    public DateTime DatumKreiranja { get; set; }

    public DateTime? DatumIzmene { get; set; }

    public virtual ICollection<DogadjajiPozivnice> DogadjajiPozivnices { get; set; } = new List<DogadjajiPozivnice>();

    public virtual ICollection<Gosti> Gostis { get; set; } = new List<Gosti>();

    public virtual ICollection<MedijiPozivnice> MedijiPozivnices { get; set; } = new List<MedijiPozivnice>();

    public virtual Paketi Paket { get; set; } = null!;

    public virtual SabloniPozivnica Sablon { get; set; } = null!;
}
