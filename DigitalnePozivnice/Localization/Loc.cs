using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace DigitalnePozivnice.Localization;

/// <summary>
/// Jezici sajta: Srpski, Hrvatski, Bosanski, Slovenščina, Deutsch, English.
/// Tekstovi u pogledima ostaju na izvornom jeziku; HTML odgovor se prevodi u
/// <see cref="HtmlTranslator"/> prema ključu iz <see cref="Strings"/>.
/// </summary>
public static class Loc
{
    public const string Default = "sr";
    public const string CookieName = "lang";

    public static readonly string[] Codes = { "sr", "hr", "bs", "sl", "de", "en" };

    public static readonly (string Code, string Native)[] Jezici =
    {
        ("sr", "Srpski"), ("hr", "Hrvatski"), ("bs", "Bosanski"),
        ("sl", "Slovenščina"), ("de", "Deutsch"), ("en", "English")
    };

    private static IHttpContextAccessor? _accessor;
    public static void Init(IHttpContextAccessor accessor) => _accessor = accessor;

    private static readonly Regex Spaces = new(@"\s+", RegexOptions.Compiled);

    public static string Normalize(string? code)
    {
        var c = (code ?? "").Trim().ToLowerInvariant();
        if (c.Length > 2) c = c.Substring(0, 2);
        return Array.IndexOf(Codes, c) >= 0 ? c : Default;
    }

    public static bool Valid(string? code)
        => Array.IndexOf(Codes, (code ?? "").Trim().ToLowerInvariant()) >= 0;

    /// <summary>Jezik trenutnog zahtjeva (kolačić "lang"; pozivnica ga može privremeno promijeniti).</summary>
    public static string Lang
    {
        get
        {
            var ctx = _accessor?.HttpContext;
            if (ctx == null) return Default;
            if (ctx.Items["lang"] is string s) return s;
            var c = Normalize(ctx.Request.Cookies[CookieName]);
            ctx.Items["lang"] = c;
            return c;
        }
    }

    /// <summary>Za ovaj zahtjev (npr. javna pozivnica) koristi jezik pozivnice.</summary>
    public static void Use(HttpContext ctx, string? code) => ctx.Items["lang"] = Normalize(code);

    public static string HtmlLang => Lang;
    public static string Naziv(string code) => Jezici.First(j => j.Code == Normalize(code)).Native;

    private static int Idx(string lang) => Array.IndexOf(Codes, Normalize(lang));

    /// <summary>Vraća prijevod ili null ako izvorni tekst nije poznat.</summary>
    public static string? Lookup(string source, string lang)
    {
        if (string.IsNullOrWhiteSpace(source)) return null;
        var key = Spaces.Replace(source.Trim(), " ");
        var i = Idx(lang);
        if (Strings.Map.TryGetValue(key, out var arr)) return arr[i];

        foreach (var (pattern, templates) in Strings.Rules)
        {
            var m = Regex.Match(key, pattern);
            if (!m.Success) continue;
            var t = templates[i];
            for (var g = 1; g < m.Groups.Count; g++)
            {
                var v = m.Groups[g].Value;
                // dio koji je poznat izraz (npr. naziv dizajna) se takodje prevodi
                if (Strings.Map.TryGetValue(v, out var inner)) v = inner[i];
                t = t.Replace("{" + g + "}", v);
            }
            return t;
        }
        return null;
    }

    /// <summary>Prevodi tekst iz koda (kontroleri, helperi). Nepoznat tekst vraća nepromijenjen.</summary>
    public static string T(string source, string? lang = null)
        => Lookup(source, lang ?? Lang) ?? source;

    /// <summary>JSON za <c>window.__i18n</c> (JavaScript stringovi + skraćenice mjeseci).</summary>
    public static string JsJson()
    {
        var lang = Lang;
        var t = new Dictionary<string, string>();
        foreach (var k in Strings.JsKeys) t[k] = T(k, lang);
        var obj = new
        {
            lang,
            months = Strings.MonthsAbbr[lang],
            t
        };
        return JsonSerializer.Serialize(obj, new JsonSerializerOptions
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        }).Replace("</", "<\\/");
    }
}
