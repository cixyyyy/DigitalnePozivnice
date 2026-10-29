/* Uvodna koverta + muzika */
(function () {
    "use strict";

    var tr = window.tr || function (s) { return s; };

    var root = document.documentElement;
    var reduced = root.classList.contains("reduce-motion");
    var audio = document.getElementById("inv-music");
    var toggle = document.querySelector("[data-music-toggle]");
    var fadeTimer = null;
    var userPaused = false;   // gost je sam ugasio muziku
    var wasPlaying = false;   // za pauziranje kad se kartica sakrije
    var armed = false;

    var VOLUME = 0.5;

    function setUi(on) {
        if (!toggle) return;
        toggle.classList.toggle("is-on", on);
        toggle.setAttribute("aria-pressed", on ? "true" : "false");
        toggle.setAttribute("aria-label", on ? tr("Isključi muziku") : tr("Uključi muziku"));
    }

    function fadeTo(target, ms, done) {
        if (!audio) return;
        clearInterval(fadeTimer);
        var from = audio.volume, steps = Math.max(1, Math.round(ms / 50)), i = 0;
        fadeTimer = setInterval(function () {
            i++;
            audio.volume = Math.min(1, Math.max(0, from + (target - from) * (i / steps)));
            if (i >= steps) { clearInterval(fadeTimer); if (done) done(); }
        }, 50);
    }

    // Gost je sam pritisnuo dugme: pusti odmah (uz lagano pojačavanje)
    function play() {
        if (!audio) return;
        userPaused = false;
        audio.muted = false;
        audio.volume = 0;
        var p = audio.play();
        if (p && p.then) {
            p.then(function () { setUi(true); fadeTo(VOLUME, 2500); }).catch(function () { setUi(false); });
        } else { setUi(true); fadeTo(VOLUME, 2500); }
    }

    function pause() {
        if (!audio) return;
        userPaused = true;
        fadeTo(0, 600, function () { audio.pause(); });
        setUi(false);
    }

    /* Automatski start: pretraživači dozvoljavaju zvuk tek nakon dodira.
       Pri prvom dodiru pustimo reprodukciju utišanu, a zvuk uključimo nakon "delay" ms. */
    function startAfter(delay, fade) {
        if (!audio || armed || userPaused) return;
        armed = true;
        audio.muted = true;
        var p = audio.play();
        function unmute() {
            if (userPaused) { audio.pause(); audio.muted = false; return; }
            audio.muted = false;
            audio.volume = 0;
            setUi(true);
            fadeTo(VOLUME, fade || 3500);
        }
        if (p && p.then) {
            p.then(function () { setTimeout(unmute, delay); })
             .catch(function () { armed = false; setUi(false); });
        } else { setTimeout(unmute, delay); }
    }

    if (toggle && audio) {
        toggle.addEventListener("click", function () {
            if (audio.paused || audio.muted) { armed = true; play(); } else pause();
        });

        document.addEventListener("visibilitychange", function () {
            if (document.hidden) {
                wasPlaying = !audio.paused && !userPaused;
                if (wasPlaying) audio.pause();
            } else if (wasPlaying && !userPaused) {
                audio.play().catch(function () { });
                wasPlaying = false;
            }
        });
    }

    // ---------- Koverta ----------
    var intro = document.querySelector("[data-intro]");

    if (!intro) {
        if (audio) {
            // Dizajni koje gost mora dodirnuti (svitak, zavjesa): muzika krece uz dodir koji pokrece animaciju
            var gated = document.querySelector("[data-svitak], [data-curtain-toggle], [data-slot-spin], [data-ft-print], [data-anim=\"envelope\"]");
            var starters = "[data-svitak], [data-svitak-toggle], [data-svitak-hint], [data-curtain-toggle], .std-curtain, [data-slot-spin], [data-slot-lever], [data-ft-print], [data-anim=\"envelope\"]";

            if (gated) {
                var onStart = function (e) {
                    if (!e.target || !e.target.closest || !e.target.closest(starters)) return;
                    document.removeEventListener("click", onStart, true);
                    startAfter(reduced ? 0 : 250, 2200);
                };
                document.addEventListener("click", onStart, true);
            } else {
                // Ostali dizajni: pokusaj odmah (radi ako pretrazivac dozvoljava), inace uz prvi dodir
                var events = ["pointerdown", "touchend", "keydown", "click", "wheel", "scroll"];
                var onFirst = function (e) {
                    if (e.target && e.target.closest && e.target.closest("[data-music-toggle]")) return;
                    events.forEach(function (n) { document.removeEventListener(n, onFirst, true); });
                    startAfter(0, 2500);
                };
                events.forEach(function (n) { document.addEventListener(n, onFirst, true); });

                audio.volume = 0;
                var ap = audio.play();
                if (ap && ap.then) {
                    ap.then(function () {
                        armed = true; setUi(true); fadeTo(VOLUME, 2500);
                        events.forEach(function (n) { document.removeEventListener(n, onFirst, true); });
                    }).catch(function () { audio.volume = VOLUME; });
                }
            }
        }
        return;
    }

    root.classList.add("intro-lock");

    var button = intro.querySelector("[data-intro-open]");
    var opened = false;

    function open() {
        if (opened) return;
        opened = true;

        intro.classList.add("is-opening");
        // dodir korisnika dozvoljava reprodukciju; zvuk dolazi kad se koverta otvori
        startAfter(reduced ? 80 : 400, 2500);

        setTimeout(function () {
            intro.classList.add("is-leaving");
            root.classList.remove("intro-lock");
            window.scrollTo(0, 0);
            document.dispatchEvent(new Event("intro:opened"));
        }, reduced ? 80 : 2100);

        setTimeout(function () { intro.remove(); }, reduced ? 600 : 3600);
    }

    button.addEventListener("click", open);
    button.focus({ preventScroll: true });
})();
