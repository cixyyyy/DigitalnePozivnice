(function () {
    "use strict";

    var root = document.documentElement;
    var inv = document.querySelector(".inv");
    if (!inv) return;

    var reduced = document.documentElement.classList.contains("reduce-motion");

    function esc(t) {
        return t.replace(/[&<>"]/g, function (c) {
            return { "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;" }[c];
        });
    }

    // ---------- 1) Naslov se deli na reci (maska + klizanje) ----------
    document.querySelectorAll("[data-split]").forEach(function (el) {
        var text = el.textContent.trim();
        if (!text) return;
        el.setAttribute("aria-label", text);
        el.innerHTML = text.split(/\s+/).map(function (word, i) {
            return '<span class="w" aria-hidden="true"><span style="--i:' + i + '">' + esc(word) + "</span></span>";
        }).join(" ");
    });

    // ---------- 2) Pokreni ulaznu animaciju ----------
    // "js-ready" se javlja odmah; sama animacija ceka da se otvori uvodna koverta
    root.classList.add("js-ready");

    function start() {
        requestAnimationFrame(function () {
            requestAnimationFrame(function () {
                inv.classList.add("is-ready");
                setTimeout(function () {
                    document.querySelectorAll(".rise").forEach(function (el) { el.classList.add("rise-done"); });
                }, 3500);
            });
        });
    }

    if (document.querySelector("[data-intro]")) {
        document.addEventListener("intro:opened", start, { once: true });
    } else {
        start();
    }

    // ---------- 3) Traka napretka skrola ----------
    var bar = document.createElement("div");
    bar.className = "inv-progress";
    bar.setAttribute("aria-hidden", "true");
    inv.insertBefore(bar, inv.firstChild);

    var ticking = false;
    function updateProgress() {
        var max = document.documentElement.scrollHeight - window.innerHeight;
        var p = max > 0 ? Math.min(1, Math.max(0, window.scrollY / max)) : 0;
        bar.style.setProperty("--p", p.toFixed(4));
        inv.style.setProperty("--sy", Math.round(window.scrollY));
        ticking = false;
    }
    window.addEventListener("scroll", function () {
        if (!ticking) { ticking = true; requestAnimationFrame(updateProgress); }
    }, { passive: true });
    updateProgress();

    // ---------- 4) Plutajuce ikonice ----------
    var tpl = inv.querySelector(".tpl[data-float]");
    if (tpl && window.Floaters) {
        var layer = document.createElement("div");
        layer.className = "fl-layer";
        layer.setAttribute("aria-hidden", "true");
        inv.insertBefore(layer, bar.nextSibling);

        window.Floaters.mount(layer, {
            kinds: tpl.getAttribute("data-float").split(","),
            colors: ["var(--c-main)", "var(--c-accent)", "var(--c-main)"],
            dir: tpl.getAttribute("data-float-dir") || "up",
            count: parseInt(tpl.getAttribute("data-float-count"), 10) || undefined,
            image: tpl.getAttribute("data-float-img") || undefined,
            sides: tpl.hasAttribute("data-float-sides"),
            min: parseInt((tpl.getAttribute("data-float-size") || "14,40").split(",")[0], 10), max: parseInt((tpl.getAttribute("data-float-size") || "14,40").split(",")[1], 10),
            opacity: (tpl.getAttribute("data-float-op") || "0.1,0.26").split(",").map(parseFloat)
        });
    }

    // ---------- 5) Odbrojavanje ----------
    document.querySelectorAll(".inv-countdown").forEach(function (box) {
        var target = Date.parse(box.getAttribute("data-target"));
        if (isNaN(target)) { box.hidden = true; return; }

        var units = {
            d: box.querySelector('[data-unit="d"]'),
            h: box.querySelector('[data-unit="h"]'),
            m: box.querySelector('[data-unit="m"]'),
            s: box.querySelector('[data-unit="s"]')
        };
        var timer = null, flip = false, lastSec = -1;

        function tick() {
            var diff = target - Date.now();
            if (diff <= 0) {
                box.innerHTML = '<p class="inv-countdown-over">' + (window.tr ? window.tr("Veliki dan je stigao ♥") : "Veliki dan je stigao ♥") + '</p>';
                if (timer) clearInterval(timer);
                return;
            }
            var sec = Math.floor(diff / 1000);
            units.d.textContent = Math.floor(sec / 86400);
            units.h.textContent = Math.floor((sec % 86400) / 3600);
            units.m.textContent = Math.floor((sec % 3600) / 60);

            var s = sec % 60;
            if (s !== lastSec) {
                units.s.textContent = s;
                if (!reduced) { flip = !flip; units.s.className = flip ? "p1" : "p2"; }
                lastSec = s;
            }
        }

        timer = setInterval(tick, 500);
        tick();
    });

    // ---------- 6) RSVP forma ----------
    var plusOne = document.querySelector("[data-plus-one]");
    var radios = document.querySelectorAll('input[name="StatusDolaska"]');
    if (plusOne && radios.length) {
        var sync = function () {
            var checked = document.querySelector('input[name="StatusDolaska"]:checked');
            plusOne.hidden = !checked || checked.value !== "Dolazim";
        };
        radios.forEach(function (r) { r.addEventListener("change", sync); });
        sync();
    }

    document.querySelectorAll("[data-step]").forEach(function (btn) {
        btn.addEventListener("click", function () {
            var input = document.getElementById("BrojOsobaPratnje");
            if (!input) return;
            var v = (parseInt(input.value, 10) || 0) + parseInt(btn.getAttribute("data-step"), 10);
            input.value = Math.max(0, Math.min(10, v));
        });
    });

    // ---------- 7) Pojavljivanje sekcija pri skrolu ----------
    // Kartice i brojevi unutar sekcije ulaze jedan za drugim
    document.querySelectorAll(".inv-events, .inv-countdown").forEach(function (group) {
        Array.prototype.forEach.call(group.children, function (el, i) {
            el.classList.add("inv-reveal");
            el.style.setProperty("--rd", (i * 0.14).toFixed(2) + "s");
        });
    });

    var items = document.querySelectorAll(".inv-reveal");

    if (!("IntersectionObserver" in window) || reduced) {
        items.forEach(function (el) { el.classList.add("is-in"); });
        return;
    }

    var observer = new IntersectionObserver(function (entries) {
        entries.forEach(function (entry) {
            if (entry.isIntersecting) {
                entry.target.classList.add("is-in");
                observer.unobserve(entry.target);
            }
        });
    }, { threshold: 0.12, rootMargin: "0px 0px -6% 0px" });

    items.forEach(function (el) { observer.observe(el); });
})();
