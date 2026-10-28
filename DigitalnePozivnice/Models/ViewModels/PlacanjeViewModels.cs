using System;
using System.Collections.Generic;
using DigitalnePozivnice.Models.Generated;

namespace DigitalnePozivnice.Models.ViewModels
{
    public class PlacanjeViewModel
    {
        public Pozivnice Pozivnica { get; set; } = null!;
        public Paketi Paket { get; set; } = null!;
        public decimal Iznos { get; set; }
        public decimal Eur { get; set; }
    }

    public class AdminPregledViewModel
    {
        public int CekaUplatu { get; set; }
        public int Placeno { get; set; }
        public decimal PrihodUkupno { get; set; }
        public int Pozivnica { get; set; }
        public int Objavljenih { get; set; }
        public int Korisnika { get; set; }
        public List<Narudzba> Zadnje { get; set; } = new();
        public Dictionary<string, string> Emailovi { get; set; } = new();
    }

    /// <summary>Stranicenje za admin liste.</summary>
    public class Stranicenje
    {
        public const int PoStranici = 20;
        public int Stranica { get; set; } = 1;
        public int Ukupno { get; set; }
        public int UkupnoStranica => Math.Max(1, (int)Math.Ceiling(Ukupno / (double)PoStranici));

        public static (List<T> Stavke, Stranicenje Str) Primijeni<T>(List<T> lista, int stranica)
        {
            var str = new Stranicenje { Ukupno = lista.Count };
            str.Stranica = Math.Clamp(stranica, 1, str.UkupnoStranica);
            var stavke = lista.Skip((str.Stranica - 1) * PoStranici).Take(PoStranici).ToList();
            return (stavke, str);
        }
    }

    public class AdminNarudzbeViewModel
    {
        public List<Narudzba> Narudzbe { get; set; } = new();
        public Dictionary<string, string> Emailovi { get; set; } = new();
        public string? Status { get; set; }
        public string? Q { get; set; }
        public int CekaUplatu { get; set; }
        public int Placeno { get; set; }
        public int Otkazano { get; set; }
        public Stranicenje Str { get; set; } = new();
    }

    public class AdminPozivnicaRed
    {
        public Pozivnice Pozivnica { get; set; } = null!;
        public string Vlasnik { get; set; } = "";
        public bool Placena { get; set; }
        public string? OtvorenaNarudzba { get; set; }
    }

    public class AdminPozivniceViewModel
    {
        public List<AdminPozivnicaRed> Redovi { get; set; } = new();
        public string? Q { get; set; }
        public Stranicenje Str { get; set; } = new();
    }

    public class AdminKorisnikRed
    {
        public string Id { get; set; } = "";
        public string Email { get; set; } = "";
        public string? ImePrezime { get; set; }
        public int Pozivnica { get; set; }
        public int Narudzbi { get; set; }
        public bool Zakljucan { get; set; }
        public bool JeAdmin { get; set; }
    }

    public class AdminPaketForma
    {
        public int PaketId { get; set; }
        public string? Oznaka { get; set; }
        public string Naziv { get; set; } = "";
        public string? Opis { get; set; }
        /// <summary>Cijena kako je unesena (tacka ili zarez); parsira se rucno da ne zavisi od kulture servera.</summary>
        public string CenaUnos { get; set; } = "";
        public bool Aktivan { get; set; }
    }
}
