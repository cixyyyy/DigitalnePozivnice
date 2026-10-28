
using System;
using System.Collections.Generic;

namespace DigitalnePozivnice.Models.ViewModels
{
    public class PregledPotvrdaViewModel
    {
        public int PozivnicaId { get; set; }
        public string Naslov { get; set; } = string.Empty;

        // Broj gostiju koji su potvrdili dolazak
        public int BrojGostijuDolazi { get; set; }

        // Ukupan broj osoba, uključujući pratnju
        public int BrojOsobaKojeDolaze { get; set; }

        // Broj gostiju koji su odbili dolazak
        public int BrojGostijuNeDolazi { get; set; }

        public List<StavkaPotvrdeViewModel> Potvrde { get; set; }
            = new List<StavkaPotvrdeViewModel>();
    }

    public class StavkaPotvrdeViewModel
    {
        public string ImePrezime { get; set; } = string.Empty;
        public string StatusDolaska { get; set; } = string.Empty;
        public int BrojOsobaPratnje { get; set; }
        public string? Poruka { get; set; }
        public DateTime DatumOdgovora { get; set; }
    }
}
