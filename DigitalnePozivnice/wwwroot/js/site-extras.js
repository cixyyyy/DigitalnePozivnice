(function () {
    "use strict";

    // ---------- Dugme "na vrh" ----------
    var btn = document.getElementById("to-top");
    if (btn) {
        var ticking = false;
        var update = function () {
            ticking = false;
            btn.classList.toggle("is-visible", (window.pageYOffset || document.documentElement.scrollTop) > 700);
        };
        window.addEventListener("scroll", function () {
            if (!ticking) { ticking = true; requestAnimationFrame(update); }
        }, { passive: true });
        update();

        btn.addEventListener("click", function () {
            var reduced = document.documentElement.classList.contains("reduce-motion");
            window.scrollTo({ top: 0, behavior: reduced ? "auto" : "smooth" });
        });
    }

    // ---------- Mobilni meni: zatvara se klikom/dodirom bilo gdje vani ----------
    var nav = document.getElementById("main-navigation");
    var toggle = document.querySelector(".mobile-menu-toggle");
    if (nav && toggle) {
        var close = function () {
            if (!nav.classList.contains("is-open")) return;
            nav.classList.remove("is-open");
            toggle.setAttribute("aria-expanded", "false");
            toggle.setAttribute("aria-label", (window.tr ? window.tr("Otvori meni") : "Otvori meni"));
        };

        document.addEventListener("pointerdown", function (e) {
            if (!nav.classList.contains("is-open")) return;
            if (nav.contains(e.target) || toggle.contains(e.target)) return;
            close();
        });
        // klik na link u meniju (npr. "Paketi" na istoj stranici) takodje zatvara meni
        nav.addEventListener("click", function (e) {
            var a = e.target.closest && e.target.closest("a");
            if (a) close();
        });
        document.addEventListener("keydown", function (e) { if (e.key === "Escape") close(); });
    }
})();
