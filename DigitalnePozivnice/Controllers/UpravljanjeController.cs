using System.Security.Claims;
using System.Text;
using DigitalnePozivnice.Data.Generated;
using DigitalnePozivnice.Helpers;
using DigitalnePozivnice.Models.Generated;
using DigitalnePozivnice.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitalnePozivnice.Controllers
{
    /// <summary>Admin panel vlasnika pozivnice: potvrde dolaska, izvoz, raspored sjedenja.</summary>
    [Authorize]
    public class UpravljanjeController : Controller
    {
        private readonly DigitalnePozivniceScaffoldContext _context;

        public UpravljanjeController(DigitalnePozivniceScaffoldContext context)
        {
            _context = context;
        }

        private async Task<Pozivnice?> UcitajAsync(int id, bool pratiPromjene = false)
        {
            var korisnikId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            IQueryable<Pozivnice> upit = _context.Pozivnices
                .Include(p => p.Paket)
                .Include(p => p.DogadjajiPozivnices);

            if (!pratiPromjene) upit = upit.AsNoTracking();

            return await upit.FirstOrDefaultAsync(p =>
                p.PozivnicaId == id && p.KorisnikId == korisnikId);
        }

        private async Task<List<Gosti>> GostiSaOdgovorimaAsync(int pozivnicaId, bool pratiPromjene = false, bool bezRucnih = false)
        {
            IQueryable<Gosti> upit = _context.Gostis
                .Include(g => g.PotvrdeDolaska)
                .Include(g => g.Sto)
                .Where(g => g.PozivnicaId == pozivnicaId && g.PotvrdeDolaska != null);

            if (bezRucnih) upit = upit.Where(g => !g.JeRucni);

            if (!pratiPromjene) upit = upit.AsNoTracking();

            return await upit.OrderBy(g => g.ImePrezime).ToListAsync();
        }

        private static readonly string[] Oblici = { "okrugli", "pravougaoni" };

        private static bool Dolazi(Gosti g) =>
            string.Equals(g.PotvrdeDolaska?.StatusDolaska?.Trim(), "Dolazim", StringComparison.OrdinalIgnoreCase);

        // ---------------- Potvrde dolaska ----------------

        [HttpGet("/moje-pozivnice/{id:int}/potvrde")]
        public async Task<IActionResult> Potvrde(int id)
        {
            var pozivnica = await UcitajAsync(id);
            if (pozivnica == null || !InvitationTheme.ImaRsvp(pozivnica.Paket.Oznaka))
                return NotFound();

            var gosti = await GostiSaOdgovorimaAsync(id, bezRucnih: true);

            var model = new PotvrdeAdminViewModel
            {
                PozivnicaId = pozivnica.PozivnicaId,
                Naslov = pozivnica.Naslov,
                Imena = pozivnica.ImenaDomacina,
                PaketNaziv = pozivnica.Paket.Naziv,
                Exclusive = InvitationTheme.JeExclusive(pozivnica.Paket.Oznaka),
                Objavljena = pozivnica.Objavljena,
                JavniLink = Url.Action("JavnaPozivnica", "Pozivnice",
                    new { adresa = pozivnica.AdresaPozivnice }, Request.Scheme),
                DatumDogadjaja = InvitationTheme.PrviDogadjaj(pozivnica)?.DatumVreme,
                Gosti = gosti.Select(g => new AdminGostViewModel
                {
                    GostId = g.GostId,
                    Ime = g.ImePrezime ?? "Nepoznato ime",
                    Dolazi = Dolazi(g),
                    Pratnja = g.PotvrdeDolaska!.BrojOsobaPratnje,
                    Poruka = g.PotvrdeDolaska.Poruka,
                    DatumOdgovora = g.PotvrdeDolaska.DatumOdgovora,
                    Sto = g.Sto?.Naziv
                }).ToList()
            };

            return View(model);
        }

        [HttpGet("/moje-pozivnice/{id:int}/potvrde/izvoz")]
        public async Task<IActionResult> Izvoz(int id)
        {
            var pozivnica = await UcitajAsync(id);
            if (pozivnica == null || !InvitationTheme.ImaRsvp(pozivnica.Paket.Oznaka))
                return NotFound();

            var gosti = await GostiSaOdgovorimaAsync(id, bezRucnih: true);

            static string Csv(string? v)
            {
                v ??= "";
                // zastita od "CSV injection" u Excelu
                if (v.Length > 0 && "=+-@".Contains(v[0])) v = "'" + v;
                return "\"" + v.Replace("\"", "\"\"") + "\"";
            }

            var sb = new StringBuilder();
            sb.AppendLine("Ime i prezime;Odgovor;Ukupno osoba;Pratnja;Sto;Poruka;Datum odgovora");

            foreach (var g in gosti)
            {
                var dolazi = Dolazi(g);
                var pratnja = g.PotvrdeDolaska!.BrojOsobaPratnje;
                sb.AppendLine(string.Join(";",
                    Csv(g.ImePrezime),
                    Csv(dolazi ? "Dolazi" : "Ne dolazi"),
                    dolazi ? 1 + pratnja : 0,
                    dolazi ? pratnja : 0,
                    Csv(g.Sto?.Naziv),
                    Csv(g.PotvrdeDolaska.Poruka),
                    Csv(g.PotvrdeDolaska.DatumOdgovora.ToString("dd.MM.yyyy. HH:mm"))));
            }

            var bajtovi = Encoding.UTF8.GetPreamble()
                .Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();

            return File(bajtovi, "text/csv; charset=utf-8", $"potvrde-{pozivnica.AdresaPozivnice}.csv");
        }

        // ---------------- Raspored sjedenja (Exclusive) ----------------

        [HttpGet("/moje-pozivnice/{id:int}/raspored")]
        public async Task<IActionResult> Raspored(int id)
        {
            var pozivnica = await UcitajAsync(id);
            if (pozivnica == null || !InvitationTheme.JeExclusive(pozivnica.Paket.Oznaka))
                return NotFound();

            var stolovi = await _context.Stolovis
                .AsNoTracking()
                .Where(s => s.PozivnicaId == id)
                .OrderBy(s => s.StoId)
                .ToListAsync();

            var gosti = (await GostiSaOdgovorimaAsync(id)).Where(Dolazi).ToList();

            var model = new RasporedViewModel
            {
                PozivnicaId = pozivnica.PozivnicaId,
                Naslov = pozivnica.ImenaDomacina ?? pozivnica.Naslov,
                Stolovi = stolovi.Select(s => new StoDto
                {
                    Kljuc = "s" + s.StoId,
                    Id = s.StoId,
                    Naziv = s.Naziv,
                    Mjesta = s.BrojMjesta,
                    X = s.PozX,
                    Y = s.PozY,
                    Oblik = s.Oblik == "pravougaoni" ? "pravougaoni" : "okrugli",
                    Rotacija = s.Rotacija,
                    Skala = s.Skala,
                    SkalaV = s.SkalaV
                }).ToList(),
                Gosti = gosti.Select(g => new GostRasporedDto
                {
                    Id = g.GostId,
                    Ime = g.ImePrezime ?? "Nepoznato ime",
                    Osoba = 1 + g.PotvrdeDolaska!.BrojOsobaPratnje,
                    Sto = g.StoId.HasValue ? "s" + g.StoId.Value : null,
                    Rucni = g.JeRucni
                }).ToList()
            };

            return View(model);
        }

        [HttpPost("/moje-pozivnice/{id:int}/raspored")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SacuvajRaspored(int id, [FromBody] RasporedSaveRequest zahtjev)
        {
            var pozivnica = await UcitajAsync(id, pratiPromjene: true);
            if (pozivnica == null || !InvitationTheme.JeExclusive(pozivnica.Paket.Oznaka))
                return NotFound();

            if (zahtjev == null || zahtjev.Stolovi.Count > 60)
                return BadRequest("Neispravan zahtjev.");

            var postojeci = await _context.Stolovis
                .Where(s => s.PozivnicaId == id).ToListAsync();

            var gosti = (await GostiSaOdgovorimaAsync(id, pratiPromjene: true)).Where(Dolazi).ToList();

            var mapa = new Dictionary<string, Stolovi>();
            var zadrzani = new HashSet<int>();

            foreach (var dto in zahtjev.Stolovi)
            {
                if (string.IsNullOrWhiteSpace(dto.Kljuc) || mapa.ContainsKey(dto.Kljuc))
                    return BadRequest("Neispravan sto.");

                Stolovi sto;
                if (dto.Id > 0)
                {
                    var nadjen = postojeci.FirstOrDefault(s => s.StoId == dto.Id);
                    if (nadjen == null) return BadRequest("Sto ne postoji.");
                    sto = nadjen;
                    zadrzani.Add(sto.StoId);
                }
                else
                {
                    sto = new Stolovi { PozivnicaId = id };
                    _context.Stolovis.Add(sto);
                }

                var naziv = (dto.Naziv ?? "").Trim();
                sto.Naziv = string.IsNullOrEmpty(naziv) ? "Sto" : (naziv.Length > 60 ? naziv[..60] : naziv);
                sto.PozX = Math.Clamp(dto.X, 0, 100);
                sto.PozY = Math.Clamp(dto.Y, 0, 100);
                sto.Oblik = Oblici.Contains(dto.Oblik) ? dto.Oblik : "okrugli";
                sto.BrojMjesta = Math.Clamp(dto.Mjesta, 1, sto.Oblik == "okrugli" ? 16 : 30);
                sto.Rotacija = sto.Oblik == "pravougaoni" && dto.Rotacija == 90 ? 90 : 0;
                var maxSkala = sto.Oblik == "pravougaoni" ? 3m : 2m;
                sto.Skala = Math.Clamp(dto.Skala, 0.5m, maxSkala);
                sto.SkalaV = Math.Clamp(dto.SkalaV <= 0 ? 1m : dto.SkalaV, 0.5m, 3m);

                mapa[dto.Kljuc] = sto;
            }

            // Brisanje stolova koji su uklonjeni
            foreach (var s in postojeci.Where(s => !zadrzani.Contains(s.StoId)))
            {
                foreach (var g in gosti.Where(g => g.StoId == s.StoId)) g.Sto = null;
                _context.Stolovis.Remove(s);
            }

            // Dodjele
            foreach (var g in gosti) g.Sto = null;

            foreach (var dodjela in zahtjev.Gosti)
            {
                var gost = gosti.FirstOrDefault(g => g.GostId == dodjela.GostId);
                if (gost == null) continue;

                if (!string.IsNullOrEmpty(dodjela.Sto) && mapa.TryGetValue(dodjela.Sto, out var sto))
                    gost.Sto = sto;
            }

            // Provjera popunjenosti
            foreach (var par in mapa)
            {
                var zauzeto = gosti.Where(g => g.Sto == par.Value)
                    .Sum(g => 1 + g.PotvrdeDolaska!.BrojOsobaPratnje);

                if (zauzeto > par.Value.BrojMjesta)
                    return BadRequest($"„{par.Value.Naziv}“ je prepunjen ({zauzeto}/{par.Value.BrojMjesta}).");
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                stolovi = mapa.Select(p => new { kljuc = p.Key, id = p.Value.StoId })
            });
        }

        // ---------------- Ručno dodani gosti ----------------

        [HttpPost("/moje-pozivnice/{id:int}/raspored/gost")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DodajRucnogGosta(int id, [FromBody] RucniGostZahtjev zahtjev)
        {
            var pozivnica = await UcitajAsync(id);
            if (pozivnica == null || !InvitationTheme.JeExclusive(pozivnica.Paket.Oznaka))
                return NotFound();

            var ime = (zahtjev?.Ime ?? "").Trim();
            if (ime.Length < 1 || ime.Length > 200)
                return BadRequest("Unesite ime gosta (do 200 znakova).");

            var osoba = Math.Clamp(zahtjev!.Osoba, 1, 20);

            var brojRucnih = await _context.Gostis.CountAsync(g => g.PozivnicaId == id && g.JeRucni);
            if (brojRucnih >= 300)
                return BadRequest("Dostignut je maksimalan broj ručno dodanih gostiju.");

            var gost = new Gosti
            {
                PozivnicaId = id,
                ImePrezime = ime,
                HashPristupnogTokena = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(
                    System.Security.Cryptography.RandomNumberGenerator.GetBytes(32))),
                NajviseOsobaPratnje = osoba - 1,
                DatumKreiranja = DateTime.UtcNow,
                JeRucni = true
            };

            var potvrda = new PotvrdeDolaska
            {
                Gost = gost,
                StatusDolaska = "Dolazim",
                BrojOsobaPratnje = osoba - 1,
                DatumOdgovora = DateTime.UtcNow
            };

            _context.Gostis.Add(gost);
            _context.PotvrdeDolaskas.Add(potvrda);
            await _context.SaveChangesAsync();

            return Ok(new { id = gost.GostId, ime = gost.ImePrezime, osoba });
        }

        [HttpPost("/moje-pozivnice/{id:int}/raspored/gost/{gostId:int}/ukloni")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UkloniRucnogGosta(int id, int gostId)
        {
            var pozivnica = await UcitajAsync(id);
            if (pozivnica == null || !InvitationTheme.JeExclusive(pozivnica.Paket.Oznaka))
                return NotFound();

            var gost = await _context.Gostis
                .Include(g => g.PotvrdeDolaska)
                .FirstOrDefaultAsync(g => g.GostId == gostId && g.PozivnicaId == id && g.JeRucni);

            if (gost == null) return NotFound();

            if (gost.PotvrdeDolaska != null) _context.PotvrdeDolaskas.Remove(gost.PotvrdeDolaska);
            _context.Gostis.Remove(gost);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
