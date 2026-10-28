using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DigitalnePozivnice.Data.Generated;
using DigitalnePozivnice.Helpers;
using DigitalnePozivnice.Models.Generated;
using DigitalnePozivnice.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitalnePozivnice.Controllers
{
    /// <summary>Korisnik: placanje pozivnice (uplatnica/virman) i pregled svojih narudzbi.</summary>
    [Authorize]
    public class NarudzbeController : Controller
    {
        private readonly DigitalnePozivniceScaffoldContext _context;

        public NarudzbeController(DigitalnePozivniceScaffoldContext context)
        {
            _context = context;
        }

        private string? KorisnikId => User.FindFirstValue(ClaimTypes.NameIdentifier);

        private Task<Pozivnice?> MojaPozivnicaAsync(int pozivnicaId)
        {
            var id = KorisnikId;
            return _context.Pozivnices
                .Include(p => p.Paket)
                .FirstOrDefaultAsync(p => p.PozivnicaId == pozivnicaId && p.KorisnikId == id);
        }

        // Vraca preusmjerenje ako placanje nije potrebno ili vec postoji otvorena narudzba, inace null.
        private async Task<IActionResult?> PreskociAkoTrebaAsync(Pozivnice p)
        {
            if (!p.Objavljena && await PlacanjeHelper.JeBlokiranaAsync(_context, p.PozivnicaId))
            {
                return RedirectToAction("Details", "Pozivnice", new { id = p.PozivnicaId });
            }

            if (p.Objavljena || await PlacanjeHelper.MozeObjavitiAsync(_context, p.PozivnicaId, p.Paket.Cena))
                return RedirectToAction("Details", "Pozivnice", new { id = p.PozivnicaId });

            var otvorena = await _context.Narudzbe
                .Where(n => n.PozivnicaId == p.PozivnicaId && n.Status == NarudzbaStatus.CekaUplatu)
                .OrderByDescending(n => n.NarudzbaId)
                .Select(n => (int?)n.NarudzbaId)
                .FirstOrDefaultAsync();

            if (otvorena != null)
                return RedirectToAction(nameof(Detalji), new { id = otvorena.Value });

            return null;
        }

        [HttpGet("/placanje/{pozivnicaId:int}")]
        public async Task<IActionResult> Placanje(int pozivnicaId)
        {
            var p = await MojaPozivnicaAsync(pozivnicaId);
            if (p == null) return NotFound();

            var preskoci = await PreskociAkoTrebaAsync(p);
            if (preskoci != null) return preskoci;

            return View(new PlacanjeViewModel
            {
                Pozivnica = p,
                Paket = p.Paket,
                Iznos = p.Paket.Cena,
                Eur = Cijene.Eur(p.Paket.Cena)
            });
        }

        [HttpPost("/placanje/{pozivnicaId:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Kreiraj(int pozivnicaId)
        {
            var p = await MojaPozivnicaAsync(pozivnicaId);
            if (p == null) return NotFound();

            var preskoci = await PreskociAkoTrebaAsync(p);
            if (preskoci != null) return preskoci;

            var narudzba = new Narudzba
            {
                KorisnikId = KorisnikId!,
                PozivnicaId = p.PozivnicaId,
                PaketId = p.PaketId,
                Iznos = p.Paket.Cena,
                Valuta = "BAM",
                NacinPlacanja = "uplatnica",
                Status = NarudzbaStatus.CekaUplatu,
                DatumKreiranja = DateTime.UtcNow
            };

            _context.Narudzbe.Add(narudzba);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Detalji), new { id = narudzba.NarudzbaId });
        }

        [HttpGet("/narudzba/{id:int}")]
        public async Task<IActionResult> Detalji(int id)
        {
            var korisnikId = KorisnikId;

            var n = await _context.Narudzbe
                .AsNoTracking()
                .Include(x => x.Paket)
                .Include(x => x.Pozivnica)
                .FirstOrDefaultAsync(x => x.NarudzbaId == id && x.KorisnikId == korisnikId);

            if (n == null) return NotFound();

            return View(n);
        }

        [HttpPost("/narudzba/{id:int}/otkazi")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Otkazi(int id)
        {
            var korisnikId = KorisnikId;

            var n = await _context.Narudzbe
                .FirstOrDefaultAsync(x => x.NarudzbaId == id && x.KorisnikId == korisnikId);

            if (n == null) return NotFound();

            if (n.Status == NarudzbaStatus.CekaUplatu)
            {
                n.Status = NarudzbaStatus.Otkazano;
                await _context.SaveChangesAsync();
                TempData["Poruka"] = "Narudžba je otkazana.";
            }

            return RedirectToAction("Details", "Pozivnice", new { id = n.PozivnicaId });
        }

        [HttpGet("/moje-narudzbe")]
        public async Task<IActionResult> Moje()
        {
            var korisnikId = KorisnikId;

            var lista = await _context.Narudzbe
                .AsNoTracking()
                .Include(x => x.Paket)
                .Include(x => x.Pozivnica)
                .Where(x => x.KorisnikId == korisnikId)
                .OrderByDescending(x => x.NarudzbaId)
                .ToListAsync();

            return View(lista);
        }
    }
}
