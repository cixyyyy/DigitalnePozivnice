using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DigitalnePozivnice.Models.ViewModels
{
    /// <summary>Jedna stavka programa (npr. "Vjenčanje u crkvi", 14:00).</summary>
    public class DogadjajUnos
    {
        [StringLength(150)]
        public string? Naziv { get; set; }

        public TimeOnly? Vrijeme { get; set; }

        [StringLength(200)]
        public string? NazivLokacije { get; set; }

        [StringLength(300)]
        public string? Adresa { get; set; }

        public bool JePrazan =>
            string.IsNullOrWhiteSpace(Naziv) && Vrijeme == null &&
            string.IsNullOrWhiteSpace(NazivLokacije) && string.IsNullOrWhiteSpace(Adresa);
    }

    public class KreirajPozivnicuViewModel : IValidatableObject
    {
        private const string BojaRegex = "^#[0-9A-Fa-f]{6}$";
        private const string BojaPoruka = "Izabrana paleta nije ispravna.";

        /// <summary>Popunjeno samo kod izmjene postojeće pozivnice.</summary>
        public int? PozivnicaId { get; set; }

        public int PaketId { get; set; }
        public int SablonId { get; set; }

        /// <summary>Save the date paket nema program (samo datum i poruka).</summary>
        public bool BezProgram { get; set; }

        public string PaketNaziv { get; set; } = "";
        public string PaketOznaka { get; set; } = "";
        public string SablonNaziv { get; set; } = "";
        public string SablonKljuc { get; set; } = "";

        [Required(ErrorMessage = "Izaberite vrstu proslave.")]
        [StringLength(30)]
        public string VrstaProslave { get; set; } = "Vjenčanje";

        /// <summary>Samo za vrstu "Prilagođeno": naslov koji se prikazuje iznad imena.</summary>
        [StringLength(60, ErrorMessage = "Naslov može imati najviše 60 znakova.")]
        public string? NaslovPozivnice { get; set; }

        [Required(ErrorMessage = "Unesite imena.")]
        [StringLength(250)]
        public string ImenaDomacina { get; set; } = "";

        [StringLength(1000)]
        public string? UvodniTekst { get; set; }

        [Required(ErrorMessage = "Izaberite datum proslave.")]
        [DataType(DataType.Date)]
        public DateTime? Datum { get; set; }

        public List<DogadjajUnos> Dogadjaji { get; set; } = new();

        [Required]
        [RegularExpression(BojaRegex, ErrorMessage = BojaPoruka)]
        public string OsnovnaBoja { get; set; } = "#8B7355";

        [Required]
        [RegularExpression(BojaRegex, ErrorMessage = BojaPoruka)]
        public string DodatnaBoja { get; set; } = "#D8C3A5";

        [Required]
        [RegularExpression(BojaRegex, ErrorMessage = BojaPoruka)]
        public string BojaPozadine { get; set; } = "#FFFDF8";

        [StringLength(20)]
        public string StilPozadine { get; set; } = "glatka";

        [StringLength(5)]
        public string Jezik { get; set; } = "sr";

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Datum.HasValue && Datum.Value.Date < DateTime.Today)
                yield return new ValidationResult("Datum ne može biti u prošlosti.", new[] { nameof(Datum) });

            if (BezProgram) yield break;

            var popunjeni = Dogadjaji.Select((d, i) => (d, i)).Where(x => !x.d.JePrazan).ToList();

            if (popunjeni.Count == 0)
            {
                yield return new ValidationResult(
                    "Dodajte barem jednu stavku programa.", new[] { nameof(Dogadjaji) });
                yield break;
            }

            if (popunjeni.Count > 10)
                yield return new ValidationResult(
                    "Program može imati najviše 10 stavki.", new[] { nameof(Dogadjaji) });

            foreach (var (d, i) in popunjeni)
            {
                if (string.IsNullOrWhiteSpace(d.Naziv))
                    yield return new ValidationResult(
                        $"Stavka programa {i + 1}: unesite naziv.", new[] { $"Dogadjaji[{i}].Naziv" });

                if (d.Vrijeme == null)
                    yield return new ValidationResult(
                        $"Stavka programa {i + 1}: izaberite vrijeme.", new[] { $"Dogadjaji[{i}].Vrijeme" });
            }
        }
    }
}
