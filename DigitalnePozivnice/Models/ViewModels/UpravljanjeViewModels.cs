using System;
using System.Collections.Generic;
using System.Linq;

namespace DigitalnePozivnice.Models.ViewModels
{
    public class AdminGostViewModel
    {
        public int GostId { get; set; }
        public string Ime { get; set; } = "";
        public bool Dolazi { get; set; }
        public int Pratnja { get; set; }
        public string? Poruka { get; set; }
        public DateTime DatumOdgovora { get; set; }
        public string? Sto { get; set; }

        public int Osoba => Dolazi ? 1 + Pratnja : 0;
    }

    public class PotvrdeAdminViewModel
    {
        public int PozivnicaId { get; set; }
        public string Naslov { get; set; } = "";
        public string? Imena { get; set; }
        public string PaketNaziv { get; set; } = "";
        public bool Exclusive { get; set; }
        public bool Objavljena { get; set; }
        public string? JavniLink { get; set; }
        public DateTimeOffset? DatumDogadjaja { get; set; }
        public List<AdminGostViewModel> Gosti { get; set; } = new();

        public int GostijuDolazi => Gosti.Count(g => g.Dolazi);
        public int GostijuNeDolazi => Gosti.Count(g => !g.Dolazi);
        public int OsobaDolazi => Gosti.Sum(g => g.Osoba);
        public int Pratnja => Gosti.Where(g => g.Dolazi).Sum(g => g.Pratnja);
        public int Ukupno => Gosti.Count;
    }

    // ---------- Raspored sjedenja ----------

    public class StoDto
    {
        public string Kljuc { get; set; } = "";
        public int Id { get; set; }
        public string Naziv { get; set; } = "";
        public int Mjesta { get; set; } = 8;
        public decimal X { get; set; }
        public decimal Y { get; set; }
        public string Oblik { get; set; } = "okrugli";
        public int Rotacija { get; set; }
        public decimal Skala { get; set; } = 1m;
        public decimal SkalaV { get; set; } = 1m;
    }

    public class GostRasporedDto
    {
        public int Id { get; set; }
        public string Ime { get; set; } = "";
        public int Osoba { get; set; }
        public string? Sto { get; set; }
        public bool Rucni { get; set; }
    }

    public class RucniGostZahtjev
    {
        public string? Ime { get; set; }
        public int Osoba { get; set; } = 1;
    }

    public class RasporedViewModel
    {
        public int PozivnicaId { get; set; }
        public string Naslov { get; set; } = "";
        public List<StoDto> Stolovi { get; set; } = new();
        public List<GostRasporedDto> Gosti { get; set; } = new();
    }

    public class DodjelaDto
    {
        public int GostId { get; set; }
        public string? Sto { get; set; }
    }

    public class RasporedSaveRequest
    {
        public List<StoDto> Stolovi { get; set; } = new();
        public List<DodjelaDto> Gosti { get; set; } = new();
    }
}
