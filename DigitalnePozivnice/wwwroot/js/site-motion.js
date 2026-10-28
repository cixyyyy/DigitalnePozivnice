(function () {
    "use strict";

    var root = document.documentElement;
    var reduced = document.documentElement.classList.contains("reduce-motion");

    root.classList.add("js-ready"); // javlja <head> tajmeru da JS radi

    // ---------- 1) Header dobija senku kad se skroluje ----------
    var header = document.querySelector(".site-header");
    var ticking = false;

    var hero = document.querySelector(".hero-photo");

    function onScroll() {
        if (ticking) return;
        ticking = true;
        requestAnimationFrame(function () {
            var y = window.scrollY || 0;
            if (header) header.classList.toggle("is-scrolled", y > 12);
            if (hero && !reduced && y < 900) {
                hero.style.setProperty("--py", (y * 0.12).toFixed(1) + "px");
            }
            ticking = false;
        });
    }
    window.addEventListener("scroll", onScroll, { passive: true });
    onScroll();

    // ---------- 2) Hero fotografija: ucitaj pa blago uplivaj ----------
    if (hero) {
        var src = hero.getAttribute("data-src");
        if (src) {
            var img = new Image();
            img.onload = function () {
                hero.style.setProperty("--photo", 'url("' + src + '")');
                requestAnimationFrame(function () { hero.classList.add("is-loaded"); });
            };
            img.src = src;
        }
    }

    // ---------- 3) Ulazak sekcija pri skrolu ----------
    var selector = ".intro-section,.section-heading,.how-heading,.template-card,.step-card," +
        ".templates-cta,.final-cta,.template-disclaimer,.pricing-heading,.pricing-card," +
        ".pricing-note,.my-invitations-heading,.invitation-card,.invitations-empty";
    var items = Array.prototype.slice.call(document.querySelectorAll(selector));

    // Kasnjenje (stagger) za susedne elemente istog tipa (kartice u gridu)
    items.forEach(function (el) {
        var group = Array.prototype.filter.call(el.parentNode.children, function (n) {
            return n.matches(selector);
        });
        var index = Math.max(0, group.indexOf(el));
        el.style.setProperty("--rv-delay", (Math.min(index, 5) * 0.13).toFixed(2) + "s");
    });

    // Elementi sa data-rv (nove sekcije): smjer i kasnjenje se citaju iz atributa
    var rvItems = Array.prototype.slice.call(document.querySelectorAll("[data-rv]"));
    rvItems.forEach(function (el) {
        var delay = el.getAttribute("data-rv-delay");
        if (delay) el.style.setProperty("--rv-delay", delay + "s");
    });
    items = items.concat(rvItems);

    if (reduced || !("IntersectionObserver" in window)) {
        items.forEach(function (el) { el.classList.add("is-visible"); });
    } else {
        var io = new IntersectionObserver(function (entries) {
            entries.forEach(function (entry) {
                if (entry.isIntersecting) {
                    entry.target.classList.add("is-visible");
                    io.unobserve(entry.target);
                }
            });
        }, { threshold: 0.12, rootMargin: "0px 0px -6% 0px" });

        items.forEach(function (el) { io.observe(el); });
    }

    // ---------- 4) Plutajuce ikonice u pozadini ----------
    var layer = document.querySelector(".fl-site");
    if (layer && window.Floaters) {
        var kinds = (layer.getAttribute("data-float") || "heart,flower,envelope,sparkle,ring,leaf").split(",");
        window.Floaters.mount(layer, {
            kinds: kinds,
            colors: ["#ad9465", "#89947a", "#c9a7a0", "#ad9465"],
            count: window.innerWidth < 700 ? 9 : 18,
            min: 18, max: 46,
            opacity: [0.14, 0.32]
        });
    }
})();
