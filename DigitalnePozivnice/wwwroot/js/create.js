(function () {
    "use strict";

    var dataEl = document.getElementById("cf-data");
    var form = document.getElementById("cf-form");
    if (!dataEl || !form) return;

    var data = JSON.parse(dataEl.textContent);
    var frame = document.getElementById("cf-frame");
    var screen = document.getElementById("cf-screen");
    var list = document.getElementById("cf-events");
    var tpl = document.getElementById("cf-ev-tpl");
    var addBtn = document.getElementById("cf-add");
    var hasProgram = !!(list && tpl);
    var msg = form.querySelector('[name="UvodniTekst"]');
    var names = form.querySelector('[name="ImenaDomacina"]');
    var namesLabel = document.getElementById("cf-imena-label");
    var chipsBox = document.getElementById("cf-msg-chips");
    var submit = form.querySelector(".cf-submit");
    var submitText = submit ? submit.textContent : "";
    var tr = window.tr || function (s) { return s; };

    var langSel = form.querySelector('[name="Jezik"]');
    var lang = (langSel && langSel.value) || data.jezik || "sr";
    var vrste = {};
    function setLang(code) {
        lang = data.jezici[code] ? code : "sr";
        vrste = {};
        data.jezici[lang].forEach(function (v) { vrste[v.naziv] = v; });
    }
    setLang(lang);
    var current = form.querySelector('[name="VrstaProslave"]:checked').value;

    // Je li tekst neki od ponudjenih predloga (na bilo kojem jeziku)?
    function jePredlog(text) {
        return Object.keys(data.jezici).some(function (k) {
            return data.jezici[k].some(function (v) { return v.poruke.indexOf(text) !== -1; });
        });
    }
    var eventsDirty = !!data.dirty;
    var timer = null;

    // ---------- Stavke programa ----------
    function rows() { return hasProgram ? Array.prototype.slice.call(list.children) : []; }

    function reindex() {
        rows().forEach(function (row, i) {
            row.querySelectorAll("[data-f]").forEach(function (inp) {
                inp.name = "Dogadjaji[" + i + "]." + inp.getAttribute("data-f");
            });
            row.querySelector(".cf-ev-no").textContent = i + 1;
            row.querySelector(".cf-ev-del").disabled = rows().length === 1;
        });
    }

    // ---------- Ikonice uz stavke programa ----------
    var ic = data.ikone || { ikone: [], pravila: [], podrazumijevana: "srce" };
    var icByKey = {};
    ic.ikone.forEach(function (i) { icByKey[i.kljuc] = i; });
    var icRules = ic.pravila.map(function (r) { return [new RegExp(r[0]), r[1]]; });
    function icNorm(s) { return (s || "").toLowerCase().replace(/đ/g, "dj").normalize("NFD").replace(/[\u0300-\u036f]/g, ""); }
    function icFind(t) {
        if (!t) return null;
        for (var i = 0; i < icRules.length; i++) if (icRules[i][0].test(t)) return icRules[i][1];
        return null;
    }
    function icGuess(naziv, mjesto) {
        var a = icFind(icNorm(naziv)); if (a && a !== "prsteni") return a;
        var b = icFind(icNorm(mjesto)); if (b && b !== "prsteni") return b;
        return a || b || ic.podrazumijevana;
    }
    function icInit(row) {
        var box = row.querySelector(".cf-ev-icon");
        if (!box) return;
        var hid = box.querySelector('[data-f="Ikona"]');
        var cur = box.querySelector(".cf-ic-cur");
        var grid = box.querySelector(".cf-ic-grid");
        var nm = row.querySelector('[data-f="Naziv"]');
        var pl = row.querySelector('[data-f="NazivLokacije"]');
        var autoLabel = tr("Automatski");
        function show() {
            var key = hid.value || icGuess(nm.value, pl.value);
            var i = icByKey[key] || icByKey[ic.podrazumijevana];
            if (!i) return;
            box.querySelector(".cf-ic-svg").innerHTML = i.svg;
            box.querySelector(".cf-ic-name").textContent = hid.value ? i.naziv : autoLabel + " · " + i.naziv;
            grid.querySelectorAll("button").forEach(function (b) { b.classList.toggle("is-on", b.dataset.k === hid.value); });
        }
        var html = '<button type="button" class="cf-ic-opt cf-ic-auto" data-k=""><span>✦</span><small>' + autoLabel + '</small></button>';
        ic.ikone.forEach(function (i) {
            html += '<button type="button" class="cf-ic-opt" data-k="' + i.kljuc + '" title="' + i.naziv.replace(/"/g, "&quot;") + '">' + i.svg + '<small>' + i.naziv + '</small></button>';
        });
        grid.innerHTML = html;
        cur.addEventListener("click", function () {
            var open = grid.hidden;
            grid.hidden = !open;
            cur.setAttribute("aria-expanded", open ? "true" : "false");
        });
        grid.addEventListener("click", function (e) {
            var b = e.target.closest(".cf-ic-opt");
            if (!b) return;
            hid.value = b.dataset.k;
            grid.hidden = true; cur.setAttribute("aria-expanded", "false");
            show();
            eventsDirty = true;
            hid.dispatchEvent(new Event("input", { bubbles: true }));
        });
        [nm, pl].forEach(function (el) { if (el) el.addEventListener("input", function () { if (!hid.value) show(); }); });
        show();
    }

    function addRow(v) {
        if (!hasProgram) return null;
        v = v || {};
        var row = tpl.content.firstElementChild.cloneNode(true);
        var set = function (f, val) { var el = row.querySelector('[data-f="' + f + '"]'); if (el && val) el.value = val; };
        set("Vrijeme", v.vrijeme); set("Naziv", v.naziv); set("NazivLokacije", v.lokacija); set("Adresa", v.adresa); set("Ikona", v.ikona);
        list.appendChild(row);
        icInit(row);
        reindex();
        return row;
    }

    function fillProgram(items) {
        if (!hasProgram) return;
        list.innerHTML = "";
        items.forEach(function (p) { addRow({ naziv: p.naziv, vrijeme: p.vrijeme }); });
        if (!items.length) addRow({});
    }

    if (hasProgram) {
        if (data.dogadjaji && data.dogadjaji.length) data.dogadjaji.forEach(addRow); else addRow({});
    }

    if (hasProgram) addBtn.addEventListener("click", function () {
        eventsDirty = true;
        var row = addRow({});
        var first = row.querySelector('[data-f="Vrijeme"]');
        if (first) first.focus();
        schedule(200);
    });

    if (hasProgram) list.addEventListener("click", function (e) {
        var row = e.target.closest(".cf-ev");
        if (!row) return;

        if (e.target.closest(".cf-ev-del")) {
            if (rows().length > 1) { row.remove(); reindex(); eventsDirty = true; schedule(200); }
        } else if (e.target.closest(".cf-copy")) {
            var prev = row.previousElementSibling;
            if (prev) {
                ["NazivLokacije", "Adresa"].forEach(function (f) {
                    row.querySelector('[data-f="' + f + '"]').value = prev.querySelector('[data-f="' + f + '"]').value;
                });
                eventsDirty = true; schedule(200);
            }
        }
    });

    if (hasProgram) list.addEventListener("input", function () { eventsDirty = true; });

    // ---------- Predlozi poruke ----------
    function buildChips() {
        chipsBox.innerHTML = "";
        (vrste[current].poruke || []).forEach(function (text, i) {
            var b = document.createElement("button");
            b.type = "button";
            b.textContent = ((data.predlog && data.predlog[lang]) || "Predlog") + " " + (i + 1);
            b.addEventListener("click", function () { msg.value = text; schedule(150); });
            chipsBox.appendChild(b);
        });
    }

    // ---------- Vlastiti naslov (samo za "Prilagođeno") ----------
    var naslovWrap = document.getElementById("cf-naslov-wrap");
    function toggleNaslov() {
        if (naslovWrap) naslovWrap.hidden = current !== "Prilagođeno";
    }
    toggleNaslov();

    // ---------- Promjena vrste proslave ----------
    form.querySelectorAll('[name="VrstaProslave"]').forEach(function (radio) {
        radio.addEventListener("change", function () {
            if (!radio.checked) return;
            var old = vrste[current], next = vrste[radio.value];

            var text = msg.value.trim();
            if (!text || jePredlog(text)) msg.value = next.poruke[0];

            if (!eventsDirty) fillProgram(next.program);

            namesLabel.textContent = next.label;
            names.placeholder = next.primjer;

            current = radio.value;
            toggleNaslov();
            buildChips();
            schedule(150);
        });
    });
    buildChips();

    // ---------- Promjena jezika pozivnice ----------
    if (langSel) langSel.addEventListener("change", function () {
        var vr = vrste[current];
        var text = msg.value.trim();
        var idx = vr ? vr.poruke.indexOf(text) : -1;
        setLang(langSel.value);
        var next = vrste[current];

        // tekst koji je korisnik sam pisao ostaje; ponudjeni predlog se prevodi
        if (!text) msg.value = next.poruke[0];
        else if (idx !== -1) msg.value = next.poruke[idx];
        else if (jePredlog(text)) msg.value = next.poruke[0];

        if (!eventsDirty) fillProgram(next.program);

        namesLabel.textContent = next.label;
        names.placeholder = next.primjer;
        buildChips();
        schedule(150);
    });

    // ---------- Boje i pozadina ----------
    var swatches = document.getElementById("cf-swatches");
    var bgStyles = document.getElementById("cf-bgstyles");
    var bgColors = document.getElementById("cf-bgcolors");
    var hidden = {
        main: form.querySelector('[name="OsnovnaBoja"]'),
        accent: form.querySelector('[name="DodatnaBoja"]'),
        bg: form.querySelector('[name="BojaPozadine"]')
    };
    // (birači vlastitih boja su uklonjeni; ostaju paleta i gotove boje pozadine)
    var picks = {
        main: document.getElementById("cf-pick-main"),
        accent: document.getElementById("cf-pick-accent"),
        bg: document.getElementById("cf-pick-bg")
    };

    function syncBgTiles() {
        bgStyles.style.setProperty("--pm", hidden.main.value);
        bgStyles.style.setProperty("--pa", hidden.accent.value);
        bgStyles.style.setProperty("--pb", hidden.bg.value);
    }

    function markBgColor() {
        var cur = hidden.bg.value.toLowerCase();
        bgColors.querySelectorAll(".cf-bgc").forEach(function (b) {
            b.classList.toggle("is-active", b.getAttribute("data-bg").toLowerCase() === cur);
        });
    }

    function setColors(main, accent, bg) {
        if (main) { hidden.main.value = main; if (picks.main) picks.main.value = main; }
        if (accent) { hidden.accent.value = accent; if (picks.accent) picks.accent.value = accent; }
        if (bg) { hidden.bg.value = bg; if (picks.bg) picks.bg.value = bg; }
        syncBgTiles();
        markBgColor();
    }

    function clearPalette() {
        swatches.querySelectorAll(".cf-swatch").forEach(function (b) { b.classList.remove("is-active"); });
    }

    swatches.addEventListener("click", function (e) {
        var btn = e.target.closest(".cf-swatch");
        if (!btn) return;
        clearPalette();
        btn.classList.add("is-active");
        setColors(btn.getAttribute("data-main"), btn.getAttribute("data-accent"), btn.getAttribute("data-bg"));
        schedule(100);
    });

    bgColors.addEventListener("click", function (e) {
        var btn = e.target.closest(".cf-bgc");
        if (!btn) return;
        clearPalette();
        setColors(null, null, btn.getAttribute("data-bg"));
        schedule(100);
    });

    ["main", "accent", "bg"].forEach(function (k) {
        if (!picks[k]) return;
        picks[k].addEventListener("input", function () {
            clearPalette();
            hidden[k].value = picks[k].value;
            syncBgTiles(); markBgColor();
            schedule(250);
        });
    });
    markBgColor();

    // ---------- Pregled uživo ----------
    function fit() {
        var w = screen.clientWidth || 366;
        var s = w / 390;
        frame.style.transform = "scale(" + s + ")";
        screen.style.height = Math.round(800 * s) + "px";
    }

    var lastScroll = 0;
    function refresh() {
        try { lastScroll = frame.contentWindow.pageYOffset || lastScroll; } catch (e) { }
        var oldAction = form.getAttribute("action");
        var oldTarget = form.getAttribute("target");

        form.setAttribute("action", data.previewUrl);
        form.setAttribute("target", "cf-frame");
        screen.classList.add("is-loading");
        form.submit();

        if (oldAction === null) form.removeAttribute("action"); else form.setAttribute("action", oldAction);
        if (oldTarget === null) form.removeAttribute("target"); else form.setAttribute("target", oldTarget);
    }

    function schedule(ms) {
        clearTimeout(timer);
        timer = setTimeout(refresh, ms || 700);
    }

    frame.addEventListener("load", function () {
        if (frame.getAttribute("src") === "about:blank" && !frame.contentDocument.body.children.length) return;
        screen.classList.remove("is-loading");
        // u pregledu pozadine uvijek rade (da kupac vidi sta bira), cak i ako OS trazi smanjeno kretanje
        try { frame.contentDocument.documentElement.classList.remove("reduce-motion"); } catch (e) { }
        // vrati poziciju skrola nakon osvjezavanja pregleda
        if (lastScroll > 0) {
            var y = lastScroll;
            [60, 350].forEach(function (ms) {
                setTimeout(function () { try { frame.contentWindow.scrollTo(0, y); } catch (e) { } }, ms);
            });
        }
    });

    form.addEventListener("input", function () { schedule(); });
    form.addEventListener("change", function () { schedule(); });

    window.addEventListener("resize", fit);
    if ("ResizeObserver" in window) new ResizeObserver(fit).observe(screen);
    fit();
    setTimeout(refresh, 300);

    // ---------- Mobilni pregled ----------
    var fab = document.getElementById("cf-fab");
    var close = document.getElementById("cf-close");
    function toggle(open) {
        document.body.classList.toggle("cf-preview-open", open);
        setTimeout(fit, 50);
        if (open) refresh();
    }
    if (fab) fab.addEventListener("click", function () { toggle(true); });
    if (close) close.addEventListener("click", function () { toggle(false); });

    // ---------- Slanje ----------
    form.addEventListener("submit", function () {
        if (submit) { submit.disabled = true; submit.textContent = tr("Čuvanje…"); }
    });

    // HTML5 provjera (forma ima novalidate zbog pregleda uživo)
    form.addEventListener("submit", function (e) {
        if (!form.checkValidity()) {
            e.preventDefault();
            if (submit) { submit.disabled = false; submit.textContent = submitText; }
            form.reportValidity();
        }
    });
})();
