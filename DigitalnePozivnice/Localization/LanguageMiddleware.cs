using System.Text;

namespace DigitalnePozivnice.Localization;

/// <summary>
/// Određuje jezik zahtjeva (kolačić "lang", ili ?lang=xx) i prevodi HTML odgovore.
/// Statički fajlovi i ne-HTML odgovori prolaze neizmijenjeni.
/// </summary>
public sealed class LanguageMiddleware
{
    private readonly RequestDelegate _next;
    public LanguageMiddleware(RequestDelegate next) => _next = next;

    private static bool TrebaDetekciju(HttpContext ctx)
    {
        if (!HttpMethods.IsGet(ctx.Request.Method)) return false;

        var path = ctx.Request.Path.Value ?? "";
        // javne pozivnice koriste jezik koji je domacin izabrao
        if (path.StartsWith("/pozivnica/", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/pozivnica-pregled/", StringComparison.OrdinalIgnoreCase))
            return false;

        var last = path.Substring(path.LastIndexOf('/') + 1);
        if (last.Contains('.')) return false; // staticki fajlovi

        var ua = ctx.Request.Headers.UserAgent.ToString();
        if (string.IsNullOrWhiteSpace(ua)) return false;
        if (ua.Contains("bot", StringComparison.OrdinalIgnoreCase) ||
            ua.Contains("crawl", StringComparison.OrdinalIgnoreCase) ||
            ua.Contains("spider", StringComparison.OrdinalIgnoreCase) ||
            ua.Contains("preview", StringComparison.OrdinalIgnoreCase))
            return false;

        return true;
    }

    public async Task InvokeAsync(HttpContext ctx, GeoJezik geo)
    {
        var q = ctx.Request.Query["lang"].ToString();
        if (Loc.Valid(q))
        {
            var code = Loc.Normalize(q);
            ctx.Response.Cookies.Append(Loc.CookieName, code, new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                Path = "/",
                IsEssential = true,
                SameSite = SameSiteMode.Lax
            });
            ctx.Items["lang"] = code;
        }
        else if (Loc.Valid(ctx.Request.Cookies[Loc.CookieName]))
        {
            ctx.Items["lang"] = Loc.Normalize(ctx.Request.Cookies[Loc.CookieName]);
        }
        else
        {
            // Posjetilac jos nije birao jezik: odredi ga prema drzavi (samo za stranice sajta, ne za pozivnice gostiju i botove)
            ctx.Items["lang"] = Loc.Default;
            if (TrebaDetekciju(ctx))
            {
                var code = Loc.Normalize(await geo.OdrediAsync(ctx));
                ctx.Items["lang"] = code;
                ctx.Response.Cookies.Append(Loc.CookieName, code, new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddYears(1),
                    Path = "/",
                    IsEssential = true,
                    SameSite = SameSiteMode.Lax
                });
            }
        }

        var path = ctx.Request.Path.Value ?? "";
        var last = path.Substring(path.LastIndexOf('/') + 1);
        if (last.Contains('.'))
        {
            await _next(ctx); // css, js, slike, fontovi...
            return;
        }

        var original = ctx.Response.Body;
        await using var buffer = new MemoryStream();
        ctx.Response.Body = buffer;
        try
        {
            await _next(ctx);
        }
        finally
        {
            ctx.Response.Body = original;
        }

        var ct = ctx.Response.ContentType;
        buffer.Position = 0;
        if (buffer.Length > 0 && ct != null && ct.StartsWith("text/html", StringComparison.OrdinalIgnoreCase))
        {
            var html = Encoding.UTF8.GetString(buffer.GetBuffer(), 0, (int)buffer.Length);
            var lang = ctx.Items["lang"] as string ?? Loc.Default;
            var bytes = Encoding.UTF8.GetBytes(HtmlTranslator.Translate(html, lang));
            ctx.Response.ContentLength = bytes.Length;
            await original.WriteAsync(bytes);
        }
        else
        {
            await buffer.CopyToAsync(original);
        }
    }
}

public static class LanguageExtensions
{
    public static IApplicationBuilder UseSiteLanguage(this IApplicationBuilder app)
    {
        Loc.Init(app.ApplicationServices.GetRequiredService<IHttpContextAccessor>());
        return app.UseMiddleware<LanguageMiddleware>();
    }
}
