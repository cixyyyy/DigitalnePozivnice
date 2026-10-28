using System.Security.Claims;
using DigitalnePozivnice.Data.Generated;
using DigitalnePozivnice.Helpers;
using DigitalnePozivnice.Models.Generated;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitalnePozivnice.Controllers;

/// <summary>
/// Fotografije i muzika pozivnice (Premium / Exclusive).
/// Fajlovi se čuvaju u App_Data/uploads/p{id}/ (van wwwroot) i služe se kroz ovaj kontroler.
/// </summary>
public class MedijiController : Controller
{
    private const int MaxFoto = 8;
    private const long MaxFotoBajtova = 6L * 1024 * 1024;
    private const long MaxMuzikaBajtova = 10L * 1024 * 1024;

    private readonly DigitalnePozivniceScaffoldContext _db;
    private readonly IWebHostEnvironment _env;

    public MedijiController(DigitalnePozivniceScaffoldContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    private string Folder(int pozivnicaId)
        => Path.Combine(_env.ContentRootPath, "App_Data", "uploads", "p" + pozivnicaId);

    private Task<Pozivnice?> Moja(int id, bool saMedijima = true)
    {
        var korisnikId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        IQueryable<Pozivnice> q = _db.Pozivnices.Include(p => p.Paket);
        if (saMedijima) q = q.Include(p => p.MedijiPozivnices);
        return q.FirstOrDefaultAsync(p => p.PozivnicaId == id && p.KorisnikId == korisnikId);
    }

    // ------------------------------- STRANICA -------------------------------

    [Authorize]
    [HttpGet("/moje-pozivnice/{id:int}/mediji")]
    public async Task<IActionResult> Index(int id)
    {
        var p = await Moja(id);
        if (p == null) return NotFound();
        if (!InvitationTheme.ImaRsvp(p.Paket?.Oznaka))
            return RedirectToAction("Index", "Paketi");
        return View(p);
    }

    // ------------------------------- FOTOGRAFIJE -------------------------------

    [Authorize]
    [HttpPost("/moje-pozivnice/{id:int}/mediji/foto")]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(12_000_000)]
    [RequestFormLimits(MultipartBodyLengthLimit = 12_000_000)]
    public async Task<IActionResult> OtpremiFoto(int id, IFormFile? fajl)
    {
        var p = await Moja(id);
        if (p == null) return NotFound();
        if (!InvitationTheme.ImaRsvp(p.Paket?.Oznaka))
            return Greska("Fotografije su dostupne u paketima Premium i Exclusive.");

        var foto = p.MedijiPozivnices.Where(m => m.Opis == MuzikaBiblioteka.OpisFoto).ToList();
        if (foto.Count >= MaxFoto)
            return Greska("Možete dodati najviše 8 fotografija.");

        if (fajl == null || fajl.Length == 0) return Greska("Fajl nije odabran.");
        if (fajl.Length > MaxFotoBajtova) return Greska("Fotografija je prevelika (najviše 6 MB).");

        var ext = await PrepoznajSliku(fajl);
        if (ext == null) return Greska("Podržani formati: JPG, PNG i WEBP.");

        var ime = Guid.NewGuid().ToString("N") + ext;
        await Sacuvaj(fajl, p.PozivnicaId, ime);

        var m = new MedijiPozivnice
        {
            PozivnicaId = p.PozivnicaId,
            PutanjaFajla = ime,
            Opis = MuzikaBiblioteka.OpisFoto,
            Redosled = foto.Count == 0 ? 1 : foto.Max(x => x.Redosled) + 1
        };
        _db.MedijiPozivnices.Add(m);
        await _db.SaveChangesAsync();

        return Json(new { ok = true, id = m.MedijId, url = MuzikaBiblioteka.MedijUrl(m) });
    }

