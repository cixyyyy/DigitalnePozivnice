using DigitalnePozivnice.Localization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalnePozivnice.Controllers
{
    public class JezikController : Controller
    {
        /// <summary>Čuva izabrani jezik sajta u kolačiću i vraća korisnika gdje je bio.</summary>
        [HttpGet("/jezik/{kod}")]
        public IActionResult Postavi(string kod, string? povratak)
        {
            if (Loc.Valid(kod))
            {
                Response.Cookies.Append(Loc.CookieName, Loc.Normalize(kod), new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddYears(1),
                    Path = "/",
                    IsEssential = true,
                    SameSite = SameSiteMode.Lax
                });
            }

            if (!string.IsNullOrEmpty(povratak) && Url.IsLocalUrl(povratak))
                return LocalRedirect(povratak);

            var referer = Request.Headers.Referer.ToString();
            if (Uri.TryCreate(referer, UriKind.Absolute, out var u) &&
                string.Equals(u.Host, Request.Host.Host, StringComparison.OrdinalIgnoreCase))
                return Redirect(u.PathAndQuery);

            return RedirectToAction("Index", "Home");
        }
    }
}
