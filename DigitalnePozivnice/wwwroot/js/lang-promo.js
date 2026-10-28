// Pocetna: kartica pozivnice koja se smjenjuje kroz sve jezike
(function () {
    "use strict";
    var dataEl = document.getElementById("lp-data");
    var card = document.getElementById("lp-card");
    if (!dataEl || !card) return;

    var data = JSON.parse(dataEl.textContent);
    var order = ["sr", "hr", "bs", "sl", "de", "en"];
    var pills = document.querySelectorAll(".lp-pill");
    var reduced = document.documentElement.classList.contains("reduce-motion");
    var idx = 0, timer = null;

    function fill(code) {
        var d = data[code];
        card.querySelectorAll("[data-k]").forEach(function (el) {
            var k = el.getAttribute("data-k");
            el.textContent = k === "code" ? code.toUpperCase() : d[k];
        });
        pills.forEach(function (p) { p.classList.toggle("is-active", p.getAttribute("data-lp") === code); });
    }

    function show(code, animate) {
        idx = order.indexOf(code);
        if (!animate || reduced) { fill(code); return; }
        card.classList.add("is-out");
        setTimeout(function () { fill(code); card.classList.remove("is-out"); }, 320);
    }

    function start() {
        stop();
        if (reduced) return;
        timer = setInterval(function () { show(order[(idx + 1) % order.length], true); }, 2800);
    }
    function stop() { if (timer) { clearInterval(timer); timer = null; } }

    pills.forEach(function (p) {
        p.addEventListener("click", function () { show(p.getAttribute("data-lp"), true); start(); });
    });

    fill(order[0]);
    start();
})();
