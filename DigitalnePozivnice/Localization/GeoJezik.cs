using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;

namespace DigitalnePozivnice.Localization;

/// <summary>
/// Odredjuje jezik sajta za posjetioca koji jos nije birao jezik, prema drzavi iz koje posjecuje sajt:
/// Srbija, Republika Srpska, Crna Gora = srpski; Federacija BiH = bosanski; Hrvatska = hrvatski;
/// Slovenija = slovenacki; Njemacka, Svicarska, Austrija = njemacki; ostale drzave = engleski.
/// Drzava se cita iz zaglavlja CDN-a (CF-IPCountry...) ili se trazi po IP adresi (ipwho.is), uz kesiranje.
/// Ako drzava nije poznata (npr. lokalni razvoj), koristi se jezik pretrazivaca.
/// </summary>
public sealed class GeoJezik
{
    private readonly IHttpClientFactory _http;
    private readonly IMemoryCache _cache;
    private readonly ILogger<GeoJezik> _log;

    public GeoJezik(IHttpClientFactory http, IMemoryCache cache, ILogger<GeoJezik> log)
    {
        _http = http;
        _cache = cache;
        _log = log;
    }

    private sealed record Mjesto(string? Drzava, string? Regija);

    /// <summary>Vraca kod jezika (sr, hr, bs, sl, de, en).</summary>
    public async Task<string> OdrediAsync(HttpContext ctx)
    {
        var acceptLang = IzAcceptLanguage(ctx.Request.Headers.AcceptLanguage.ToString());

        var mjesto = await NadjiMjestoAsync(ctx);
        if (!string.IsNullOrEmpty(mjesto.Drzava))
            return IzDrzave(mjesto.Drzava!, mjesto.Regija, acceptLang);

        // Drzava nije poznata (lokalni razvoj, privatna mreza, servis nedostupan): srpski
        return Loc.Default;
    }

    public static string IzDrzave(string drzava, string? regija, string? acceptLang)
    {
        switch (drzava.Trim().ToUpperInvariant())
        {
            case "RS":
            case "ME":
                return "sr";
            case "HR":
                return "hr";
            case "SI":
                return "sl";
            case "DE":
            case "AT":
            case "CH":
            case "LI":
                return "de";
            case "BA":
                var r = (regija ?? "").ToLowerInvariant();
                if (r.Contains("srpska")) return "sr";
                if (r.Contains("federa")) return "bs";
                // Brcko ili nepoznata regija: srpski ako preglednik trazi srpski, inace bosanski
                return acceptLang == "sr" ? "sr" : "bs";
            default:
                return "en";
        }
    }

    private static string? IzAcceptLanguage(string header)
    {
        foreach (var dio in header.Split(','))
        {
            var tag = dio.Split(';')[0].Trim().ToLowerInvariant();
            if (tag.Length < 2) continue;
            var jezik = tag.Substring(0, 2);
            if (Loc.Valid(jezik)) return jezik;
        }
        return null;
    }

    private async Task<Mjesto> NadjiMjestoAsync(HttpContext ctx)
    {
        // 0) Samo za lokalno testiranje (zahtjev sa istog racunara): ?drzava=RS&regija=Republika Srpska
        var probna = ctx.Request.Query["drzava"].ToString();
        if (probna.Length == 2 && ctx.Connection.RemoteIpAddress != null && IPAddress.IsLoopback(ctx.Connection.RemoteIpAddress))
        {
            var pr = ctx.Request.Query["regija"].ToString();
            return new Mjesto(probna, string.IsNullOrWhiteSpace(pr) ? null : pr);
        }

        // 1) CDN / reverse proxy zaglavlja (bez mrezne potrage)
        foreach (var h in new[] { "CF-IPCountry", "CloudFront-Viewer-Country", "X-Vercel-IP-Country", "X-Country-Code" })
        {
            var v = ctx.Request.Headers[h].ToString();
            if (v.Length == 2 && !v.Equals("XX", StringComparison.OrdinalIgnoreCase) && !v.Equals("T1", StringComparison.OrdinalIgnoreCase))
            {
                var regija = ctx.Request.Headers["X-Region"].ToString();
                return new Mjesto(v, string.IsNullOrWhiteSpace(regija) ? null : regija);
            }
        }

        // 2) Potraga po IP adresi
        var ip = KlijentIp(ctx);
        if (ip == null || IsPrivatna(ip)) return new Mjesto(null, null);

        var kljuc = "geo:" + ip;
        if (_cache.TryGetValue(kljuc, out Mjesto? kesirano) && kesirano != null) return kesirano;

        Mjesto rezultat;
        try
        {
            using var klijent = _http.CreateClient("geo");
            klijent.Timeout = TimeSpan.FromSeconds(2);
            using var odgovor = await klijent.GetAsync($"https://ipwho.is/{ip}?fields=success,country_code,region");
            if (!odgovor.IsSuccessStatusCode) throw new HttpRequestException("HTTP " + (int)odgovor.StatusCode);

            using var doc = JsonDocument.Parse(await odgovor.Content.ReadAsStringAsync());
            var root = doc.RootElement;
            var uspjeh = root.TryGetProperty("success", out var su) && su.ValueKind == JsonValueKind.True;
            var drzava = uspjeh && root.TryGetProperty("country_code", out var cc) ? cc.GetString() : null;
            var reg = uspjeh && root.TryGetProperty("region", out var rg) ? rg.GetString() : null;
            rezultat = new Mjesto(drzava, reg);
        }
        catch (Exception ex)
        {
            _log.LogDebug(ex, "Geolokacija po IP adresi nije uspjela.");
            // kratko kesiranje neuspjeha, da ne usporava svaki zahtjev
            _cache.Set(kljuc, new Mjesto(null, null), TimeSpan.FromMinutes(10));
            return new Mjesto(null, null);
        }

        _cache.Set(kljuc, rezultat, TimeSpan.FromHours(24));
        return rezultat;
    }

    private static IPAddress? KlijentIp(HttpContext ctx)
    {
        var f = ctx.Request.Headers["CF-Connecting-IP"].ToString();
        if (string.IsNullOrWhiteSpace(f)) f = ctx.Request.Headers["X-Forwarded-For"].ToString().Split(',')[0].Trim();
        if (!string.IsNullOrWhiteSpace(f) && IPAddress.TryParse(f, out var iz)) return iz;
        return ctx.Connection.RemoteIpAddress;
    }

    private static bool IsPrivatna(IPAddress ip)
    {
        if (ip.IsIPv4MappedToIPv6) ip = ip.MapToIPv4();
        if (IPAddress.IsLoopback(ip)) return true;

        var b = ip.GetAddressBytes();
        if (b.Length == 4)
        {
            return b[0] == 10
                || (b[0] == 172 && b[1] >= 16 && b[1] <= 31)
                || (b[0] == 192 && b[1] == 168)
                || (b[0] == 169 && b[1] == 254)
                || (b[0] == 100 && b[1] >= 64 && b[1] <= 127);
        }

        return ip.IsIPv6LinkLocal || ip.IsIPv6SiteLocal || (b.Length == 16 && (b[0] & 0xFE) == 0xFC);
    }
}
