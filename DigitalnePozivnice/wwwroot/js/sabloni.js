(function () {
    "use strict";

    var screens = Array.prototype.slice.call(document.querySelectorAll(".sg-screen"));
    if (!screens.length) return;

    function fit(screen) {
        var frame = screen.querySelector("iframe");
        var k = screen.clientWidth / 390;
        frame.style.transform = "scale(" + k + ")";
    }

    function load(screen) {
        var frame = screen.querySelector("iframe");
        if (frame.getAttribute("src")) return;
        frame.addEventListener("load", function () {
            setTimeout(function () { screen.classList.add("is-ready"); }, 350);
        });
        frame.setAttribute("src", screen.getAttribute("data-src"));
    }

    screens.forEach(fit);
    window.addEventListener("resize", function () { screens.forEach(fit); });

    if (!("IntersectionObserver" in window)) { screens.forEach(load); return; }

    var io = new IntersectionObserver(function (entries) {
        entries.forEach(function (entry) {
            if (entry.isIntersecting) {
                fit(entry.target);
                load(entry.target);
                io.unobserve(entry.target);
            }
        });
    }, { rootMargin: "250px 0px" });

    screens.forEach(function (s) { io.observe(s); });
})();
