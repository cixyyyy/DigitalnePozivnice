namespace DigitalnePozivnice.Helpers;

/// <summary>
/// Pronalazi ilustracije dizajna u wwwroot/templates/{kljuc}/.
/// Ako fajl ne postoji, dizajn se prikazuje bez njega (nema greške).
/// </summary>
public static class TemplateAssets
{
    public sealed record Asset(string Url, bool Mono);

    // Imena fajlova (bez ekstenzije) koja dizajn prepoznaje
    private static readonly string[] ImageSlots =
        { "corner", "garland-top", "garland-bottom", "divider", "paper", "hero", "petal", "seal" };

    // Redosled: webp ima prednost, a svg je rezervna varijanta
    private static readonly string[] Extensions = { ".webp", ".png", ".jpg", ".svg" };

    public static Dictionary<string, Asset> Resolve(IWebHostEnvironment env, string templateKey)
    {
        var result = new Dictionary<string, Asset>();

        // templateKey dolazi iz InvitationTheme.NormalizeKey (bela lista), pa je bezbedan
        var folder = Path.Combine(env.WebRootPath ?? "", "templates", templateKey);
        if (!Directory.Exists(folder)) return result;

        foreach (var slot in ImageSlots)
        {
            foreach (var ext in Extensions)
            {
                var path = Path.Combine(folder, slot + ext);
                if (!File.Exists(path)) continue;

                var version = File.GetLastWriteTimeUtc(path).Ticks;
                // .svg se boji bojama pozivnice (mono), ostalo se prikazuje u originalu
                result[slot] = new Asset($"/templates/{templateKey}/{slot}{ext}?v={version}", ext == ".svg");
                break;
            }
        }

        var music = Path.Combine(folder, "music.mp3");
        if (File.Exists(music))
        {
            var v = File.GetLastWriteTimeUtc(music).Ticks;
            result["music"] = new Asset($"/templates/{templateKey}/music.mp3?v={v}", false);
        }

        return result;
    }
}