    [Authorize]
    [HttpPost("/moje-pozivnice/{id:int}/mediji/foto/{medijId:int}/obrisi")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ObrisiFoto(int id, int medijId)
    {
        var p = await Moja(id);
        if (p == null) return NotFound();

        var m = p.MedijiPozivnices.FirstOrDefault(x => x.MedijId == medijId && x.Opis == MuzikaBiblioteka.OpisFoto);
        if (m == null) return Greska("Fotografija nije pronađena.");

        _db.MedijiPozivnices.Remove(m);
        await _db.SaveChangesAsync();
        ObrisiFajl(p.PozivnicaId, m.PutanjaFajla);
        return Json(new { ok = true });
    }

    // ------------------------------- MUZIKA -------------------------------

    /// <summary>izbor: default, none, moja ili lib:{kljuc}</summary>
    [Authorize]
    [HttpPost("/moje-pozivnice/{id:int}/mediji/muzika")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> IzaberiMuziku(int id, string izbor)
    {
        var p = await Moja(id);
        if (p == null) return NotFound();
        if (!InvitationTheme.ImaRsvp(p.Paket?.Oznaka))
            return Greska("Izbor muzike je dostupan u paketima Premium i Exclusive.");

        izbor = (izbor ?? "").Trim();
        string? nova;
        if (izbor == "default") nova = null;
        else if (izbor == "none") nova = "none";
        else if (izbor == "moja")
        {
            if (!p.MedijiPozivnices.Any(m => m.Opis == MuzikaBiblioteka.OpisMuzika))
                return Greska("Prvo otpremite svoju pjesmu.");
            nova = "moja";
        }
        else if (izbor.StartsWith("lib:") && MuzikaBiblioteka.Nadji(izbor.Substring(4)) != null)
            nova = izbor;
        else return Greska("Nepoznat izbor muzike.");

        p.Muzika = nova;
        p.DatumIzmene = DateTime.Now;
        await _db.SaveChangesAsync();
        return Json(new { ok = true });
    }

    [Authorize]
    [HttpPost("/moje-pozivnice/{id:int}/mediji/muzika/otpremi")]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(14_000_000)]
    [RequestFormLimits(MultipartBodyLengthLimit = 14_000_000)]
    public async Task<IActionResult> OtpremiMuziku(int id, IFormFile? fajl, bool potvrdaPrava = false)
    {
        var p = await Moja(id);
        if (p == null) return NotFound();
        if (!InvitationTheme.ImaRsvp(p.Paket?.Oznaka))
            return Greska("Otpremanje muzike je dostupno u paketima Premium i Exclusive.");

        if (!potvrdaPrava)
            return Greska("Potvrdite da imate pravo korištenja ove pjesme.");
        if (fajl == null || fajl.Length == 0) return Greska("Fajl nije odabran.");
        if (fajl.Length > MaxMuzikaBajtova) return Greska("Pjesma je prevelika (najviše 10 MB).");
        if (!await JeMp3(fajl)) return Greska("Podržan je samo MP3 format.");

        foreach (var staro in p.MedijiPozivnices.Where(m => m.Opis == MuzikaBiblioteka.OpisMuzika).ToList())
        {
            _db.MedijiPozivnices.Remove(staro);
            ObrisiFajl(p.PozivnicaId, staro.PutanjaFajla);
        }

        var ime = Guid.NewGuid().ToString("N") + ".mp3";
        await Sacuvaj(fajl, p.PozivnicaId, ime);

        _db.MedijiPozivnices.Add(new MedijiPozivnice
        {
            PozivnicaId = p.PozivnicaId,
            PutanjaFajla = ime,
            Opis = MuzikaBiblioteka.OpisMuzika,
            Redosled = 0
        });
        p.Muzika = "moja";
        p.DatumIzmene = DateTime.Now;
        await _db.SaveChangesAsync();

        return Json(new { ok = true });
    }

    [Authorize]
    [HttpPost("/moje-pozivnice/{id:int}/mediji/muzika/ukloni")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UkloniMuziku(int id)
    {
        var p = await Moja(id);
        if (p == null) return NotFound();

        foreach (var m in p.MedijiPozivnices.Where(m => m.Opis == MuzikaBiblioteka.OpisMuzika).ToList())
        {
            _db.MedijiPozivnices.Remove(m);
            ObrisiFajl(p.PozivnicaId, m.PutanjaFajla);
        }
        if (p.Muzika == "moja") p.Muzika = null;
        await _db.SaveChangesAsync();
        return Json(new { ok = true });
    }

    // ------------------------------- SLUŽENJE FAJLOVA -------------------------------

    [AllowAnonymous]
    [HttpGet("/mediji/{pozivnicaId:int}/{ime}")]
    public async Task<IActionResult> Fajl(int pozivnicaId, string ime)
    {
        if (!System.Text.RegularExpressions.Regex.IsMatch(ime, @"^[a-f0-9]{32}\.(jpg|png|webp|mp3)$"))
            return NotFound();

        var postoji = await _db.MedijiPozivnices.AsNoTracking()
            .AnyAsync(m => m.PozivnicaId == pozivnicaId && m.PutanjaFajla == ime);
        if (!postoji) return NotFound();

        var putanja = Path.Combine(Folder(pozivnicaId), ime);
        if (!System.IO.File.Exists(putanja)) return NotFound();

        var tip = Path.GetExtension(ime) switch
        {
            ".jpg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => "audio/mpeg"
        };

        Response.Headers["Cache-Control"] = "public, max-age=31536000, immutable";
        Response.Headers["X-Content-Type-Options"] = "nosniff";
        return PhysicalFile(putanja, tip, enableRangeProcessing: true);
    }

    // ------------------------------- POMOĆNO -------------------------------

    private IActionResult Greska(string poruka)
        => BadRequest(new { ok = false, poruka });

    private async Task Sacuvaj(IFormFile fajl, int pozivnicaId, string ime)
    {
        var folder = Folder(pozivnicaId);
        Directory.CreateDirectory(folder);
        await using var fs = new FileStream(Path.Combine(folder, ime), FileMode.CreateNew);
        await fajl.CopyToAsync(fs);
    }

    private void ObrisiFajl(int pozivnicaId, string ime)
    {
        try
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(ime, @"^[a-f0-9]{32}\.(jpg|png|webp|mp3)$")) return;
            var putanja = Path.Combine(Folder(pozivnicaId), ime);
            if (System.IO.File.Exists(putanja)) System.IO.File.Delete(putanja);
        }
        catch { /* nije kritično */ }
    }

