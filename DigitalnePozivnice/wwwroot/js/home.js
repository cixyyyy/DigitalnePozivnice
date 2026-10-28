(function () {
    "use strict";

    var reduced = document.documentElement.classList.contains("reduce-motion");

    // =====================================================
    // 1) Rotirajuća riječ u naslovu
    // =====================================================
    var rot = document.querySelector("[data-rot]");
    if (rot) {
        var words = Array.prototype.slice.call(rot.querySelectorAll(".hx-word"));
        var index = 0;

        var setWidth = function (el) {
            rot.style.width = Math.ceil(el.getBoundingClientRect().width) + "px";
        };

        var ready = (document.fonts && document.fonts.ready) ? document.fonts.ready : Promise.resolve();
        ready.then(function () { setWidth(words[index]); });
        window.addEventListener("resize", function () { setWidth(words[index]); });

        if (!reduced && words.length > 1) {
            setTimeout(function () {
                setInterval(function () {
                    var current = words[index];
                    index = (index + 1) % words.length;
                    var next = words[index];

                    setWidth(next);
                    current.classList.remove("is-active");
                    current.classList.add("is-leaving");
                    next.classList.remove("is-leaving");
                    next.classList.add("is-active");

                    setTimeout(function () { current.classList.remove("is-leaving"); }, 1100);
                }, 3100);
            }, 2600);
        }
    }

    // =====================================================
    // 2) "Kucanje" podnaslova
    // =====================================================
    var tw = document.querySelector("[data-typewriter]");
    if (tw) {
        var full = tw.getAttribute("data-typewriter");
        var out = tw.querySelector(".hx-sub-type");

        if (reduced) {
            out.textContent = full;
            tw.classList.add("caret-off");
        } else {
            var n = 0;
            var step = function () {
                n++;
                out.textContent = full.slice(0, n);
                if (n < full.length) {
                    var ch = full.charAt(n - 1);
                    var pause = (ch === "." || ch === ",") ? 110 : 9 + Math.random() * 14;
                    setTimeout(step, pause);
                } else {
                    setTimeout(function () { tw.classList.add("caret-off"); }, 2400);
                }
            };
            setTimeout(step, 700);
        }
    }

    // =====================================================
    // 2b) Zivi prikaz (Slot masina) u telefonu na vrhu
    // =====================================================
    var live = document.querySelector("[data-hero-live]");
    if (live) {
        var lf = live.querySelector("iframe");
        var fitLive = function () {
            var k = live.clientWidth / 390;
            if (!k) return;
            lf.style.width = "390px";
            lf.style.height = Math.ceil(live.clientHeight / k) + "px";
            lf.style.transform = "scale(" + k + ")";
        };
        fitLive();
        window.addEventListener("resize", fitLive);
        if ("ResizeObserver" in window) new ResizeObserver(fitLive).observe(live);
        lf.addEventListener("load", function () {
            fitLive();
            setTimeout(function () { live.classList.add("is-ready"); }, 300);
        });
        lf.setAttribute("src", live.getAttribute("data-src"));
    }

    // =====================================================
    // 3) Telefon prati miš (blagi 3D nagib)
    // =====================================================
    var stage = document.querySelector(".hx-stage");
    var tilt = stage && stage.querySelector(".phone-tilt");
    if (tilt && !reduced && window.matchMedia("(pointer: fine)").matches) {
        var tx = 0, ty = 0, cx = 0, cy = 0, raf = null;

        var frame = function () {
            cx += (tx - cx) * 0.08;
            cy += (ty - cy) * 0.08;
            tilt.style.setProperty("--ry", (cx * 16).toFixed(2) + "deg");
            tilt.style.setProperty("--rx", (-cy * 11).toFixed(2) + "deg");
            raf = (Math.abs(tx - cx) > 0.002 || Math.abs(ty - cy) > 0.002)
                ? requestAnimationFrame(frame) : null;
        };
        var kick = function () { if (!raf) raf = requestAnimationFrame(frame); };

        stage.addEventListener("pointermove", function (e) {
            var r = stage.getBoundingClientRect();
            tx = (e.clientX - r.left) / r.width - 0.5;
            ty = (e.clientY - r.top) / r.height - 0.5;
            kick();
        });
        stage.addEventListener("pointerleave", function () { tx = 0; ty = 0; kick(); });
    }

    // =====================================================
    // 4) Brojači u admin panelu
    // =====================================================
    var counters = document.querySelectorAll("[data-count]");
    if (counters.length) {
        var run = function (el) {
            var target = parseInt(el.getAttribute("data-count"), 10);
            if (reduced) { el.textContent = target; return; }
            var start = null, duration = 1800;
            el.textContent = "0";
            var tick = function (t) {
                if (start === null) start = t;
                var p = Math.min(1, (t - start) / duration);
                var eased = 1 - Math.pow(2, -10 * p);
                el.textContent = Math.round(target * (p === 1 ? 1 : eased));
                if (p < 1) requestAnimationFrame(tick);
            };
            requestAnimationFrame(tick);
        };

        if ("IntersectionObserver" in window) {
            var io = new IntersectionObserver(function (entries) {
                entries.forEach(function (e) {
                    if (e.isIntersecting) {
                        setTimeout(function () { run(e.target); }, 500);
                        io.unobserve(e.target);
                    }
                });
            }, { threshold: 0.6 });
            counters.forEach(function (el) { el.textContent = "0"; io.observe(el); });
        }
    }
})();
