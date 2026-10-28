
using DigitalnePozivnice.Data.Generated;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitalnePozivnice.Controllers
{
    public class PaketiController : Controller
    {
        private readonly DigitalnePozivniceScaffoldContext _context;

        public PaketiController(
            DigitalnePozivniceScaffoldContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var paketi = await _context.Paketis
                .AsNoTracking()
                .Where(p => p.Aktivan)
                .OrderBy(p => p.Cena)
                .ToListAsync();

            return View(paketi);
        }
    }
}