    private static async Task<byte[]> Zaglavlje(IFormFile f, int n = 16)
    {
        var b = new byte[n];
        await using var s = f.OpenReadStream();
        var read = 0;
        while (read < n)
        {
            var k = await s.ReadAsync(b.AsMemory(read, n - read));
            if (k == 0) break;
            read += k;
        }
        return b[..read];
    }

    private static async Task<string?> PrepoznajSliku(IFormFile f)
    {
        var b = await Zaglavlje(f, 12);
        if (b.Length >= 3 && b[0] == 0xFF && b[1] == 0xD8 && b[2] == 0xFF) return ".jpg";
        if (b.Length >= 8 && b[0] == 0x89 && b[1] == 0x50 && b[2] == 0x4E && b[3] == 0x47) return ".png";
        if (b.Length >= 12 && b[0] == 'R' && b[1] == 'I' && b[2] == 'F' && b[3] == 'F'
            && b[8] == 'W' && b[9] == 'E' && b[10] == 'B' && b[11] == 'P') return ".webp";
        return null;
    }

    private static async Task<bool> JeMp3(IFormFile f)
    {
        var b = await Zaglavlje(f, 4);
        if (b.Length < 3) return false;
        if (b[0] == 'I' && b[1] == 'D' && b[2] == '3') return true;          // ID3 oznaka
        return b.Length >= 2 && b[0] == 0xFF && (b[1] & 0xE0) == 0xE0;       // MP3 okvir
    }
}
