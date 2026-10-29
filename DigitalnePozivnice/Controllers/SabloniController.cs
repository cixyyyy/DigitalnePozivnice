using DigitalnePozivnice.Data.Generated;
using DigitalnePozivnice.Helpers;
using DigitalnePozivnice.Localization;
using DigitalnePozivnice.Models.Generated;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitalnePozivnice.Controllers
{
    public class SabloniController : Controller
    {
        private readonly DigitalnePozivniceScaffoldContext _context;

        public SabloniController(DigitalnePozivniceScaffoldContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? paket)
        {
            if (string.IsNullOrWhiteSpace(paket))
                return RedirectToAction("Index", "Paketi");

            var izabraniPaket = await _context.Paketis
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Oznaka == paket);

            if (izabraniPaket == null)
                return NotFound();

            var aktivni = await _context.SabloniPozivnicas
                .AsNoTracking()
                .Where(s => s.Aktivan)
                .OrderBy(s => s.Naziv)
                .ToListAsync();

            // Save the date: samo animacije. Premium i Exclusive: animacije + premium dizajni.
            var dozvoljeni = aktivni
                .Where(s => InvitationTheme.SablonDozvoljen(paket, s.Kljuc))
                .ToList();

            ViewBag.Paket = paket;
            ViewBag.PaketNaziv = izabraniPaket.Naziv;
            ViewBag.JeSaveTheDate = InvitationTheme.JeSaveTheDate(paket);

            return View(dozvoljeni);
        }

        // Javni primer dizajna sa izmisljenim podacima.
        // ?mini=1 = bez uvodne koverte i bez trake (za male preglede na stranici izbora).
        [HttpGet("/sabloni/demo/{kljuc}")]
        public IActionResult Demo(string kljuc, bool mini = false, string? paket = null)
        {
            kljuc = (kljuc ?? "").Trim().ToLowerInvariant();

            if (!InvitationTheme.Templates.ContainsKey(kljuc))
                return NotFound();

            var jeAnimacija = InvitationTheme.GrupaSablona(kljuc) == InvitationTheme.GrupaSaveTheDate;

            // Demo prati izabrani paket: Save the date = animacija + poruka + odbrojavanje;
            // Premium / Exclusive = sve (program, mapa, potvrda dolaska).
            var oznaka = (paket ?? "").Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(oznaka))
                oznaka = jeAnimacija ? "save-the-date" : "premium";
            // oznake iz baze: save-the-date/osnovni, interaktivna/premium, exclusive
            var samoDatum = InvitationTheme.JeSaveTheDate(oznaka);
            var jeExclusive = InvitationTheme.JeExclusive(oznaka);
            if (!samoDatum && !jeExclusive && !InvitationTheme.ImaRsvp(oznaka))
                oznaka = "premium"; // nepoznata oznaka: pun prikaz
            var nazivPaketa = jeExclusive ? "Exclusive" : samoDatum ? "Save the date" : "Premium";
            var boje = InvitationTheme.DefaultColors(kljuc);
            var datum = DateTime.Today.AddMonths(3);

            var model = new Pozivnice
            {
                PozivnicaId = 0,
                AdresaPozivnice = "primer",
                Naslov = "Naše vjenčanje",
                ImenaDomacina = "Ana i Marko",
                UvodniTekst = InvitationText.Poruke("Vjenčanje")[0],
                OsnovnaBoja = boje.Main,
                DodatnaBoja = boje.Accent,
                BojaPozadine = boje.Bg,
                StilPozadine = InvitationTheme.DefaultStil(kljuc),
                Jezik = Loc.Lang,
                NazivFonta = "Georgia",
                Objavljena = true,
                Paket = new Paketi { Naziv = nazivPaketa, Oznaka = oznaka },
                Sablon = new SabloniPozivnica { Naziv = "Primer", Kljuc = kljuc }
            };

            if (samoDatum)
            {
                model.DogadjajiPozivnices.Add(new DogadjajiPozivnice
                {
                    VrstaDogadjaja = "Vjenčanje",
                    Naziv = "Vjenčanje",
                    DatumVreme = new DateTimeOffset(datum.AddHours(12)),
                    Redosled = 1
                });
            }
            else
            {
                var stavke = new (string Naziv, int Sat, string? Mjesto, string? Adresa)[]
                {
                    ("Okupljanje svatova", 12, "Restoran „Vila“", "Knez Mihailova 1, Beograd"),
                    ("Vjenčanje", 14, "Crkva Svetog Marka", "Bulevar kralja Aleksandra 17, Beograd"),
                    ("Svečani ručak", 17, "Restoran „Vila“", "Knez Mihailova 1, Beograd")
                };

                for (var i = 0; i < stavke.Length; i++)
                {
                    model.DogadjajiPozivnices.Add(new DogadjajiPozivnice
                    {
                        VrstaDogadjaja = "Vjenčanje",
                        Naziv = stavke[i].Naziv,
                        DatumVreme = new DateTimeOffset(datum.AddHours(stavke[i].Sat)),
                        NazivLokacije = stavke[i].Mjesto,
                        Adresa = stavke[i].Adresa,
                        Redosled = i + 1
                    });
                }
            }

            ViewBag.Preview = true;
            ViewBag.JeDemo = true;

            if (mini)
            {
                ViewBag.HideBar = true;
                ViewBag.NoIntro = true;
            }
            else
            {
                ViewBag.PreviewText = Loc.T(string.IsNullOrEmpty(paket)
                    ? "Primjer dizajna – podaci su izmišljeni"
                    : $"Primjer dizajna – paket {nazivPaketa} – podaci su izmišljeni");
                ViewBag.PreviewBackUrl = string.IsNullOrEmpty(paket)
                    ? Url.Action("Index", "Paketi")
                    : Url.Action("Index", "Sabloni", new { paket = oznaka });
            }

            return View("~/Views/Pozivnice/JavnaPozivnica.cshtml", model);
        }
    }
}
