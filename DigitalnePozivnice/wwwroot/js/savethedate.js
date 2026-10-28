(function () {
    "use strict";

    var tr = window.tr || function (s) { return s; };

    var MONTHS = (window.__i18n && window.__i18n.months) ||
        ["jan", "feb", "mar", "apr", "maj", "jun", "jul", "avg", "sep", "okt", "nov", "dec"];
    var reduced = document.documentElement.classList.contains("reduce-motion");

    function fire(origin) { if (window.Confetti) window.Confetti.fire(origin); }

    // =====================================================
    // ZVUK (Web Audio – generisan u pregledniku, bez fajlova)
    // =====================================================
    var Sfx = (function () {
        var ctx = null, on = false, lastFlip = 0;

        function ensure() {
            if (!ctx) {
                var C = window.AudioContext || window.webkitAudioContext;
                if (!C) return null;
                ctx = new C();
            }
            if (ctx.state === "suspended") ctx.resume();
            return ctx;
        }
        function tone(freq, dur, type, vol, delay) {
            if (!on) return;
            var c = ensure(); if (!c) return;
            var t = c.currentTime + (delay || 0);
            var o = c.createOscillator(), g = c.createGain();
            o.type = type || "square"; o.frequency.value = freq;
            g.gain.setValueAtTime(vol || 0.04, t);
            g.gain.exponentialRampToValueAtTime(0.0001, t + dur);
            o.connect(g); g.connect(c.destination);
            o.start(t); o.stop(t + dur + 0.02);
        }
        return {
            setOn: function (v) { on = v; if (v) ensure(); },
            isOn: function () { return on; },
            tick: function () { tone(880 + Math.random() * 260, 0.035, "square", 0.025); },
            clack: function () { tone(160, 0.09, "triangle", 0.09); tone(95, 0.12, "sine", 0.08); },
            flip: function () {
                var n = Date.now();
                if (n - lastFlip < 55) return;
                lastFlip = n;
                tone(260 + Math.random() * 120, 0.03, "square", 0.02);
            },
            win: function () { [523, 659, 784, 1047, 1319].forEach(function (f, i) { tone(f, 0.38, "triangle", 0.07, i * 0.1); }); }
        };
    })();

    function addSoundToggle(host) {
        var b = document.createElement("button");
        b.type = "button";
        b.className = "std-sound";
        b.setAttribute("aria-pressed", "false");
        b.setAttribute("aria-label", tr("Uključi zvuk"));
        b.innerHTML = '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.7" stroke-linecap="round" stroke-linejoin="round">' +
            '<path d="M4 9v6h4l5 4V5L8 9H4z"/><path class="w1" d="M16 9a4 4 0 0 1 0 6"/><path class="w2" d="M18.5 6.5a8 8 0 0 1 0 11"/><path class="off" d="M17 9l5 6M22 9l-5 6"/></svg>';
        b.addEventListener("click", function () {
            var v = !Sfx.isOn();
            Sfx.setOn(v);
            b.classList.toggle("is-on", v);
            b.setAttribute("aria-pressed", v ? "true" : "false");
            b.setAttribute("aria-label", v ? tr("Isključi zvuk") : tr("Uključi zvuk"));
            if (v) Sfx.tick();
        });
        // zvucni efekti su podrazumijevano ukljuceni (gost ih moze ugasiti)
        Sfx.setOn(true);
        b.classList.add("is-on"); b.setAttribute("aria-pressed", "true"); b.setAttribute("aria-label", tr("Isključi zvuk"));
        host.appendChild(b);
    }

    // =====================================================
    // 1) SLOT MASINA
    // =====================================================
    function initSlot(root) {
        var day = parseInt(root.getAttribute("data-day"), 10);
        var month = parseInt(root.getAttribute("data-month"), 10);
        var year = parseInt(root.getAttribute("data-year"), 10);
        var button = root.querySelector("[data-slot-spin]");
        var lever = root.querySelector("[data-slot-lever]");
        var machine = root.querySelector(".std-slot-machine");
        var REPEAT = 7;

        function range(a, b) { var r = []; for (var i = a; i <= b; i++) r.push(String(i)); return r; }

        var defs = [
            { name: "day", items: range(1, 31), target: String(day) },
            { name: "month", items: MONTHS.slice(), target: MONTHS[month - 1] },
            { name: "year", items: range(year - 2, year + 2), target: String(year) }
        ];

        var reels = defs.map(function (def) {
            var el = root.querySelector('[data-reel="' + def.name + '"]');
            var list = ["?"];
            for (var r = 0; r < REPEAT; r++) list = list.concat(def.items);
            el.innerHTML = list.map(function (t) { return "<span>" + t + "</span>"; }).join("");
            var idx = 1 + (REPEAT - 2) * def.items.length + def.items.indexOf(def.target);
            return { el: el, n: list.length, idx: idx };
        });

        var spinning = false, tickTimer = null;
        var travel = function (r) { return "translateY(-" + (r.idx / r.n * 100) + "%)"; };

        function finish() {
            spinning = false;
            clearInterval(tickTimer);
            root.classList.remove("is-spinning");
            root.classList.add("is-done");
            if (button) { button.disabled = false; button.textContent = tr("Zavrti ponovo ♥"); }
            Sfx.win();
            fire(machine || root);
        }

        function spin() {
            if (spinning) return;
            spinning = true;
            root.classList.remove("is-done", "is-idle");
            root.classList.add("is-spinning");
            if (button) button.disabled = true;

            if (lever) {
                lever.classList.add("is-pulled");
                setTimeout(function () { lever.classList.remove("is-pulled"); }, 520);
            }

            if (reduced) {
                reels.forEach(function (r) { r.el.style.transition = "none"; r.el.style.transform = travel(r); });
                finish();
                return;
            }

            reels.forEach(function (r) { r.el.style.transition = "none"; r.el.style.transform = "translateY(0)"; });
            void root.offsetWidth;

            var last = 0;
            reels.forEach(function (r, i) {
                var duration = 2400 + i * 900;
                last = duration;
                r.el.classList.add("fast");
                r.el.style.transition = "transform " + duration + "ms cubic-bezier(.1,.62,.12,1)";
                r.el.style.transform = travel(r);

                setTimeout(function () { r.el.classList.remove("fast"); }, duration * 0.58);
                setTimeout(function () {
                    r.el.classList.remove("settle"); void r.el.offsetWidth; r.el.classList.add("settle");
                    Sfx.clack();
                }, duration);
            });

            tickTimer = setInterval(Sfx.tick, 85);
            setTimeout(finish, last + 280);
        }

        root.classList.add("is-idle");

        // ?auto=1 (npr. telefon na pocetnoj): masina se sama vrti u krug
        var auto = /[?&]auto=1/.test(location.search);
        if (auto) {
            var again = function () {
                setTimeout(function () {
                    spin();
                    var wait = setInterval(function () {
                        if (!spinning) { clearInterval(wait); again(); }
                    }, 500);
                }, 3800);
            };
            setTimeout(function () { spin(); var w0 = setInterval(function () { if (!spinning) { clearInterval(w0); again(); } }, 500); }, 1500);
        }

        if (button) button.addEventListener("click", spin);
        if (lever) lever.addEventListener("click", spin);

        if (!auto) addSoundToggle(root.closest(".std-stage") || root);
    }

    // =====================================================
    // 2) KOVERTA
    // =====================================================
    function initEnvelope(root) {
        var hint = document.querySelector("[data-envelope-hint]");

        function toggle() {
            var open = root.classList.toggle("is-open");
            root.setAttribute("aria-expanded", open ? "true" : "false");
            root.setAttribute("aria-label", open ? tr("Zatvori kovertu") : tr("Otvori kovertu"));
            if (hint) hint.textContent = open ? tr("Dodirnite ponovo da zatvorite") : tr("Dodirnite pečat da otvorite kovertu");
            if (open) setTimeout(function () { fire(root.querySelector(".env-card") || root); }, 1300);
        }

        root.addEventListener("click", toggle);
        root.addEventListener("keydown", function (e) {
            if (e.key === "Enter" || e.key === " ") { e.preventDefault(); toggle(); }
        });
    }

    // =====================================================
    // 3) TABLA (pravi split-flap: gornja i donja polovina)
    // =====================================================
    function initBoard(root) {
        var rows = (root.getAttribute("data-rows") || "").split("|");
        var CHARSET = "ABCDEFGHIJKLMNOPRSTUVZ0123456789";
        var tiles = [];
        var timer = null;

        root.innerHTML = "";

        function half(cls) {
            var d = document.createElement("div");
            d.className = cls;
            d.appendChild(document.createElement("span"));
            return d;
        }

        rows.forEach(function (text) {
            var row = document.createElement("div");
            row.className = "std-flap-row";
            row.setAttribute("aria-hidden", "true");

            Array.prototype.forEach.call(text, function (ch) {
                var el = document.createElement("div");
                if (ch === " ") { el.className = "std-flap is-blank"; row.appendChild(el); return; }

                el.className = "std-flap";
                var t = { el: el, top: half("f-top"), bot: half("f-bot"), ft: half("f-ft"), fb: half("f-fb"),
                          cur: " ", target: ch, busy: false, done: false, stopAt: 0 };
                [t.top, t.bot, t.ft, t.fb].forEach(function (h) { el.appendChild(h); });
                row.appendChild(el);
                tiles.push(t);
            });
            root.appendChild(row);
        });

        function put(h, c) { h.firstChild.textContent = c === " " ? "" : c; }
        function showStatic(t, c) { put(t.top, c); put(t.bot, c); put(t.ft, c); put(t.fb, c); t.cur = c; }

        function flipTo(t, c) {
            t.busy = true;
            put(t.top, c); put(t.ft, t.cur); put(t.fb, c);
            t.el.classList.add("flip");
            Sfx.flip();
            setTimeout(function () {
                put(t.bot, c);
                t.el.classList.remove("flip");
                t.cur = c; t.busy = false;
            }, 140);
        }

        tiles.forEach(function (t) { showStatic(t, " "); });

        function finishNow() {
            tiles.forEach(function (t) { showStatic(t, t.target); t.done = true; });
            root.classList.add("is-done");
        }

        function run() {
            if (timer) clearInterval(timer);
            root.classList.remove("is-done");
            if (reduced) { finishNow(); return; }

            var start = Date.now();
            tiles.forEach(function (t, i) {
                t.done = false;
                t.stopAt = 900 + i * 120 + Math.random() * 300;
            });

            timer = setInterval(function () {
                var elapsed = Date.now() - start, pending = 0;

                tiles.forEach(function (t) {
                    if (t.done) return;
                    if (t.busy) { pending++; return; }
                    if (elapsed >= t.stopAt) {
                        if (t.cur === t.target) { t.done = true; return; }
                        flipTo(t, t.target);
                        t.done = true;
                    } else {
                        flipTo(t, CHARSET.charAt(Math.floor(Math.random() * CHARSET.length)));
                        pending++;
                    }
                });

                if (pending === 0 && tiles.every(function (t) { return t.done && !t.busy; })) {
                    clearInterval(timer); timer = null;
                    root.classList.add("is-done");
                    Sfx.win();
                    fire(root);
                }
            }, 150);
        }

        var replay = document.querySelector("[data-board-replay]");
        if (replay) replay.addEventListener("click", function () {
            tiles.forEach(function (t) { showStatic(t, " "); });
            run();
        });

        addSoundToggle(root.closest(".std-stage") || root);

        if ("IntersectionObserver" in window) {
            var obs = new IntersectionObserver(function (entries) {
                if (entries[0].isIntersecting) { obs.disconnect(); setTimeout(run, 500); }
            }, { threshold: 0.3 });
            obs.observe(root);
        } else { run(); }
    }

    // =====================================================
    // 4) ZAVESA
    // =====================================================
    function initCurtain(root) {
        var button = root.querySelector("[data-curtain-toggle]");
        if (!button) return;

        button.addEventListener("click", function () {
            var open = root.classList.toggle("is-open");
            button.textContent = open ? tr("Zatvori zavesu") : tr("Otvori zavesu");
            if (open) setTimeout(function () { fire(root.querySelector(".std-curtain-content") || root); }, 1500);
        });
    }

    document.querySelectorAll("[data-anim]").forEach(function (el) {
        switch (el.getAttribute("data-anim")) {
            case "slot": initSlot(el); break;
            case "envelope": initEnvelope(el); break;
            case "board": initBoard(el); break;
            case "curtain": initCurtain(el); break;
        }
    });
})();
