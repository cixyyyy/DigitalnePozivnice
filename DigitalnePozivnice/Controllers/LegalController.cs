using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalnePozivnice.Controllers;

/// <summary>Pravne stranice: Uslovi korištenja i Politika privatnosti (uključujući kolačiće).</summary>
[AllowAnonymous]
public class LegalController : Controller
{
    [HttpGet("/uslovi")]
    public IActionResult Uslovi() => View();

    [HttpGet("/privatnost")]
    public IActionResult Privatnost() => View();
}
