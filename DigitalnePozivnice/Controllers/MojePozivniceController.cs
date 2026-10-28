using DigitalnePozivnice.Data.Generated;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;

namespace DigitalnePozivnice.Controllers
{
    [Authorize]
    public class MojePozivniceController : Controller
    {
        private readonly DigitalnePozivniceScaffoldContext _context;

        public MojePozivniceController(DigitalnePozivniceScaffoldContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var korisnikId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var pozivnice = await _context.Pozivnices
                .AsNoTracking()
                .Include(p => p.Paket)
                .Include(p => p.Sablon)
                .Include(p => p.DogadjajiPozivnices)
                .Where(p => p.KorisnikId == korisnikId)
                .OrderByDescending(p => p.DatumKreiranja)
                .ToListAsync();

            // Pozivnice cije je narudzbe otkazao admin (i nema placene narudzbe)
            var ids = pozivnice.Select(x => x.PozivnicaId).ToList();
            var statusi = await _context.Narudzbe
                .AsNoTracking()
                .Where(n => ids.Contains(n.PozivnicaId))
                .Select(n => new { n.PozivnicaId, n.Status })
                .ToListAsync();

            ViewBag.Otkazane = statusi
                .GroupBy(n => n.PozivnicaId)
                .Where(g => g.Any(n => n.Status == Models.Generated.NarudzbaStatus.Ponisteno)
                         && !g.Any(n => n.Status == Models.Generated.NarudzbaStatus.Placeno))
                .Select(g => g.Key)
                .ToHashSet();

            return View(pozivnice);
        }
    }
}
