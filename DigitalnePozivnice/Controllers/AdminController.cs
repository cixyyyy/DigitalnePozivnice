using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DigitalnePozivnice.Data.Generated;
using DigitalnePozivnice.Helpers;
using DigitalnePozivnice.Models;
using DigitalnePozivnice.Models.Generated;
using DigitalnePozivnice.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitalnePozivnice.Controllers
{
    /// <summary>Admin panel sajta: narudzbe, uplate, pozivnice, paketi, korisnici.</summary>
    [Authorize(Policy = "Admin")]
    [Route("admin")]
    public class AdminController : Controller
    {
        private readonly DigitalnePozivniceScaffoldContext _context;
        private readonly UserManager<ApplicationUser> _users;
        private readonly AdminProvjera _admin;

        public AdminController(
            DigitalnePozivniceScaffoldContext context,
            UserManager<ApplicationUser> users,
            AdminProvjera admin)
        {
            _context = context;
            _users = users;
            _admin = admin;
        }

        private async Task<Dictionary<string, string>> EmailoviAsync()
        {
            return await _users.Users
                .AsNoTracking()
                .Where(u => u.Email != null)
                .ToDictionaryAsync(u => u.Id, u => u.Email!);
        }

        // ------------------------------ Pregled ------------------------------

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var vm = new AdminPregledViewModel
            {
                CekaUplatu = await _context.Narudzbe.CountAsync(n => n.Status == NarudzbaStatus.CekaUplatu),
                Placeno = await _context.Narudzbe.CountAsync(n => n.Status == NarudzbaStatus.Placeno),
                PrihodUkupno = await _context.Narudzbe
                    .Where(n => n.Status == NarudzbaStatus.Placeno)
                    .SumAsync(n => (decimal?)n.Iznos) ?? 0m,
                Pozivnica = await _context.Pozivnices.CountAsync(),
                Objavljenih = await _context.Pozivnices.CountAsync(p => p.Objavljena),
                Korisnika = await _users.Users.CountAsync(),
                Zadnje = await _context.Narudzbe
                    .AsNoTracking()
                    .Include(n => n.Paket)
                    .Include(n => n.Pozivnica)
                    .OrderByDescending(n => n.NarudzbaId)
                    .Take(8)
                    .ToListAsync(),
                Emailovi = await EmailoviAsync()
            };

            return View(vm);
        }

        // ------------------------------ Narudzbe ------------------------------

        [HttpGet("narudzbe")]
        public async Task<IActionResult> Narudzbe(string? status, string? q, int str = 1)
        {
            var emailovi = await EmailoviAsync();

            IQueryable<Narudzba> upit = _context.Narudzbe
                .AsNoTracking()
                .Include(n => n.Paket)
                .Include(n => n.Pozivnica);

            var vm = new AdminNarudzbeViewModel
            {
                Status = status,
                Q = q,
                Emailovi = emailovi,
                CekaUplatu = await _context.Narudzbe.CountAsync(n => n.Status == NarudzbaStatus.CekaUplatu),
                Placeno = await _context.Narudzbe.CountAsync(n => n.Status == NarudzbaStatus.Placeno),
                Otkazano = await _context.Narudzbe.CountAsync(n => n.Status == NarudzbaStatus.Otkazano || n.Status == NarudzbaStatus.Ponisteno)
            };

            if (status == NarudzbaStatus.Otkazano)
                upit = upit.Where(n => n.Status == NarudzbaStatus.Otkazano || n.Status == NarudzbaStatus.Ponisteno);
            else if (!string.IsNullOrWhiteSpace(status))
                upit = upit.Where(n => n.Status == status);

            var lista = await upit.OrderByDescending(n => n.NarudzbaId).ToListAsync();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var t = q.Trim();
                lista = lista.Where(n =>
                        n.PozivNaBroj.Contains(t, StringComparison.OrdinalIgnoreCase)
                        || n.NarudzbaId.ToString() == t.TrimStart('0')
                        || (emailovi.TryGetValue(n.KorisnikId, out var em) && em.Contains(t, StringComparison.OrdinalIgnoreCase))
                        || (n.Pozivnica.ImenaDomacina ?? "").Contains(t, StringComparison.OrdinalIgnoreCase)
                        || n.Pozivnica.Naslov.Contains(t, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            (vm.Narudzbe, vm.Str) = Stranicenje.Primijeni(lista, str);
            return View(vm);
        }

        [HttpGet("narudzbe/{id:int}")]
        public async Task<IActionResult> NarudzbaDetalji(int id)
        {
            var n = await _context.Narudzbe
                .AsNoTracking()
                .Include(x => x.Paket)
                .Include(x => x.Pozivnica)
                .FirstOrDefaultAsync(x => x.NarudzbaId == id);

            if (n == null) return NotFound();

            var emailovi = await EmailoviAsync();
            ViewBag.Email = emailovi.TryGetValue(n.KorisnikId, out var em) ? em : "(nepoznat korisnik)";

            return View(n);
        }

        [HttpPost("narudzbe/{id:int}/potvrdi")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PotvrdiUplatu(int id, string? napomena)
        {
            var n = await _context.Narudzbe.FirstOrDefaultAsync(x => x.NarudzbaId == id);
            if (n == null) return NotFound();

            if (n.Status != NarudzbaStatus.Placeno)
            {
                n.Status = NarudzbaStatus.Placeno;
                n.DatumPlacanja = DateTime.UtcNow;
                n.PotvrdioKorisnik = User.Identity?.Name;
                if (!string.IsNullOrWhiteSpace(napomena)) n.Napomena = Skrati(napomena, 500);
                await _context.SaveChangesAsync();
                TempData["Poruka"] = "Uplata je potvrđena. Korisnik sada može objaviti pozivnicu.";
            }

            return RedirectToAction(nameof(NarudzbaDetalji), new { id });
        }

        [HttpPost("narudzbe/{id:int}/otkazi")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OtkaziNarudzbu(int id)
        {
            var n = await _context.Narudzbe.FirstOrDefaultAsync(x => x.NarudzbaId == id);
            if (n == null) return NotFound();

            if (n.Status == NarudzbaStatus.CekaUplatu)
            {
                // Trajno otkazano: korisnik ne moze ponovo platiti ni objaviti (admin moze vratiti narudzbu).
                n.Status = NarudzbaStatus.Ponisteno;

                var poz = await _context.Pozivnices.FirstOrDefaultAsync(x => x.PozivnicaId == n.PozivnicaId);
                if (poz != null && poz.Objavljena
                    && !await _context.Narudzbe.AnyAsync(x => x.PozivnicaId == n.PozivnicaId && x.NarudzbaId != n.NarudzbaId && x.Status == NarudzbaStatus.Placeno))
                    poz.Objavljena = false;

                await _context.SaveChangesAsync();
                TempData["Poruka"] = "Narudžba je otkazana. Korisnik više ne može platiti ni objaviti ovu pozivnicu.";
            }

            return RedirectToAction(nameof(NarudzbaDetalji), new { id });
        }

        // Ispravka greske: potvrdjena uplata vraca se na "Čeka uplatu".
        [HttpPost("narudzbe/{id:int}/vrati")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VratiNaCekanje(int id)
        {
            var n = await _context.Narudzbe.FirstOrDefaultAsync(x => x.NarudzbaId == id);
            if (n == null) return NotFound();

            if (n.Status != NarudzbaStatus.CekaUplatu)
            {
                n.Status = NarudzbaStatus.CekaUplatu;
                n.DatumPlacanja = null;
                n.PotvrdioKorisnik = null;
                await _context.SaveChangesAsync();
                TempData["Poruka"] = "Narudžba je vraćena na „Čeka uplatu“.";
            }

            return RedirectToAction(nameof(NarudzbaDetalji), new { id });
        }

        [HttpPost("narudzbe/{id:int}/napomena")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Napomena(int id, string? napomena)
        {
            var n = await _context.Narudzbe.FirstOrDefaultAsync(x => x.NarudzbaId == id);
            if (n == null) return NotFound();

            n.Napomena = string.IsNullOrWhiteSpace(napomena) ? null : Skrati(napomena, 500);
            await _context.SaveChangesAsync();
            TempData["Poruka"] = "Napomena je sačuvana.";

            return RedirectToAction(nameof(NarudzbaDetalji), new { id });
        }

        [HttpPost("narudzbe/{id:int}/obrisi")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ObrisiNarudzbu(int id)
        {
            var n = await _context.Narudzbe.FirstOrDefaultAsync(x => x.NarudzbaId == id);
            if (n == null) return NotFound();

            // Brisu se samo otkazane narudzbe; placene ostaju zbog evidencije.
            if (n.Status != NarudzbaStatus.Otkazano)
            {
                TempData["Poruka"] = n.Status == NarudzbaStatus.Ponisteno
                    ? "Narudžba koju je otkazao admin ne može se obrisati (čuva blokadu). Prvo je vratite na „Čeka uplatu“."
                    : "Obrisati se mogu samo otkazane narudžbe.";
                return RedirectToAction(nameof(NarudzbaDetalji), new { id });
            }

            _context.Narudzbe.Remove(n);
            await _context.SaveChangesAsync();
            TempData["Poruka"] = "Narudžba je obrisana.";

            return RedirectToAction(nameof(Narudzbe));
        }

        // ------------------------------ Pozivnice ------------------------------

        [HttpGet("pozivnice")]
        public async Task<IActionResult> PozivniceLista(string? q, int str = 1)
        {
            var emailovi = await EmailoviAsync();

            var pozivnice = await _context.Pozivnices
                .AsNoTracking()
                .Include(p => p.Paket)
                .Include(p => p.Sablon)
                .OrderByDescending(p => p.PozivnicaId)
                .ToListAsync();

            var narudzbe = await _context.Narudzbe
                .AsNoTracking()
                .Select(n => new { n.PozivnicaId, n.Status, n.NarudzbaId })
                .ToListAsync();

            var redovi = pozivnice.Select(p => new AdminPozivnicaRed
            {
                Pozivnica = p,
                Vlasnik = emailovi.TryGetValue(p.KorisnikId, out var em) ? em : "(nepoznat)",
                Placena = narudzbe.Any(n => n.PozivnicaId == p.PozivnicaId && n.Status == NarudzbaStatus.Placeno),
                OtvorenaNarudzba = narudzbe
                    .Where(n => n.PozivnicaId == p.PozivnicaId && n.Status == NarudzbaStatus.CekaUplatu)
                    .Select(n => n.NarudzbaId.ToString("D6"))
                    .FirstOrDefault()
            }).ToList();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var t = q.Trim();
                redovi = redovi.Where(r =>
                        r.Vlasnik.Contains(t, StringComparison.OrdinalIgnoreCase)
                        || r.Pozivnica.Naslov.Contains(t, StringComparison.OrdinalIgnoreCase)
                        || (r.Pozivnica.ImenaDomacina ?? "").Contains(t, StringComparison.OrdinalIgnoreCase)
                        || r.Pozivnica.AdresaPozivnice.Contains(t, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            var (stavke, strana) = Stranicenje.Primijeni(redovi, str);
            return View(new AdminPozivniceViewModel { Redovi = stavke, Q = q, Str = strana });
        }

        [HttpPost("pozivnice/{id:int}/objava")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PreklopiObjavu(int id)
        {
            var p = await _context.Pozivnices.FirstOrDefaultAsync(x => x.PozivnicaId == id);
            if (p == null) return NotFound();

            p.Objavljena = !p.Objavljena;
            await _context.SaveChangesAsync();
            TempData["Poruka"] = p.Objavljena ? "Pozivnica je objavljena." : "Pozivnica je skinuta sa objave.";

            return RedirectToAction(nameof(PozivniceLista));
        }

        // Admin ručno evidentira uplatu (npr. gotovina ili dogovor) i otključava objavu.
        [HttpPost("pozivnice/{id:int}/rucna-uplata")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RucnaUplata(int id, string? napomena)
        {
            var p = await _context.Pozivnices
                .Include(x => x.Paket)
                .FirstOrDefaultAsync(x => x.PozivnicaId == id);

            if (p == null) return NotFound();

            var vecPlacena = await _context.Narudzbe.AnyAsync(n =>
                n.PozivnicaId == id && n.Status == NarudzbaStatus.Placeno);

            if (vecPlacena)
            {
                TempData["Poruka"] = "Za ovu pozivnicu već postoji plaćena narudžba.";
                return RedirectToAction(nameof(PozivniceLista));
            }

            var otvorena = await _context.Narudzbe
                .Where(n => n.PozivnicaId == id && n.Status == NarudzbaStatus.CekaUplatu)
                .OrderByDescending(n => n.NarudzbaId)
                .FirstOrDefaultAsync();

            var vrijeme = DateTime.UtcNow;

            if (otvorena != null)
            {
                otvorena.Status = NarudzbaStatus.Placeno;
                otvorena.DatumPlacanja = vrijeme;
                otvorena.PotvrdioKorisnik = User.Identity?.Name;
                if (!string.IsNullOrWhiteSpace(napomena)) otvorena.Napomena = Skrati(napomena, 500);
            }
            else
            {
                _context.Narudzbe.Add(new Narudzba
                {
                    KorisnikId = p.KorisnikId,
                    PozivnicaId = p.PozivnicaId,
                    PaketId = p.PaketId,
                    Iznos = p.Paket.Cena,
                    Valuta = "BAM",
                    NacinPlacanja = "rucno",
                    Status = NarudzbaStatus.Placeno,
                    DatumKreiranja = vrijeme,
                    DatumPlacanja = vrijeme,
                    PotvrdioKorisnik = User.Identity?.Name,
                    Napomena = string.IsNullOrWhiteSpace(napomena) ? "Ručno evidentirana uplata" : Skrati(napomena, 500)
                });
            }

            await _context.SaveChangesAsync();
            TempData["Poruka"] = "Uplata je evidentirana. Korisnik može objaviti pozivnicu.";

            return RedirectToAction(nameof(PozivniceLista));
        }

        // ------------------------------ Paketi ------------------------------

        [HttpGet("paketi")]
        public async Task<IActionResult> PaketiLista()
        {
            var lista = await _context.Paketis
                .AsNoTracking()
                .OrderBy(p => p.Cena)
                .ToListAsync();

            return View(lista);
        }

        [HttpGet("paketi/{id:int}")]
        public async Task<IActionResult> UrediPaket(int id)
        {
            var p = await _context.Paketis.AsNoTracking().FirstOrDefaultAsync(x => x.PaketId == id);
            if (p == null) return NotFound();

            return View(new AdminPaketForma
            {
                PaketId = p.PaketId,
                Oznaka = p.Oznaka,
                Naziv = p.Naziv,
                Opis = p.Opis,
                CenaUnos = p.Cena.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture),
                Aktivan = p.Aktivan
            });
        }

        [HttpPost("paketi/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UrediPaket(int id, AdminPaketForma forma)
        {
            var p = await _context.Paketis.FirstOrDefaultAsync(x => x.PaketId == id);
            if (p == null) return NotFound();

            forma.PaketId = id;
            forma.Oznaka = p.Oznaka;

            if (string.IsNullOrWhiteSpace(forma.Naziv))
                ModelState.AddModelError(nameof(forma.Naziv), "Naziv je obavezan.");

            var cijenaTekst = (forma.CenaUnos ?? "").Trim().Replace(',', '.');
            if (!decimal.TryParse(cijenaTekst, System.Globalization.NumberStyles.AllowDecimalPoint,
                    System.Globalization.CultureInfo.InvariantCulture, out var cijena)
                || cijena < 0 || cijena > 100000)
            {
                ModelState.AddModelError(nameof(forma.CenaUnos), "Unesite ispravnu cijenu, npr. 35 ili 35.50.");
            }

            if (!ModelState.IsValid) return View(forma);

            p.Naziv = forma.Naziv.Trim();
            p.Opis = string.IsNullOrWhiteSpace(forma.Opis) ? null : forma.Opis.Trim();
            p.Cena = Math.Round(cijena, 2);
            p.Aktivan = forma.Aktivan;

            await _context.SaveChangesAsync();
            TempData["Poruka"] = "Paket je sačuvan.";

            return RedirectToAction(nameof(PaketiLista));
        }

        // ------------------------------ Korisnici ------------------------------

        [HttpGet("korisnici")]
        public async Task<IActionResult> Korisnici(string? q, int str = 1)
        {
            var korisnici = await _users.Users.AsNoTracking().OrderBy(u => u.Email).ToListAsync();

            var brojPozivnica = await _context.Pozivnices
                .GroupBy(p => p.KorisnikId)
                .Select(g => new { Id = g.Key, N = g.Count() })
                .ToDictionaryAsync(x => x.Id, x => x.N);

            var brojNarudzbi = await _context.Narudzbe
                .GroupBy(n => n.KorisnikId)
                .Select(g => new { Id = g.Key, N = g.Count() })
                .ToDictionaryAsync(x => x.Id, x => x.N);

            var redovi = korisnici.Select(u => new AdminKorisnikRed
            {
                Id = u.Id,
                Email = u.Email ?? u.UserName ?? "",
                ImePrezime = u.ImePrezime,
                Pozivnica = brojPozivnica.TryGetValue(u.Id, out var a) ? a : 0,
                Narudzbi = brojNarudzbi.TryGetValue(u.Id, out var b) ? b : 0,
                Zakljucan = u.LockoutEnd != null && u.LockoutEnd > DateTimeOffset.UtcNow,
                JeAdmin = _admin.JeAdminEmail(u.Email ?? u.UserName)
            }).ToList();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var t = q.Trim();
                redovi = redovi.Where(r =>
                        r.Email.Contains(t, StringComparison.OrdinalIgnoreCase)
                        || (r.ImePrezime ?? "").Contains(t, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            var (stavke, strana) = Stranicenje.Primijeni(redovi, str);
            ViewBag.Q = q;
            ViewBag.Str = strana;
            return View(stavke);
        }

        [HttpPost("korisnici/{id}/zakljucaj")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Zakljucaj(string id)
        {
            var u = await _users.FindByIdAsync(id);
            if (u == null) return NotFound();

            if (_admin.JeAdminEmail(u.Email ?? u.UserName))
            {
                TempData["Poruka"] = "Admin nalog se ne može zaključati.";
                return RedirectToAction(nameof(Korisnici));
            }

            await _users.SetLockoutEnabledAsync(u, true);
            await _users.SetLockoutEndDateAsync(u, DateTimeOffset.UtcNow.AddYears(100));
            TempData["Poruka"] = "Nalog je zaključan; korisnik se više ne može prijaviti.";

            return RedirectToAction(nameof(Korisnici));
        }

        [HttpPost("korisnici/{id}/otkljucaj")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Otkljucaj(string id)
        {
            var u = await _users.FindByIdAsync(id);
            if (u == null) return NotFound();

            await _users.SetLockoutEndDateAsync(u, null);
            TempData["Poruka"] = "Nalog je otključan.";

            return RedirectToAction(nameof(Korisnici));
        }

        private static string Skrati(string s, int max)
        {
            s = s.Trim();
            return s.Length <= max ? s : s.Substring(0, max);
        }
    }
}
