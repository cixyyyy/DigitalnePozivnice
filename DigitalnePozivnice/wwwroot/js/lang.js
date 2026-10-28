// Zatvara meni jezika kad se klikne bilo gdje drugdje
(function () {
    document.addEventListener("click", function (e) {
        document.querySelectorAll("details.lang-switch[open]").forEach(function (d) {
            if (!d.contains(e.target)) d.removeAttribute("open");
        });
    });
    document.addEventListener("keydown", function (e) {
        if (e.key === "Escape") document.querySelectorAll("details.lang-switch[open]").forEach(function (d) { d.removeAttribute("open"); });
    });
})();
