/* Galerija: pregled fotografija preko cijelog ekrana */
(function () {
    "use strict";

    var gals = document.querySelectorAll("[data-gallery]");
    if (!gals.length) return;

    var tr = window.tr || function (s) { return s; };
    var items = [];
    gals.forEach(function (g) {
        g.querySelectorAll("[data-full]").forEach(function (b) { items.push(b); });
    });

    var lb = null, img, count, cur = 0, startX = null, lastFocus = null;

    function build() {
        lb = document.createElement("div");
        lb.className = "inv-lb";
        lb.hidden = true;
        lb.setAttribute("role", "dialog");
        lb.setAttribute("aria-modal", "true");
        lb.innerHTML =
            '<button type="button" class="inv-lb-x" aria-label="' + tr("Zatvori") + '">×</button>' +
            '<button type="button" class="inv-lb-p" aria-label="' + tr("Prethodna") + '">‹</button>' +
            '<img alt="" />' +
            '<button type="button" class="inv-lb-n" aria-label="' + tr("Sljedeća") + '">›</button>' +
            '<div class="inv-lb-c"></div>';
        document.body.appendChild(lb);
        img = lb.querySelector("img");
        count = lb.querySelector(".inv-lb-c");

        lb.querySelector(".inv-lb-x").addEventListener("click", close);
        lb.querySelector(".inv-lb-p").addEventListener("click", function () { go(-1); });
        lb.querySelector(".inv-lb-n").addEventListener("click", function () { go(1); });
        lb.addEventListener("click", function (e) { if (e.target === lb) close(); });
        lb.addEventListener("touchstart", function (e) { startX = e.touches[0].clientX; }, { passive: true });
        lb.addEventListener("touchend", function (e) {
            if (startX === null) return;
            var dx = e.changedTouches[0].clientX - startX;
            startX = null;
            if (Math.abs(dx) > 50) go(dx < 0 ? 1 : -1);
        }, { passive: true });
    }

    function show(i) {
        cur = (i + items.length) % items.length;
        img.src = items[cur].getAttribute("data-full");
        count.textContent = (cur + 1) + " / " + items.length;
    }

    function go(d) { if (items.length > 1) show(cur + d); }

    function open(i) {
        if (!lb) build();
        lastFocus = document.activeElement;
        show(i);
        lb.hidden = false;
        requestAnimationFrame(function () { lb.classList.add("is-open"); });
        document.documentElement.style.overflow = "hidden";
        lb.querySelector(".inv-lb-x").focus();
    }

    function close() {
        if (!lb) return;
        lb.classList.remove("is-open");
        document.documentElement.style.overflow = "";
        setTimeout(function () { lb.hidden = true; img.removeAttribute("src"); }, 220);
        if (lastFocus && lastFocus.focus) lastFocus.focus();
    }

    items.forEach(function (b, i) { b.addEventListener("click", function () { open(i); }); });

    document.addEventListener("keydown", function (e) {
        if (!lb || lb.hidden) return;
        if (e.key === "Escape") close();
        else if (e.key === "ArrowLeft") go(-1);
        else if (e.key === "ArrowRight") go(1);
    });
})();
