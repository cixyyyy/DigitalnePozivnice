
using System.ComponentModel.DataAnnotations;

namespace DigitalnePozivnice.Models.ViewModels
{
    public class PotvrdaDolaskaViewModel
    {
        [Required(ErrorMessage = "Unesi ime i prezime.")]
        [StringLength(200)]
        public string ImePrezime { get; set; } = string.Empty;

        [Required(ErrorMessage = "Izaberi da li dolaziš.")]
        public string StatusDolaska { get; set; } = "Dolazim";

        [Range(0, 10, ErrorMessage = "Broj osoba koje dolaze sa vama mora biti od 0 do 10.")]
        public int BrojOsobaPratnje { get; set; } = 0;

        [StringLength(1000)]
        public string? Poruka { get; set; }
    }
}
