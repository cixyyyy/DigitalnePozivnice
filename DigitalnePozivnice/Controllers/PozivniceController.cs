
using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using DigitalnePozivnice.Data.Generated;
using DigitalnePozivnice.Localization;
using DigitalnePozivnice.Models.Generated;
using DigitalnePozivnice.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DigitalnePozivnice.Helpers;

namespace DigitalnePozivnice.Controllers
{
    [Authorize]
    public class PozivniceController : Controller
    {
        private readonly DigitalnePozivniceScaffoldContext _context;

        public PozivniceController(
            DigitalnePozivniceScaffoldContext context)
        {
            _context = context;
        }

        private static bool PaketImaPotvrduDolaska(string? oznaka)
        {
            oznaka = (oznaka ?? "").Trim().ToLowerInvariant();

            return oznaka == "premium"
                || oznaka == "interaktivna"
                || oznaka == "exclusive";
        }

        // ------------------------------- CREATE -------------------------------

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Create(string? paket, int sablonId)
        {
            // Posjetilac bez naloga: umjesto greske vodimo ga na pravljenje naloga,
            // pa se nakon toga vraca tacno na ovaj korak.
            if (User.Identity?.IsAuthenticated != true)
            {
                var nazad = Url.Action("Create", "Pozivnice", new { paket, sablonId });
                return RedirectToAction("Register", "Account", new { returnUrl = nazad });
            }

            if (string.IsNullOrWhiteSpace(paket))
                return BadRequest("Nije izabran paket.");

            var izabraniPaket = await _context.Paketis
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Oznaka == paket);

            var sablon = await _context.SabloniPozivnicas
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.SablonId == sablonId && s.Aktivan);

            if (izabraniPaket == null || sablon == null)
                return NotFound("Paket ili šablon ne postoji.");

            if (!InvitationTheme.SablonDozvoljen(izabraniPaket.Oznaka, sablon.Kljuc))
                return BadRequest("Izabrani dizajn nije dostupan za ovaj paket.");

            var boje = InvitationTheme.DefaultColors(sablon.Kljuc);
            var vrsta = InvitationText.Vrste[0];

            var model = new KreirajPozivnicuViewModel
            {
                PaketId = izabraniPaket.PaketId,
                PaketNaziv = izabraniPaket.Naziv,
                PaketOznaka = izabraniPaket.Oznaka,
                BezProgram = InvitationTheme.JeSaveTheDate(izabraniPaket.Oznaka),
                SablonId = sablon.SablonId,
                SablonNaziv = sablon.Naziv,
                SablonKljuc = InvitationTheme.NormalizeKey(sablon.Kljuc),

                VrstaProslave = vrsta.Naziv,
                UvodniTekst = InvitationText.Poruke(vrsta.Naziv)[0],

                OsnovnaBoja = boje.Main,
                DodatnaBoja = boje.Accent,
                BojaPozadine = boje.Bg,
                StilPozadine = InvitationTheme.DefaultStil(sablon.Kljuc),
                Jezik = Loc.Lang
            };

