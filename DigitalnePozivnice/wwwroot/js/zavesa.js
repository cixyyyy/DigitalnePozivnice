/* Zavjesa: dodir na zavjesu je otvara; mali pregledi je sami otvore. */
(function () {
    "use strict";
    var stage = document.querySelector(".zv-stage");
    if (!stage) return;
    var btn = stage.querySelector("[data-curtain-toggle]");
    if (!btn) return;

    stage.querySelectorAll(".std-curtain").forEach(function (c) {
        c.addEventListener("click", function () { if (!stage.classList.contains("is-open")) btn.click(); });
    });

    var embedded = false;
    try { embedded = window.self !== window.top; } catch (e) { embedded = true; }
    if (embedded || /[?&]auto=1/.test(location.search)) setTimeout(function () {
        if (!stage.classList.contains("is-open")) btn.click();
    }, 1200);
})();
