(function () {
    "use strict";

    var tr = window.tr || function (s) { return s; };

    var root = document.documentElement;
    var reduced = root.classList.contains("reduce-motion");

    // ---------- Kopiranje linka ----------
    document.querySelectorAll("[data-copy]").forEach(function (btn) {
        btn.addEventListener("click", function () {
            var input = document.querySelector(btn.getAttribute("data-copy"));
            var msg = document.getElementById("copyMsg");
            if (!input) return;

            function done(ok) {
                if (msg) msg.textContent = ok ? tr("Link je kopiran.") : tr("Kopirajte ručno (Ctrl+C).");
                var old = btn.textContent;
                if (ok) { btn.textContent = tr("Kopirano ✓"); setTimeout(function () { btn.textContent = old; }, 1800); }
            }

            input.select();
            if (navigator.clipboard && window.isSecureContext) {
                navigator.clipboard.writeText(input.value).then(function () { done(true); }, function () { done(false); });
            } else {
                try { done(document.execCommand("copy")); } catch (e) { done(false); }
            }
        });
    });

    // ---------- Telefon sa pregledom (Details) ----------
    var screen = document.getElementById("pzScreen");
    if (screen) {
        var frame = screen.querySelector("iframe");
        var fit = function () {
            var k = screen.clientWidth / 390;
            frame.style.transform = "scale(" + k + ")";
            screen.style.height = Math.round(800 * k) + "px";
        };
        fit();
        window.addEventListener("resize", fit);
        if ("ResizeObserver" in window) new ResizeObserver(fit).observe(screen);
    }

    // ---------- Brojači ----------
    var counters = document.querySelectorAll("[data-count]");
    function count(el) {
        var target = parseInt(el.getAttribute("data-count"), 10) || 0;
        if (reduced || target === 0) { el.textContent = target; return; }
        var start = null, duration = 1500;
        el.textContent = "0";
        function tick(t) {
            if (start === null) start = t;
            var p = Math.min(1, (t - start) / duration);
            el.textContent = Math.round(target * (p === 1 ? 1 : 1 - Math.pow(2, -10 * p)));
            if (p < 1) requestAnimationFrame(tick);
        }
        requestAnimationFrame(tick);
    }
    if (counters.length && "IntersectionObserver" in window) {
        var io = new IntersectionObserver(function (entries) {
            entries.forEach(function (e) {
                if (e.isIntersecting) { setTimeout(function () { count(e.target); }, 350); io.unobserve(e.target); }
            });
        }, { threshold: 0.6 });
        counters.forEach(function (el) { if (!reduced) el.textContent = "0"; io.observe(el); });
    }

    // ---------- Filter i pretraga gostiju ----------
    var rows = document.querySelectorAll(".pt-row");
    if (rows.length) {
        var tabs = document.querySelectorAll(".pt-tabs button");
        var search = document.getElementById("ptSearch");
        var none = document.getElementById("ptNone");
        var filter = "all";

        function apply() {
            var q = (search.value || "").trim().toLowerCase();
            var visible = 0;
            rows.forEach(function (r) {
                var ok = (filter === "all" || r.getAttribute("data-status") === filter) &&
                         (!q || r.getAttribute("data-name").indexOf(q) !== -1);
                r.hidden = !ok;
                if (ok) visible++;
            });
            if (none) none.hidden = visible > 0;
        }

        tabs.forEach(function (b) {
            b.addEventListener("click", function () {
                tabs.forEach(function (x) { x.classList.remove("is-active"); });
                b.classList.add("is-active");
                filter = b.getAttribute("data-filter");
                apply();
            });
        });
        if (search) search.addEventListener("input", apply);
    }
})();