            if (!model.BezProgram)
            {
                foreach (var (naziv, vrijeme) in InvitationText.Program(vrsta.Naziv))
                    model.Dogadjaji.Add(new DogadjajUnos { Naziv = naziv, Vrijeme = TimeOnly.Parse(vrijeme) });
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(KreirajPozivnicuViewModel model)
        {
            var korisnikId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(korisnikId))
                return Challenge();

            var paket = await _context.Paketis
                .FirstOrDefaultAsync(p => p.PaketId == model.PaketId);

            var sablon = await _context.SabloniPozivnicas
                .FirstOrDefaultAsync(s => s.SablonId == model.SablonId && s.Aktivan);

            if (paket == null || sablon == null)
                return NotFound("Paket ili šablon ne postoji.");

            if (!InvitationTheme.SablonDozvoljen(paket.Oznaka, sablon.Kljuc))
                return BadRequest("Izabrani dizajn nije dostupan za ovaj paket.");

            model.PaketNaziv = paket.Naziv;
            model.PaketOznaka = paket.Oznaka;
            model.SablonNaziv = sablon.Naziv;
            model.SablonKljuc = InvitationTheme.NormalizeKey(sablon.Kljuc);
            model.BezProgram = InvitationTheme.JeSaveTheDate(paket.Oznaka);

            if (!ModelState.IsValid)
                return View(model);

            var naslov = InvitationText.NaslovPozivnice(model.VrstaProslave, model.NaslovPozivnice);
            var vrstaNaziv = InvitationText.Nadji(model.VrstaProslave).Naziv;

            var novaPozivnica = new Pozivnice
            {
                KorisnikId = korisnikId,
                PaketId = paket.PaketId,
                SablonId = sablon.SablonId,

                AdresaPozivnice = await GenerisiAdresuPozivniceAsync(model.ImenaDomacina, naslov),

                Naslov = naslov,
                ImenaDomacina = model.ImenaDomacina.Trim(),
                UvodniTekst = string.IsNullOrWhiteSpace(model.UvodniTekst) ? null : model.UvodniTekst.Trim(),

                OsnovnaBoja = model.OsnovnaBoja,
                DodatnaBoja = model.DodatnaBoja,
                BojaPozadine = model.BojaPozadine,
                StilPozadine = InvitationTheme.StilPozadineOk(model.StilPozadine),
                Jezik = Loc.Normalize(model.Jezik),
                NazivFonta = "Georgia",

                Objavljena = false,
                DatumKreiranja = DateTime.UtcNow
            };

            foreach (var d in NapraviDogadjaje(model, model.BezProgram, vrstaNaziv))
            {
                d.Pozivnica = novaPozivnica;
                _context.DogadjajiPozivnices.Add(d);
            }

            _context.Pozivnices.Add(novaPozivnica);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = novaPozivnica.PozivnicaId });
        }

        // -------------------------------- EDIT --------------------------------

        // Admin je otkazao narudzbu: pozivnica je trajno otkazana i ne moze se otvoriti ni uredjivati.
        private async Task<IActionResult?> OtkazanaAsync(int pozivnicaId)
        {
            if (!await PlacanjeHelper.JeBlokiranaAsync(_context, pozivnicaId)) return null;
            TempData["Poruka"] = "Ova pozivnica je otkazana od strane administratora.";
            return RedirectToAction("Index", "MojePozivnice");
        }

        [HttpGet("/moje-pozivnice/{id:int}/uredi")]
        public async Task<IActionResult> Edit(int id)
        {
            var korisnikId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var p = await _context.Pozivnices
                .AsNoTracking()
                .Include(x => x.Paket)
                .Include(x => x.Sablon)
                .Include(x => x.DogadjajiPozivnices)
                .FirstOrDefaultAsync(x => x.PozivnicaId == id && x.KorisnikId == korisnikId);

            if (p == null)
                return NotFound();

            var otk = await OtkazanaAsync(p.PozivnicaId);
            if (otk != null) return otk;

            var dogadjaji = p.DogadjajiPozivnices.OrderBy(d => d.Redosled).ToList();
            var prvi = dogadjaji.FirstOrDefault();
            var bezProgram = InvitationTheme.JeSaveTheDate(p.Paket.Oznaka);

            var model = new KreirajPozivnicuViewModel
            {
                PozivnicaId = p.PozivnicaId,
                PaketId = p.PaketId,
                PaketNaziv = p.Paket.Naziv,
                PaketOznaka = p.Paket.Oznaka,
                BezProgram = bezProgram,
                SablonId = p.SablonId,
                SablonNaziv = p.Sablon.Naziv,
                SablonKljuc = InvitationTheme.NormalizeKey(p.Sablon.Kljuc),

                VrstaProslave = InvitationText.Nadji(prvi?.VrstaDogadjaja).Naziv,
                ImenaDomacina = p.ImenaDomacina ?? "",
                NaslovPozivnice = InvitationText.JeCustom(prvi?.VrstaDogadjaja) ? p.Naslov : null,
                UvodniTekst = p.UvodniTekst,
                Datum = prvi?.DatumVreme?.Date,

                OsnovnaBoja = p.OsnovnaBoja,
                DodatnaBoja = p.DodatnaBoja,
                BojaPozadine = p.BojaPozadine,
                StilPozadine = InvitationTheme.StilPozadineOk(p.StilPozadine),
                Jezik = Loc.Normalize(p.Jezik)
            };

            if (!bezProgram)
            {
                foreach (var d in dogadjaji)
                {
                    model.Dogadjaji.Add(new DogadjajUnos
                    {
                        Naziv = d.Naziv,
                        Vrijeme = d.DatumVreme.HasValue ? TimeOnly.FromDateTime(d.DatumVreme.Value.DateTime) : null,
                        NazivLokacije = d.NazivLokacije,
                        Adresa = d.Adresa
                    });
                }
            }

            return View(model);
        }

        [HttpPost("/moje-pozivnice/{id:int}/uredi")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, KreirajPozivnicuViewModel model)
        {
            var korisnikId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var p = await _context.Pozivnices
                .Include(x => x.Paket)
                .Include(x => x.Sablon)
                .Include(x => x.DogadjajiPozivnices)
                .FirstOrDefaultAsync(x => x.PozivnicaId == id && x.KorisnikId == korisnikId);

            if (p == null)
                return NotFound();

            var otk = await OtkazanaAsync(p.PozivnicaId);
            if (otk != null) return otk;

            // Paket i dizajn se ne mijenjaju – uzimaju se iz baze
            model.PozivnicaId = id;
            model.PaketId = p.PaketId;
            model.SablonId = p.SablonId;
            model.PaketNaziv = p.Paket.Naziv;
            model.PaketOznaka = p.Paket.Oznaka;
            model.SablonNaziv = p.Sablon.Naziv;
            model.SablonKljuc = InvitationTheme.NormalizeKey(p.Sablon.Kljuc);
            model.BezProgram = InvitationTheme.JeSaveTheDate(p.Paket.Oznaka);

            if (!ModelState.IsValid)
                return View(model);

            var vrstaNaziv = InvitationText.Nadji(model.VrstaProslave).Naziv;

            p.Naslov = InvitationText.NaslovPozivnice(model.VrstaProslave, model.NaslovPozivnice);
            p.ImenaDomacina = model.ImenaDomacina.Trim();
            p.UvodniTekst = string.IsNullOrWhiteSpace(model.UvodniTekst) ? null : model.UvodniTekst.Trim();
            p.OsnovnaBoja = model.OsnovnaBoja;
            p.DodatnaBoja = model.DodatnaBoja;
            p.BojaPozadine = model.BojaPozadine;
            p.StilPozadine = InvitationTheme.StilPozadineOk(model.StilPozadine);
            p.Jezik = Loc.Normalize(model.Jezik);
            p.DatumIzmene = DateTime.UtcNow;

            _context.DogadjajiPozivnices.RemoveRange(p.DogadjajiPozivnices.ToList());

            foreach (var d in NapraviDogadjaje(model, model.BezProgram, vrstaNaziv))
            {
                d.PozivnicaId = p.PozivnicaId;
                _context.DogadjajiPozivnices.Add(d);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = p.PozivnicaId });
        }



        // ---------------------------- Pregled uzivo ----------------------------

        [HttpPost("/pozivnice/pregled-uzivo")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PreviewLive(KreirajPozivnicuViewModel model)
        {
            Loc.Use(HttpContext, model.Jezik);
            var sablon = await _context.SabloniPozivnicas
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.SablonId == model.SablonId && s.Aktivan);

            var paket = await _context.Paketis
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PaketId == model.PaketId);

            if (sablon == null || paket == null)
                return NotFound();

            var vrsta = InvitationText.Nadji(model.VrstaProslave);
            var boje = InvitationTheme.DefaultColors(sablon.Kljuc);
            var bezProgram = InvitationTheme.JeSaveTheDate(paket.Oznaka);

            var pozivnica = new Pozivnice
            {
                AdresaPozivnice = "pregled",
                Naslov = InvitationText.NaslovPozivnice(model.VrstaProslave, model.NaslovPozivnice),
                ImenaDomacina = string.IsNullOrWhiteSpace(model.ImenaDomacina)
                    ? vrsta.ImenaPrimjer
                    : model.ImenaDomacina.Trim(),
                UvodniTekst = string.IsNullOrWhiteSpace(model.UvodniTekst) ? null : model.UvodniTekst.Trim(),
                OsnovnaBoja = InvitationTheme.Color(model.OsnovnaBoja, boje.Main),
                DodatnaBoja = InvitationTheme.Color(model.DodatnaBoja, boje.Accent),
                BojaPozadine = InvitationTheme.Color(model.BojaPozadine, boje.Bg),
                StilPozadine = InvitationTheme.StilPozadineOk(model.StilPozadine),
                Jezik = Loc.Normalize(model.Jezik),
                NazivFonta = "Georgia",
                Objavljena = true,
                Paket = paket,
                Sablon = sablon
            };

            var datum = (model.Datum ?? DateTime.Today.AddMonths(3)).Date;

            if (bezProgram)
            {
                pozivnica.DogadjajiPozivnices.Add(new DogadjajiPozivnice
                {
                    VrstaDogadjaja = vrsta.Naziv,
                    Naziv = InvitationText.JeCustom(vrsta.Naziv) ? InvitationText.NaslovPozivnice(vrsta.Naziv, model.NaslovPozivnice) : vrsta.Naziv,
                    DatumVreme = new DateTimeOffset(datum.AddHours(12)),
                    Redosled = 1
                });
            }
            else
            {
                var stavke = (model.Dogadjaji ?? new List<DogadjajUnos>())
                    .Where(d => !string.IsNullOrWhiteSpace(d.Naziv))
                    .OrderBy(d => d.Vrijeme)
                    .ToList();

                if (stavke.Count == 0)
                {
                    var (n, v) = InvitationText.Program(vrsta.Naziv)[0];
                    stavke.Add(new DogadjajUnos { Naziv = n, Vrijeme = TimeOnly.Parse(v) });
                }

                var red = 1;
                foreach (var d in stavke)
                {
                    pozivnica.DogadjajiPozivnices.Add(new DogadjajiPozivnice
                    {
                        VrstaDogadjaja = vrsta.Naziv,
                        Naziv = d.Naziv!.Trim(),
                        DatumVreme = new DateTimeOffset(datum + (d.Vrijeme ?? new TimeOnly(12, 0)).ToTimeSpan()),
                        NazivLokacije = d.NazivLokacije?.Trim(),
                        Adresa = d.Adresa?.Trim(),
                        Redosled = red++
                    });
                }
            }

            ViewBag.Preview = true;
            ViewBag.HideBar = true;
            ViewBag.NoIntro = true;

            return View("JavnaPozivnica", pozivnica);
        }


        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var korisnikId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            var pozivnica = await _context.Pozivnices
                .AsNoTracking()
                .Include(p => p.Paket)
                .Include(p => p.Sablon)
                .Include(p => p.DogadjajiPozivnices)
                .Include(p => p.MedijiPozivnices)
                .FirstOrDefaultAsync(p =>
                    p.PozivnicaId == id &&
                    p.KorisnikId == korisnikId);

            if (pozivnica == null)
                return NotFound();

            var otk = await OtkazanaAsync(pozivnica.PozivnicaId);
            if (otk != null) return otk;

            ViewBag.MozeObjaviti = await PlacanjeHelper.MozeObjavitiAsync(
                _context, pozivnica.PozivnicaId, pozivnica.Paket.Cena);

            ViewBag.Blokirana = await PlacanjeHelper.JeBlokiranaAsync(_context, pozivnica.PozivnicaId);

            ViewBag.NarudzbaId = await _context.Narudzbe
                .Where(n => n.PozivnicaId == pozivnica.PozivnicaId && n.Status == NarudzbaStatus.CekaUplatu)
                .OrderByDescending(n => n.NarudzbaId)
                .Select(n => (int?)n.NarudzbaId)
                .FirstOrDefaultAsync();

            return View(pozivnica);
        }

        // Vlasnik gleda pozivnicu (nacrt ili objavljenu). ?mini=1 = bez trake i koverte (za okvir u Details)
        [HttpGet("/pozivnica-pregled/{id:int}")]
        public async Task<IActionResult> Pregled(int id, bool mini = false)
        {
            var korisnikId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var pozivnica = await _context.Pozivnices
                .AsNoTracking()
                .Include(p => p.Paket)
                .Include(p => p.Sablon)
                .Include(p => p.DogadjajiPozivnices)
                .Include(p => p.MedijiPozivnices)
                .FirstOrDefaultAsync(p => p.PozivnicaId == id && p.KorisnikId == korisnikId);

            if (pozivnica == null)
                return NotFound();

            var otk = await OtkazanaAsync(pozivnica.PozivnicaId);
            if (otk != null) return otk;

            ViewBag.Preview = true;

            if (mini)
            {
                ViewBag.HideBar = true;
                ViewBag.NoIntro = true;
            }
            else
            {
                ViewBag.PreviewText = pozivnica.Objavljena
                    ? "Pregled objavljene pozivnice"
                    : "Pregled nacrta – pozivnica još nije objavljena";
                ViewBag.PreviewBackUrl = Url.Action("Details", new { id });
            }

            return View("JavnaPozivnica", pozivnica);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Objavi(int id)
        {
            var korisnikId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            var pozivnica = await _context.Pozivnices
                .Include(p => p.Paket)
                .FirstOrDefaultAsync(p =>
                    p.PozivnicaId == id &&
                    p.KorisnikId == korisnikId);

            if (pozivnica == null)
                return NotFound();

            if (!pozivnica.Objavljena && await PlacanjeHelper.JeBlokiranaAsync(_context, pozivnica.PozivnicaId))
                return RedirectToAction("Details", new { id = pozivnica.PozivnicaId });

            // Objava je moguca tek kad je paket placen (ili je besplatan).
            if (!pozivnica.Objavljena &&
                !await PlacanjeHelper.MozeObjavitiAsync(_context, pozivnica.PozivnicaId, pozivnica.Paket.Cena))
            {
                return RedirectToAction("Placanje", "Narudzbe", new { pozivnicaId = pozivnica.PozivnicaId });
            }

            pozivnica.Objavljena = true;

            await _context.SaveChangesAsync();

            return RedirectToAction(
                "Details",
                new { id = pozivnica.PozivnicaId });
        }

        // ----------------- Javna stranica i potvrda dolaska -----------------

        private static string RsvpCookie(int pozivnicaId) => "rsvp_" + pozivnicaId;

        private async Task PostaviMojOdgovorAsync(Pozivnice pozivnica)
        {
            if (!Request.Cookies.TryGetValue(RsvpCookie(pozivnica.PozivnicaId), out var vrijednost)
                || !int.TryParse(vrijednost, out var gostId))
                return;

            var potvrda = await _context.PotvrdeDolaskas
                .AsNoTracking()
                .Include(x => x.Gost)
                .FirstOrDefaultAsync(x => x.GostId == gostId && x.Gost.PozivnicaId == pozivnica.PozivnicaId);

            if (potvrda == null) return;

            ViewBag.VecOdgovorio = true;
            ViewBag.MojOdgovor = potvrda.StatusDolaska;
            ViewBag.MojaPratnja = potvrda.BrojOsobaPratnje;
            ViewBag.MojeIme = potvrda.Gost.ImePrezime;
        }

        [AllowAnonymous]
        [HttpGet("/pozivnica/{adresa}")]
        public async Task<IActionResult> JavnaPozivnica(string adresa, bool potvrdjeno = false)
        {
            if (string.IsNullOrWhiteSpace(adresa))
                return NotFound();

            var pozivnica = await _context.Pozivnices
                .AsNoTracking()
                .Include(p => p.Sablon)
                .Include(p => p.DogadjajiPozivnices)
                .Include(p => p.MedijiPozivnices)
                .Include(p => p.Paket)
                .FirstOrDefaultAsync(p => p.AdresaPozivnice == adresa && p.Objavljena);

            if (pozivnica == null)
                return NotFound();

            ViewBag.Potvrdjeno = potvrdjeno;
            await PostaviMojOdgovorAsync(pozivnica);

            return View("JavnaPozivnica", pozivnica);
        }

        [AllowAnonymous]
        [HttpPost("/pozivnica/{adresa}/potvrda")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PotvrdiDolazak(string adresa, PotvrdaDolaskaViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest("Provjerite unesene podatke.");

            if (model.StatusDolaska != "Dolazim" && model.StatusDolaska != "Ne dolazim")
                return BadRequest("Izaberite važeći odgovor.");

            var pozivnica = await _context.Pozivnices
                .Include(p => p.Paket)
                .FirstOrDefaultAsync(p => p.AdresaPozivnice == adresa && p.Objavljena);

            if (pozivnica == null)
                return NotFound();

            if (!PaketImaPotvrduDolaska(pozivnica.Paket?.Oznaka))
                return NotFound();

            // Odgovor se moze poslati samo jednom po uredjaju/pregledacu
            if (Request.Cookies.TryGetValue(RsvpCookie(pozivnica.PozivnicaId), out var postojeci)
                && int.TryParse(postojeci, out var postojeciGost)
                && await _context.Gostis.AnyAsync(g => g.GostId == postojeciGost && g.PozivnicaId == pozivnica.PozivnicaId))
            {
                return RedirectToAction(nameof(JavnaPozivnica), new { adresa });
            }

            var brojOsoba = model.StatusDolaska == "Dolazim" ? model.BrojOsobaPratnje : 0;

            var gost = new Gosti
            {
                PozivnicaId = pozivnica.PozivnicaId,
                ImePrezime = model.ImePrezime.Trim(),
                HashPristupnogTokena = Convert.ToHexString(SHA256.HashData(RandomNumberGenerator.GetBytes(32))),
                NajviseOsobaPratnje = brojOsoba,
                DatumKreiranja = DateTime.UtcNow
            };

            var potvrda = new PotvrdeDolaska
            {
                Gost = gost,
                StatusDolaska = model.StatusDolaska,
                BrojOsobaPratnje = brojOsoba,
                Poruka = string.IsNullOrWhiteSpace(model.Poruka) ? null : model.Poruka.Trim(),
                DatumOdgovora = DateTime.UtcNow
            };

            _context.Gostis.Add(gost);
            _context.PotvrdeDolaskas.Add(potvrda);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                var detalj = ex.GetBaseException().Message;
                return StatusCode(500, "Odgovor trenutno nije moguće sačuvati. " + detalj);
            }

            Response.Cookies.Append(RsvpCookie(pozivnica.PozivnicaId), gost.GostId.ToString(), new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Lax,
                Secure = Request.IsHttps,
                IsEssential = true,
                Expires = DateTimeOffset.UtcNow.AddYears(1)
            });

            return RedirectToAction(nameof(JavnaPozivnica), new { adresa, potvrdjeno = true });
        }



        [HttpGet]
        public async Task<IActionResult> PregledPotvrda(int id)
        {
            var korisnikId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var pozivnica = await _context.Pozivnices
                .Include(p => p.Paket)
                .FirstOrDefaultAsync(p =>
                    p.PozivnicaId == id &&
                    p.KorisnikId == korisnikId);

            if (pozivnica == null)
                return NotFound();

            var otk = await OtkazanaAsync(pozivnica.PozivnicaId);
            if (otk != null) return otk;

            var oznakaPaketa = (pozivnica.Paket?.Oznaka ?? "")
                .Trim()
                .ToLowerInvariant();

            var imaPregledPotvrda =
                oznakaPaketa == "premium" ||
                oznakaPaketa == "interaktivna" ||
                oznakaPaketa == "exclusive";

            if (!imaPregledPotvrda)
                return Forbid();

            var gosti = await _context.Gostis
                .Where(g => g.PozivnicaId == id)
                .Include(g => g.PotvrdeDolaska)
                .OrderBy(g => g.ImePrezime)
                .ToListAsync();

            var odgovori = gosti
                .Where(g => g.PotvrdeDolaska != null)
                .Select(g => new StavkaPotvrdeViewModel
                {
                    ImePrezime = g.ImePrezime ?? "Nepoznato ime",
                    StatusDolaska = g.PotvrdeDolaska!.StatusDolaska,
                    BrojOsobaPratnje = g.PotvrdeDolaska.BrojOsobaPratnje,
                    Poruka = g.PotvrdeDolaska.Poruka,
                    DatumOdgovora = g.PotvrdeDolaska.DatumOdgovora
                })
                .ToList();

            var dolaze = odgovori
                .Where(o => o.StatusDolaska.Trim()
                    .Equals("Dolazim", StringComparison.OrdinalIgnoreCase))
                .ToList();

            var neDolaze = odgovori
                .Count(o => o.StatusDolaska.Trim()
                    .Equals("Ne dolazim", StringComparison.OrdinalIgnoreCase));

            var model = new PregledPotvrdaViewModel
            {
                PozivnicaId = pozivnica.PozivnicaId,
                Naslov = pozivnica.Naslov,
                BrojGostijuDolazi = dolaze.Count,
                BrojOsobaKojeDolaze = dolaze.Sum(o => 1 + o.BrojOsobaPratnje),
                BrojGostijuNeDolazi = neDolaze,
                Potvrde = odgovori
            };

            return View(model);
        }


        private async Task<string> GenerisiAdresuPozivniceAsync(
            string? imenaDomacina, string naslov)
        {
            var tekst = !string.IsNullOrWhiteSpace(imenaDomacina)
                ? imenaDomacina
                : naslov;

            tekst = tekst.Trim().ToLowerInvariant();

            tekst = tekst
                .Replace("đ", "dj")
                .Replace("č", "c")
                .Replace("ć", "c")
                .Replace("š", "s")
                .Replace("ž", "z");

            tekst = tekst.Normalize(NormalizationForm.FormD);

            var rezultat = new StringBuilder();

            foreach (var karakter in tekst)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(karakter)
                    == UnicodeCategory.NonSpacingMark)
                {
                    continue;
                }

                rezultat.Append(karakter);
            }

            tekst = rezultat.ToString()
                .Normalize(NormalizationForm.FormC);

            tekst = Regex.Replace(tekst, @"[^a-z0-9]+", "-");
            tekst = tekst.Trim('-');

            if (string.IsNullOrWhiteSpace(tekst))
                tekst = "pozivnica";

            var adresa = tekst;
            var broj = 1;

            while (await _context.Pozivnices
                .AsNoTracking()
                .AnyAsync(p => p.AdresaPozivnice == adresa))
            {
                adresa = $"{tekst}{broj}";
                broj++;
            }

            return adresa;
        }

        // ------------------------------------------------------------------
        // Zajednicka pomocna metoda: pravi stavke programa iz forme
        // (Save the date paket nema program – dobija jednu stavku sa datumom)
        // ------------------------------------------------------------------
        private static List<DogadjajiPozivnice> NapraviDogadjaje(
            KreirajPozivnicuViewModel model, bool bezProgram, string vrstaNaziv)
        {
            var datum = model.Datum!.Value.Date;
            var rezultat = new List<DogadjajiPozivnice>();

            if (bezProgram)
            {
                rezultat.Add(new DogadjajiPozivnice
                {
                    VrstaDogadjaja = vrstaNaziv,
                    Naziv = InvitationText.JeCustom(vrstaNaziv) ? InvitationText.NaslovPozivnice(vrstaNaziv, model.NaslovPozivnice) : vrstaNaziv,
                    DatumVreme = new DateTimeOffset(datum.AddHours(12)),
                    Redosled = 1
                });
                return rezultat;
            }

            var redosled = 1;
            foreach (var d in model.Dogadjaji.Where(x => !x.JePrazan).OrderBy(x => x.Vrijeme))
            {
                rezultat.Add(new DogadjajiPozivnice
                {
                    VrstaDogadjaja = vrstaNaziv,
                    Naziv = d.Naziv!.Trim(),
                    DatumVreme = new DateTimeOffset(datum + d.Vrijeme!.Value.ToTimeSpan()),
                    NazivLokacije = string.IsNullOrWhiteSpace(d.NazivLokacije) ? null : d.NazivLokacije.Trim(),
                    Adresa = string.IsNullOrWhiteSpace(d.Adresa) ? null : d.Adresa.Trim(),
                    Redosled = redosled++
                });
            }
            return rezultat;
        }



    }
}
