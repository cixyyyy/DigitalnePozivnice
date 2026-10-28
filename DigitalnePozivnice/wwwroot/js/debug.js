/* Dijagnostika animacija. Aktivira se SAMO kada u adresi dodaš ?debug=1
   npr. https://localhost:7000/?debug=1                                      */
(function () {
    "use strict";
    if (!/[?&]debug=1/.test(location.search)) return;

    function build() {
        var de = document.documentElement;
        var host = document.querySelector(".inv") || de;
        var ease = getComputedStyle(host).getPropertyValue("--ease").trim();
        var os = window.matchMedia && matchMedia("(prefers-reduced-motion: reduce)").matches;

        var sheets = Array.prototype.map.call(document.styleSheets, function (s) {
            return (s.href || "").split("/").pop().split("?")[0];
        }).filter(Boolean);
        var scripts = Array.prototype.map.call(document.scripts, function (s) {
            return (s.src || "").split("/").pop().split("?")[0];
        }).filter(Boolean);

        var rv = document.querySelectorAll("[data-rv], .inv-reveal");
        var vis = document.querySelectorAll("[data-rv].is-visible, .inv-reveal.is-in");

        var rows = [
            ["JavaScript", "radi"],
            ["Klase na <html>", de.className || "(nema)"],
            ["Windows/OS 'smanji pokrete'", os ? "UKLJUČENO" : "isključeno"],
            ["Animacije", de.classList.contains("reduce-motion")
                ? "UGAŠENE zbog OS podešavanja (dodaj ?motion=1)"
                : (de.classList.contains("motion-forced") ? "forsirane (ignorišu OS)" : "uključene")],
            ["Novi CSS (--ease)", ease ? "učitan" : "NIJE učitan"],
            ["CSS fajlovi", sheets.filter(function (n) { return /css$/.test(n); }).join(", ")],
            ["JS fajlovi", scripts.join(", ")],
            ["Elemenata za pojavljivanje", rv.length + " (vidljivih: " + vis.length + ")"],
            ["Plutajućih ikonica", document.querySelectorAll(".fl").length],
            ["Rotirajuća riječ", document.querySelector("[data-rot]") ? "postoji" : "nema"]
        ];

        var box = document.createElement("div");
        box.style.cssText = "position:fixed;left:12px;bottom:12px;z-index:99999;max-width:min(440px,92vw);" +
            "padding:14px 16px;border-radius:14px;background:rgba(20,20,20,.94);color:#fff;" +
            "font:12px/1.5 Consolas,monospace;box-shadow:0 20px 50px rgba(0,0,0,.5)";
        box.innerHTML = "<b style='font-size:13px'>Dijagnostika animacija</b><br>" +
            rows.map(function (r) { return "<div><span style='opacity:.65'>" + r[0] + ":</span> " + r[1] + "</div>"; }).join("") +
            "<div style='margin-top:10px;display:flex;gap:8px'>" +
            "<a href='?motion=1&debug=1' style='color:#111;background:#e6c58d;padding:5px 10px;border-radius:8px;text-decoration:none'>Forsiraj animacije</a>" +
            "<a href='?motion=0&debug=1' style='color:#fff;border:1px solid #888;padding:5px 10px;border-radius:8px;text-decoration:none'>Vrati OS podešavanje</a></div>";
        document.body.appendChild(box);
    }

    if (document.readyState === "complete") setTimeout(build, 600);
    else window.addEventListener("load", function () { setTimeout(build, 600); });
})();
