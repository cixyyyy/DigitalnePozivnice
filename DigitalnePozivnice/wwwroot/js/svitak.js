/* Svitak: dodir otvara svitak (valjci se razdvajaju, list se odmotava). */
(function () {
    "use strict";

    var tr = window.tr || function (s) { return s; };
    var root = document.querySelector("[data-svitak]");
    if (!root) return;

    var stage = root.closest(".svt-stage") || root;
    var btn = root.querySelector("[data-svitak-toggle]");
    var hint = stage.querySelector("[data-svitak-hint]");
    var opened = false;

    function open() {
        if (opened) return;
        opened = true;
        root.classList.add("is-open");
        stage.classList.add("is-open");
        if (btn) { btn.setAttribute("aria-expanded", "true"); btn.tabIndex = -1; }

        // nakon odmotavanja: mali pljusak zlatnih cestica i novi natpis
        setTimeout(function () {
            if (window.Confetti && !document.documentElement.classList.contains("reduce-motion")) {
                window.Confetti.fire(root.querySelector(".svt-card") || root);
            }
        }, 1900);
        if (hint) {
            setTimeout(function () {
                hint.textContent = hint.getAttribute("data-close") || hint.textContent;
            }, 2600);
        }
    }

    if (btn) btn.addEventListener("click", open);
    root.addEventListener("click", function (e) { if (!opened && !e.target.closest("a")) open(); });

    // "Dodirnite da otvorite" je takodje dugme; kad je svitak otvoren vodi na detalje
    if (hint) hint.addEventListener("click", function () {
        if (!opened) { open(); return; }
        var next = stage.parentElement && stage.parentElement.querySelector(".inv-section");
        if (next && next.scrollIntoView) next.scrollIntoView({ behavior: "smooth", block: "start" });
    });

    // Mali pregledi (okvir na stranici izbora) i ?auto=1: svitak se sam otvori
    var embedded = false;
    try { embedded = window.self !== window.top; } catch (e) { embedded = true; }
    if (embedded || /[?&]auto=1/.test(location.search)) setTimeout(open, 1200);
})();
