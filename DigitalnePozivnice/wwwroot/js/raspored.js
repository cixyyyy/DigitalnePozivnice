(function () {
    "use strict";

    var tr = window.tr || function (s) { return s; };
    function trf(k) {
        var s = tr(k);
        for (var i = 1; i < arguments.length; i++) s = s.replace("{" + (i - 1) + "}", arguments[i]);
        return s;
    }

    var dataEl = document.getElementById("rs-data");
    if (!dataEl) return;

    var data = JSON.parse(dataEl.textContent);
    var $ = function (id) { return document.getElementById(id); };

    var hall = $("rsHall"), stage = $("rsStage"), hint = $("rsHint");
    var guestsBox = $("rsGuests"), freeCount = $("rsFreeCount"), detail = $("rsDetail");
    var stateEl = $("rsState"), toast = $("rsToast"), saveBtn = $("rsSave");
    var addBtn = $("rsAdd"), menu = $("rsMenu");
    var tokenEl = document.querySelector('input[name="__RequestVerificationToken"]');

    var OBLICI = ["okrugli", "pravougaoni"];
    var OBLIK_NAZIV = { okrugli: "Okrugli", pravougaoni: "Pravougaoni" };
    var MAX_MJESTA = { okrugli: 16, pravougaoni: 30 };

    // Gotovi blokovi stolova
    var PRESETI = [
        { oblik: "okrugli", mjesta: 8, naziv: tr("Okrugli sto"), slika: "is-round" },
        { oblik: "pravougaoni", mjesta: 10, naziv: tr("Pravougaoni sto"), slika: "is-rect" }
    ];

    var tables = data.stolovi.map(function (t) {
        return {
            key: t.kljuc, id: t.id, naziv: t.naziv, mjesta: t.mjesta, x: +t.x, y: +t.y,
            oblik: t.oblik === "okrugli" ? "okrugli" : "pravougaoni",
            rot: t.rotacija === 90 ? 90 : 0,
            skala: clamp(+t.skala || 1, 0.5, 3),
            skalaV: clamp(+t.skalaV || 1, 0.5, 3)
        };
    });
    var guests = data.gosti.map(function (g) {
        return { id: g.id, ime: g.ime, osoba: g.osoba, sto: g.sto || null, rucni: !!g.rucni };
    });

    var selected = null, picked = null, dirty = false, seq = 0, is3d = false, zoom = 0.9;

    function el(tag, cls, text) {
        var n = document.createElement(tag);
        if (cls) n.className = cls;
        if (text !== undefined && text !== null) n.textContent = text;
        return n;
    }
    function clamp(v, a, b) { return Math.min(b, Math.max(a, v)); }
    function byKey(k) { return tables.filter(function (t) { return t.key === k; })[0]; }
    function guestById(id) { return guests.filter(function (g) { return g.id === id; })[0]; }
    function used(key) {
        return guests.filter(function (g) { return g.sto === key; })
                     .reduce(function (s, g) { return s + g.osoba; }, 0);
    }
    function setDirty(v) {
        dirty = v;
        stateEl.textContent = v ? tr("Nesačuvane izmjene") : tr("Sačuvano");
        stateEl.classList.toggle("is-dirty", v);
    }
    function say(msg, err) {
        toast.textContent = msg;
        toast.classList.toggle("is-err", !!err);
        toast.classList.add("is-on");
        clearTimeout(say.t);
        say.t = setTimeout(function () { toast.classList.remove("is-on"); }, 3200);
    }
    function token() { return tokenEl ? tokenEl.value : ""; }

    // ---------- Geometrija stola ----------
    // Osnovna (nerotirana) velicina
    function baseDims(t) {
        var m = t.mjesta, k = t.skala || 1;
        if (t.oblik === "pravougaoni") return { w: (60 + Math.ceil(m / 2) * 26) * k, h: 60 * (t.skalaV || 1) };
        var d = (44 + m * 3.5) * k;
        return { w: d, h: d };
    }

    // Velicina na ekranu (rotacija 90 zamijeni sirinu i visinu)
    function dims(t) {
        var b = baseDims(t);
        return t.oblik === "pravougaoni" && t.rot === 90 ? { w: b.h, h: b.w } : b;
    }

    function seatPos(t, i) {
        var m = t.mjesta, off = 11, b = baseDims(t);
        if (t.oblik === "pravougaoni") {
            var top = Math.ceil(m / 2), bottom = m - top, x, y;
            if (i < top) { x = -b.w / 2 + (i + 0.5) * b.w / top; y = -(b.h / 2 + off); }
            else { var j = i - top; x = -b.w / 2 + (j + 0.5) * b.w / bottom; y = b.h / 2 + off; }
            return t.rot === 90 ? [-y, x] : [x, y];
        }
        var r = b.w / 2 + off, a = 2 * Math.PI * i / m;
        return [r * Math.sin(a), -r * Math.cos(a)];
    }

    // ---------- Sala ----------
    function renderHall() {
        stage.innerHTML = "";
        hint.hidden = tables.length > 0;

        tables.forEach(function (t) {
            var sz = dims(t);
            var u = used(t.key);

            var node = el("div", "rs-table" + (t.oblik === "pravougaoni" ? " is-rect" : "") +
                (t.key === selected ? " is-selected" : "") + (u >= t.mjesta ? " is-full" : ""));
            node.setAttribute("data-key", t.key);
            node.style.setProperty("--w", sz.w + "px");
            node.style.setProperty("--h", sz.h + "px");
            node.style.left = t.x + "%";
            node.style.top = t.y + "%";

            for (var i = 0; i < t.mjesta; i++) {
                var p = seatPos(t, i);
                var seat = el("span", "rs-seat" + (i < u ? " is-on" : ""));
                seat.style.setProperty("--sx", p[0].toFixed(1) + "px");
                seat.style.setProperty("--sy", p[1].toFixed(1) + "px");
                node.appendChild(seat);
            }

            var label = el("div", "rs-label");
            label.appendChild(el("b", null, t.naziv));
            label.appendChild(el("small", null, u + "/" + t.mjesta));
            node.appendChild(label);

            if (t.key === selected && !is3d) {
                var pop = el("div", "rs-pop" + (t.y < 32 ? " is-below" : ""));
                pop.appendChild(el("strong", null, trf("Za stolom ({0})", u)));
                var who = guests.filter(function (g) { return g.sto === t.key; });
                if (!who.length) {
                    pop.appendChild(el("span", "rs-pop-empty", tr("Još nema gostiju")));
                } else {
                    var ul = el("ul");
                    who.slice(0, 8).forEach(function (g) {
                        ul.appendChild(el("li", null, g.ime + (g.osoba > 1 ? " +" + (g.osoba - 1) : "")));
                    });
                    if (who.length > 8) ul.appendChild(el("li", "rs-pop-more", trf("i još {0}…", who.length - 8)));
                    pop.appendChild(ul);
                }
                node.appendChild(pop);

                if (t.oblik === "pravougaoni") {
                    var gx = el("span", "rs-grip rs-grip-x");
                    gx.setAttribute("title", tr("Povucite da promijenite širinu"));
                    node.appendChild(gx);
                    var gy = el("span", "rs-grip rs-grip-y");
                    gy.setAttribute("title", tr("Povucite da promijenite visinu"));
                    node.appendChild(gy);
                }
                var grip = el("span", "rs-grip");
                grip.setAttribute("title", tr("Povucite da promijenite veličinu"));
                node.appendChild(grip);
            }

            stage.appendChild(node);
        });

        markTargets();
    }

    function markTargets() {
        var g = picked ? guestById(picked) : null;
        Array.prototype.forEach.call(stage.children, function (n) {
            var t = byKey(n.getAttribute("data-key"));
            n.classList.toggle("is-target", !!(g && t && t.mjesta - used(t.key) >= g.osoba && g.sto !== t.key));
        });
        hall.classList.toggle("is-drop", !!picked && !is3d);
        if (picked) {
            $("rsDrop").textContent = tr("Dodirnite sto, ili prazno mjesto da napravite novi sto");
        } else {
            $("rsDrop").textContent = tr("Pustite gosta na sto, ili na prazno mjesto da napravite novi sto");
        }
    }

    // ---------- Gosti bez stola ----------
    function renderGuests() {
        var free = guests.filter(function (g) { return !g.sto; });
        freeCount.textContent = free.reduce(function (s, g) { return s + g.osoba; }, 0) + " " + tr("os.");
        guestsBox.innerHTML = "";

        if (!guests.length) {
            guestsBox.appendChild(el("p", "rs-empty", tr("Još nema gostiju. Dodajte ih ručno ispod ili sačekajte potvrde.")));
            return;
        }
        if (!free.length) {
            guestsBox.appendChild(el("p", "rs-empty", tr("Svi gosti imaju sto. 🎉")));
            return;
        }

        free.forEach(function (g) {
            var chip = el("div", "rs-chip" + (picked === g.id ? " is-picked" : "") + (g.rucni ? " is-manual" : ""));
            chip.appendChild(el("span", null, g.ime));
            if (g.osoba > 1) chip.appendChild(el("em", null, "+" + (g.osoba - 1)));

            if (g.rucni) {
                var x = el("button", "rs-x", "×");
                x.type = "button";
                x.title = tr("Obriši ručno dodanog gosta");
                x.setAttribute("aria-label", trf("Obriši gosta {0}", g.ime));
                x.addEventListener("pointerdown", function (e) { e.stopPropagation(); });
                x.addEventListener("click", function (e) { e.stopPropagation(); removeManual(g); });
                chip.appendChild(x);
            }

            bindChip(chip, g);
            guestsBox.appendChild(chip);
        });
    }

    // ---------- Detalji stola ----------
    function renderDetail() {
        detail.innerHTML = "";
        var t = byKey(selected);

        if (!t) {
            detail.appendChild(el("h3", null, "Sto"));
            detail.appendChild(el("p", "rs-empty", tr("Izaberite sto u sali da promijenite naziv, oblik i broj mjesta ili da vidite njegove goste.")));
            return;
        }

        var u = used(t.key);
        var head = el("div", "rs-card-head");
        var title = el("h3", null, t.naziv);
        var count = el("span", null, u + "/" + t.mjesta);
        head.appendChild(title); head.appendChild(count);
        detail.appendChild(head);

        var bar = el("div", "rs-bar"), fill = el("span");
        fill.style.width = Math.min(100, u / t.mjesta * 100) + "%";
        bar.appendChild(fill);
        detail.appendChild(bar);

        // oblik
        var shapes = el("div", "rs-shapes");
        OBLICI.forEach(function (o) {
            var b = el("button", o === t.oblik ? "is-active" : "", OBLIK_NAZIV[o]);
            b.type = "button";
            b.addEventListener("click", function () {
                if (t.oblik === o) return;
                var max = MAX_MJESTA[o];
                if (t.mjesta > max) {
                    if (u > max) { say("Za " + OBLIK_NAZIV[o].toLowerCase() + " sto najviše " + max + " mjesta, a ovdje je smješteno " + u + " osoba.", true); return; }
                    t.mjesta = max;
                }
                t.oblik = o; if (o === "okrugli") t.rot = 0;
                setDirty(true); renderAll();
            });
            shapes.appendChild(b);
        });
        detail.appendChild(shapes);

        // velicina
        function slider(label, get, set, max) {
            var f = el("div", "rs-field"); f.appendChild(el("span", null, label));
            var r = el("input"); r.type = "range"; r.min = "50"; r.max = String(max); r.step = "5";
            r.value = String(Math.round(get() * 100)); r.className = "rs-range";
            r.addEventListener("input", function () {
                set(clamp(+r.value / 100, 0.5, max / 100));
                renderHall(); setDirty(true);
            });
            f.appendChild(r); detail.appendChild(f);
        }
        if (t.oblik === "pravougaoni") {
            // pravougaoni: dužina i širina posebno (u odnosu na rotaciju)
            slider(tr("Dužina"), function () { return t.skala; }, function (v) { t.skala = v; }, 300);
            slider(tr("Širina"), function () { return t.skalaV; }, function (v) { t.skalaV = v; }, 300);
        } else {
            slider(tr("Veličina"), function () { return t.skala; }, function (v) { t.skala = v; }, 200);
        }

        // rotacija (samo pravougaoni)
        if (t.oblik === "pravougaoni") {
            var rot = el("button", "ui-btn ui-btn-ghost ui-btn-sm ui-btn-block rs-rotate", tr("↻ Rotiraj za 90°"));
            rot.type = "button";
            rot.addEventListener("click", function () {
                t.rot = t.rot === 90 ? 0 : 90;
                t.x = clamp(t.x, 6, 94); t.y = clamp(t.y, 8, 92);
                setDirty(true); renderAll();
            });
            detail.appendChild(rot);
        }

        // naziv
        var f1 = el("label", "rs-field"); f1.appendChild(el("span", null, tr("Naziv stola")));
        var input = el("input"); input.type = "text"; input.maxLength = 60; input.value = t.naziv;
        input.addEventListener("input", function () {
            t.naziv = input.value || "Sto";
            title.textContent = t.naziv;
            renderHall(); setDirty(true);
        });
        f1.appendChild(input); detail.appendChild(f1);

        // broj mjesta
        var f2 = el("div", "rs-field"); f2.appendChild(el("span", null, tr("Broj mjesta")));
        var step = el("div", "rs-step");
        var minus = el("button", null, "−"), plus = el("button", null, "+"), out = el("output", null, String(t.mjesta));
        minus.type = plus.type = "button";
        function setSeats(n) {
            n = clamp(n, Math.max(1, u), MAX_MJESTA[t.oblik]);
            if (n === t.mjesta) { if (n === u) say(tr("Ne možete smanjiti ispod broja smještenih osoba."), true); return; }
            t.mjesta = n; setDirty(true); renderAll();
        }
        minus.addEventListener("click", function () { setSeats(t.mjesta - 1); });
        plus.addEventListener("click", function () { setSeats(t.mjesta + 1); });
        step.appendChild(minus); step.appendChild(out); step.appendChild(plus);
        f2.appendChild(step); detail.appendChild(f2);

        // clanovi stola
        var members = guests.filter(function (g) { return g.sto === t.key; });
        var list = el("ul", "rs-members");
        if (!members.length) {
            list.appendChild(el("li", null, "Na ovom stolu još nema gostiju."));
        }
        members.forEach(function (g) {
            var li = el("li");
            li.appendChild(el("span", null, g.ime + (g.osoba > 1 ? " (+" + (g.osoba - 1) + ")" : "")));
            var x = el("button", null, "×"); x.type = "button"; x.setAttribute("aria-label", tr("Ukloni gosta sa stola"));
            x.addEventListener("click", function () { g.sto = null; setDirty(true); renderAll(); });
            li.appendChild(x);
            list.appendChild(li);
        });
        detail.appendChild(list);

        var del = el("button", "ui-btn ui-btn-danger ui-btn-sm ui-btn-block", tr("Ukloni sto"));
        del.type = "button";
        del.addEventListener("click", function () {
            if (!confirm(trf("Ukloniti „{0}“? Gosti sa tog stola ostaju bez stola.", t.naziv))) return;
            guests.forEach(function (g) { if (g.sto === t.key) g.sto = null; });
            tables = tables.filter(function (x) { return x !== t; });
            selected = null; setDirty(true); renderAll();
        });
        detail.appendChild(del);
    }

    function renderAll() { renderHall(); renderGuests(); renderDetail(); }

    // ---------- Stolovi: pravljenje ----------
    function nextName() {
        var n = tables.length + 1;
        var names = tables.map(function (t) { return t.naziv; });
        while (names.indexOf(tr("Sto") + " " + n) >= 0) n++;
        return tr("Sto") + " " + n;
    }

    function newTable(preset, x, y) {
        var t = {
            key: "n" + (++seq), id: 0, naziv: nextName(),
            mjesta: clamp(preset.mjesta, 1, MAX_MJESTA[preset.oblik]), oblik: preset.oblik, rot: 0, skala: 1, skalaV: 1, x: x, y: y
        };
        tables.push(t);
        return t;
    }

    function freeSpot() {
        var i = tables.length;
        return { x: clamp(16 + (i % 4) * 22, 8, 92), y: clamp(22 + Math.floor(i / 4) * 28, 12, 88) };
    }

    function pointToPercent(cx, cy) {
        var r = hall.getBoundingClientRect();
        return {
            x: clamp((cx - r.left) / r.width * 100, 6, 94),
            y: clamp((cy - r.top) / r.height * 100, 8, 92)
        };
    }

    function inHall(cx, cy) {
        var r = hall.getBoundingClientRect();
        return cx >= r.left && cx <= r.right && cy >= r.top && cy <= r.bottom;
    }

    // Novi sto na mjestu gdje je gost pusten na praznu salu
    function createTableForGuest(g, cx, cy) {
        if (is3d) { say(tr("U 3D prikazu ne mogu se praviti stolovi. Prebacite se na 2D."), true); return; }
        var pos = pointToPercent(cx, cy);
        var mjesta = Math.min(16, Math.max(8, g.osoba));
        var t = newTable({ oblik: "okrugli", mjesta: mjesta }, pos.x, pos.y);
        selected = t.key;
        assign(g.id, t.key);
        say(trf("Napravljen je novi sto „{0}“.", t.naziv));
    }

    // ---------- Dodjela ----------
    function assign(guestId, key) {
        var g = guestById(guestId);
        var t = byKey(key);
        if (!g || !t) return;

        var slobodno = t.mjesta - used(t.key) + (g.sto === t.key ? g.osoba : 0);
        if (g.osoba > slobodno) {
            say("„" + t.naziv + "“ nema dovoljno mjesta (potrebno " + g.osoba + ", slobodno " + Math.max(0, slobodno) + ").", true);
            return;
        }
        g.sto = key; picked = null; selected = key;
        setDirty(true); renderAll();
    }

    function tableAt(x, y) {
        var list = document.elementsFromPoint(x, y);
        for (var i = 0; i < list.length; i++) {
            if (list[i].classList && list[i].classList.contains("rs-table")) return list[i];
        }
        return null;
    }

    // ---------- Prevlacenje gostiju ----------
    function bindChip(chip, g) {
        chip.addEventListener("pointerdown", function (e) {
            if (e.button !== undefined && e.button !== 0) return;
            e.preventDefault();
            var sx = e.clientX, sy = e.clientY, ghost = null, over = null;

            function move(ev) {
                if (!ghost) {
                    if (Math.hypot(ev.clientX - sx, ev.clientY - sy) < 5) return;
                    ghost = chip.cloneNode(true);
                    ghost.classList.add("rs-ghost");
                    document.body.appendChild(ghost);
                    hall.classList.add("is-drop");
                }
                ghost.style.left = ev.clientX + "px";
                ghost.style.top = ev.clientY + "px";

                var t = tableAt(ev.clientX, ev.clientY);
                if (over && over !== t) over.classList.remove("is-over");
                over = t;
                if (over) over.classList.add("is-over");
            }

            function up(ev) {
                window.removeEventListener("pointermove", move);
                window.removeEventListener("pointerup", up);
                window.removeEventListener("pointercancel", up);

                if (ghost) {
                    ghost.remove();
                    if (over) over.classList.remove("is-over");
                    hall.classList.remove("is-drop");

                    var t = tableAt(ev.clientX, ev.clientY);
                    if (t) assign(g.id, t.getAttribute("data-key"));
                    else if (inHall(ev.clientX, ev.clientY)) createTableForGuest(g, ev.clientX, ev.clientY);
                } else {
                    picked = picked === g.id ? null : g.id;
                    renderGuests(); markTargets();
                }
            }

            window.addEventListener("pointermove", move);
            window.addEventListener("pointerup", up);
            window.addEventListener("pointercancel", up);
        });
    }

    // Dodir: izabran gost pa klik na praznu salu pravi novi sto
    hall.addEventListener("click", function (e) {
        if (!picked) return;
        if (e.target.closest && e.target.closest(".rs-table")) return;
        var g = guestById(picked);
        if (g) createTableForGuest(g, e.clientX, e.clientY);
    });

    // Ujednačena promjena (ugaona ručka, pinch): okrugli samo skala, pravougaoni obje
    function applyUniform(t, g, f) {
        if (t.oblik === "pravougaoni") {
            t.skala = clamp(g.s0 * f, 0.5, 3);
            t.skalaV = clamp((g.sv0 || 1) * f, 0.5, 3);
        } else {
            t.skala = clamp(g.s0 * f, 0.5, 2);
        }
    }

    // Brza promjena veličine bez ponovnog crtanja cijele sale (ne prekida dodir)
    function liveSize(t) {
        var n = stage.querySelector('[data-key="' + t.key + '"]');
        if (!n) return;
        var sz = dims(t);
        n.style.setProperty("--w", sz.w + "px");
        n.style.setProperty("--h", sz.h + "px");
        var seats = n.querySelectorAll(".rs-seat");
        for (var i = 0; i < seats.length; i++) {
            var p = seatPos(t, i);
            seats[i].style.setProperty("--sx", p[0].toFixed(1) + "px");
            seats[i].style.setProperty("--sy", p[1].toFixed(1) + "px");
        }
    }

    // ---------- Stolovi: prevlacenje, pinch i ručka za veličinu ----------
    var pts = {}, gesture = null;

    function ptsList() { return Object.keys(pts).map(function (k) { return pts[k]; }); }
    function pinchDist() {
        var a = ptsList();
        return a.length < 2 ? 0 : Math.hypot(a[0].x - a[1].x, a[0].y - a[1].y);
    }

    stage.addEventListener("pointerdown", function (e) {
        var node = e.target.closest(".rs-table");
        if (!node) return;
        e.preventDefault();

        var t = byKey(node.getAttribute("data-key"));
        if (!t) return;
        pts[e.pointerId] = { x: e.clientX, y: e.clientY };

        // dva prsta na istom stolu = promjena veličine
        if (gesture && gesture.type === "drag" && gesture.t === t && ptsList().length === 2 && !is3d) {
            gesture = { type: "pinch", t: t, d0: pinchDist() || 1, s0: t.skala, sv0: t.skalaV };
            selected = t.key;
            return;
        }
        if (gesture) return;

        if ((e.target.closest(".rs-grip-x") || e.target.closest(".rs-grip-y")) && !is3d) {
            var rr = node.getBoundingClientRect();
            var ccx = rr.left + rr.width / 2, ccy = rr.top + rr.height / 2;
            var horiz = !!e.target.closest(".rs-grip-x");
            gesture = {
                type: "gripaxis", t: t, id: e.pointerId, cx: ccx, cy: ccy, horiz: horiz,
                s0: t.skala, sv0: t.skalaV,
                d0: Math.max(10, horiz ? Math.abs(e.clientX - ccx) : Math.abs(e.clientY - ccy))
            };
            return;
        }

        if (e.target.closest(".rs-grip") && !is3d) {
            var r = node.getBoundingClientRect();
            var cx = r.left + r.width / 2, cy = r.top + r.height / 2;
            gesture = {
                type: "grip", t: t, id: e.pointerId, cx: cx, cy: cy, s0: t.skala, sv0: t.skalaV,
                d0: Math.max(10, Math.hypot(e.clientX - cx, e.clientY - cy))
            };
            return;
        }

        gesture = { type: "drag", t: t, node: node, id: e.pointerId, sx: e.clientX, sy: e.clientY, moved: false };
    });

    window.addEventListener("pointermove", function (ev) {
        if (pts[ev.pointerId]) { pts[ev.pointerId].x = ev.clientX; pts[ev.pointerId].y = ev.clientY; }
        if (!gesture) return;
        var g = gesture, t = g.t;

        if (g.type === "pinch") {
            var d = pinchDist();
            if (d > 0) { applyUniform(t, g, d / g.d0); g.moved = true; liveSize(t); }
        } else if (g.type === "grip" && ev.pointerId === g.id) {
            var dd = Math.hypot(ev.clientX - g.cx, ev.clientY - g.cy);
            applyUniform(t, g, dd / g.d0); g.moved = true; liveSize(t);
        } else if (g.type === "gripaxis" && ev.pointerId === g.id) {
            // vodoravno povlačenje mijenja ono što je na ekranu vodoravno (zavisi od rotacije)
            var dist = g.horiz ? Math.abs(ev.clientX - g.cx) : Math.abs(ev.clientY - g.cy);
            var f = dist / g.d0;
            var uDuzinu = g.horiz === (t.rot !== 90);
            if (uDuzinu) t.skala = clamp(g.s0 * f, 0.5, 3); else t.skalaV = clamp(g.sv0 * f, 0.5, 3);
            g.moved = true; liveSize(t);
        } else if (g.type === "drag" && ev.pointerId === g.id) {
            if (is3d) return;
            if (!g.moved && Math.hypot(ev.clientX - g.sx, ev.clientY - g.sy) < 4) return;
            g.moved = true;
            g.node.classList.add("is-dragging");
            var p = pointToPercent(ev.clientX, ev.clientY);
            t.x = p.x; t.y = p.y;
            g.node.style.left = t.x + "%";
            g.node.style.top = t.y + "%";
        }
    });

    function endPointer(ev) {
        var had = !!pts[ev.pointerId];
        delete pts[ev.pointerId];
        if (!gesture || !had) return;
        var g = gesture, t = g.t;

        if (g.type === "pinch") {
            if (ptsList().length < 2) { gesture = null; pts = {}; setDirty(true); renderAll(); }
            return;
        }
        if (ev.pointerId !== g.id) return;
        gesture = null;

        if (g.type === "grip" || g.type === "gripaxis") { if (g.moved) setDirty(true); renderAll(); return; }

        g.node.classList.remove("is-dragging");
        if (g.moved) { setDirty(true); renderAll(); return; }

        if (picked) { assign(picked, t.key); }
        else { selected = selected === t.key ? null : t.key; renderAll(); }
    }
    window.addEventListener("pointerup", endPointer);
    window.addEventListener("pointercancel", endPointer);

    // ---------- Zoom sale ----------
    var zoomOut = $("rsZoomOut"), zoomIn = $("rsZoomIn"), zoomVal = $("rsZoomVal");
    function applyZoom() {
        zoom = clamp(Math.round(zoom * 10) / 10, 0.4, 1.4);
        stage.style.setProperty("--z", zoom);
        zoomVal.textContent = Math.round(zoom * 100) + "%";
    }
    zoomOut.addEventListener("click", function () { zoom -= 0.1; applyZoom(); });
    zoomIn.addEventListener("click", function () { zoom += 0.1; applyZoom(); });
    applyZoom();

    // ---------- Meni "Dodaj sto" ----------
    function buildMenu() {
        menu.innerHTML = "";
        PRESETI.forEach(function (p) {
            if (p.grupa) { menu.appendChild(el("div", "rs-menu-title", p.grupa)); return; }
            var b = el("button");
            b.type = "button";
            b.appendChild(el("span", "rs-ico " + p.slika));
            b.appendChild(el("span", null, tr(p.naziv)));
            b.appendChild(el("small", null, p.mjesta + " " + tr("mjesta")));
            b.addEventListener("click", function () {
                var spot = freeSpot();
                var t = newTable(p, spot.x, spot.y);
                selected = t.key;
                closeMenu(); setDirty(true); renderAll();
            });
            menu.appendChild(b);
        });
    }
    function closeMenu() { menu.hidden = true; addBtn.setAttribute("aria-expanded", "false"); }
    addBtn.addEventListener("click", function (e) {
        e.stopPropagation();
        var open = menu.hidden;
        menu.hidden = !open;
        addBtn.setAttribute("aria-expanded", open ? "true" : "false");
    });
    document.addEventListener("click", function (e) {
        if (!menu.hidden && !menu.contains(e.target)) closeMenu();
    });
    document.addEventListener("keydown", function (e) { if (e.key === "Escape") closeMenu(); });
    buildMenu();

    // ---------- Alatke ----------
    $("rsAuto").addEventListener("click", function () {
        if (!tables.length) { say(tr("Prvo dodajte barem jedan sto."), true); return; }

        var free = guests.filter(function (g) { return !g.sto; })
                         .sort(function (a, b) { return b.osoba - a.osoba; });
        var left = 0;

        free.forEach(function (g) {
            var best = null, bestFree = 1e9;
            tables.forEach(function (t) {
                var f = t.mjesta - used(t.key);
                if (f >= g.osoba && f < bestFree) { best = t; bestFree = f; }
            });
            if (best) g.sto = best.key; else left++;
        });

        setDirty(true); renderAll();
        say(left ? trf("{0} gostiju nije smješteno. Dodajte stolove ili mjesta.", left) : tr("Svi gosti su raspoređeni."), !!left);
    });

    $("rsClear").addEventListener("click", function () {
        if (!confirm(tr("Skinuti sve goste sa stolova?"))) return;
        guests.forEach(function (g) { g.sto = null; });
        setDirty(true); renderAll();
    });

    document.querySelectorAll(".rs-view button").forEach(function (b) {
        b.addEventListener("click", function () {
            document.querySelectorAll(".rs-view button").forEach(function (x) { x.classList.remove("is-active"); });
            b.classList.add("is-active");
            is3d = b.getAttribute("data-view") === "3d";
            hall.classList.toggle("is-3d", is3d);
            renderHall(); markTargets();
            if (is3d) say(tr("U 3D prikazu stolovi se ne pomjeraju. Vratite se na 2D za premještanje."));
        });
    });

    // ---------- Ručno dodani gosti ----------
    var gForm = $("rsGuestForm"), gName = $("rsGuestName"), gCount = $("rsGuestCount");
    var gNum = 0;

    function setNum(n) {
        gNum = clamp(n, 0, 9);
        gCount.textContent = String(gNum);
        var tot = $("rsGuestTotal");
        if (tot) tot.textContent = tr("Ukupno za sto") + ": " + (1 + gNum) + " " + (gNum === 0 ? tr("osoba") : tr("osobe"));
    }
    setNum(0);
    $("rsGuestMinus").addEventListener("click", function () { setNum(gNum - 1); });
    $("rsGuestPlus").addEventListener("click", function () { setNum(gNum + 1); });

    gForm.addEventListener("submit", function (e) {
        e.preventDefault();
        var ime = gName.value.trim();
        if (!ime) { gName.focus(); return; }

        var btn = gForm.querySelector('button[type="submit"]');
        btn.disabled = true;

        fetch(data.addGuestUrl, {
            method: "POST",
            headers: { "Content-Type": "application/json", "RequestVerificationToken": token() },
            body: JSON.stringify({ ime: ime, osoba: 1 + gNum })
        }).then(function (res) {
            if (!res.ok) return res.text().then(function (t) { throw new Error(t || tr("Gost nije dodan.")); });
            return res.json();
        }).then(function (r) {
            guests.push({ id: r.id, ime: r.ime, osoba: r.osoba, sto: null, rucni: true });
            gName.value = ""; setNum(0);
            renderAll();
            say(trf("Gost „{0}“ je dodan. Prevucite ga na sto.", r.ime));
            gName.focus();
        }).catch(function (err) {
            say(err.message, true);
        }).then(function () { btn.disabled = false; });
    });

    function removeManual(g) {
        if (!confirm("Obrisati gosta „" + g.ime + "“?")) return;
        var url = data.removeGuestUrl.replace(/\/gost\/0\//, "/gost/" + g.id + "/");

        fetch(url, { method: "POST", headers: { "RequestVerificationToken": token() } })
            .then(function (res) {
                if (!res.ok) throw new Error(tr("Brisanje nije uspjelo."));
                guests = guests.filter(function (x) { return x !== g; });
                if (picked === g.id) picked = null;
                renderAll();
                say(tr("Gost je obrisan."));
            })
            .catch(function (err) { say(err.message, true); });
    }

    // ---------- Cuvanje ----------
    saveBtn.addEventListener("click", function () {
        saveBtn.disabled = true;
        saveBtn.textContent = tr("Čuvanje…");

        var body = {
            stolovi: tables.map(function (t) {
                return { kljuc: t.key, id: t.id || 0, naziv: t.naziv, mjesta: t.mjesta, x: t.x, y: t.y, oblik: t.oblik, rotacija: t.rot, skala: t.skala, skalaV: t.skalaV };
            }),
            gosti: guests.map(function (g) { return { gostId: g.id, sto: g.sto }; })
        };

        fetch(data.saveUrl, {
            method: "POST",
            headers: { "Content-Type": "application/json", "RequestVerificationToken": token() },
            body: JSON.stringify(body)
        }).then(function (res) {
            if (!res.ok) return res.text().then(function (t) { throw new Error(t || tr("Čuvanje nije uspjelo.")); });
            return res.json();
        }).then(function (r) {
            r.stolovi.forEach(function (m) { var t = byKey(m.kljuc); if (t) t.id = m.id; });
            setDirty(false);
            say(tr("Raspored je sačuvan."));
        }).catch(function (err) {
            say(err.message, true);
        }).then(function () {
            saveBtn.disabled = false;
            saveBtn.textContent = tr("Sačuvaj raspored");
        });
    });

    window.addEventListener("beforeunload", function (e) {
        if (dirty) { e.preventDefault(); e.returnValue = ""; }
    });

    renderAll();
})();
