/* Fotografije i muzika pozivnice (stranica /moje-pozivnice/{id}/mediji) */
(function () {
    "use strict";

    var tr = window.tr || function (s) { return s; };
    var root = document.getElementById("pzMedia");
    if (!root) return;

    var base = root.getAttribute("data-base");
    var max = parseInt(root.getAttribute("data-max"), 10) || 8;
    var tokenEl = document.querySelector('input[name="__RequestVerificationToken"]');
    var token = tokenEl ? tokenEl.value : "";

    function post(url, fd) {
        fd = fd || new FormData();
        fd.append("__RequestVerificationToken", token);
        return fetch(url, { method: "POST", body: fd, credentials: "same-origin" })
            .then(function (r) {
                return r.json().catch(function () { return { ok: false, poruka: "Greška na serveru." }; })
                    .then(function (j) { j.status = r.status; return j; });
            });
    }

    function say(el, text, bad) {
        if (!el) return;
        el.textContent = text ? tr(text) : "";
        el.style.color = bad ? "#a23b2e" : "";
    }

    // ---------- Fotografije ----------
    var grid = document.getElementById("pzPhotos");
    var input = document.getElementById("pzFotoInput");
    var countEl = document.getElementById("pzFotoCount");
    var fotoMsg = document.getElementById("pzFotoMsg");

    function count() { return grid.querySelectorAll(".pz-photo").length; }
    function refresh() { countEl.textContent = count(); }

    function downscale(file) {
        return new Promise(function (resolve) {
            // Mala slika (do 900 KB) ostaje kakva jeste
            if (file.size < 900 * 1024 && /jpeg|webp/.test(file.type)) { resolve(file); return; }
            var url = URL.createObjectURL(file);
            var img = new Image();
            img.onload = function () {
                var k = Math.min(1, 1600 / Math.max(img.naturalWidth, img.naturalHeight));
                var w = Math.round(img.naturalWidth * k), h = Math.round(img.naturalHeight * k);
                var c = document.createElement("canvas");
                c.width = w; c.height = h;
                var ctx = c.getContext("2d");
                ctx.fillStyle = "#fff"; ctx.fillRect(0, 0, w, h);
                ctx.drawImage(img, 0, 0, w, h);
                URL.revokeObjectURL(url);
                c.toBlob(function (b) { resolve(b || file); }, "image/jpeg", 0.86);
            };
            img.onerror = function () { URL.revokeObjectURL(url); resolve(file); };
            img.src = url;
        });
    }

    function addFigure(id, url) {
        var f = document.createElement("figure");
        f.className = "pz-photo";
        f.setAttribute("data-id", id);
        var im = document.createElement("img");
        im.src = url; im.alt = ""; im.loading = "lazy";
        var b = document.createElement("button");
        b.type = "button"; b.className = "pz-photo-del";
        b.setAttribute("data-del", id);
        b.setAttribute("aria-label", tr("Obriši fotografiju"));
        b.textContent = "×";
        f.appendChild(im); f.appendChild(b);
        grid.appendChild(f);
    }

    function uploadList(files) {
        var list = Array.prototype.slice.call(files);
        var chain = Promise.resolve();
        list.forEach(function (file) {
            chain = chain.then(function () {
                if (count() >= max) { say(fotoMsg, "Možete dodati najviše 8 fotografija.", true); return; }
                say(fotoMsg, "Otpremanje…");
                return downscale(file).then(function (blob) {
                    var fd = new FormData();
                    fd.append("fajl", blob, "foto.jpg");
                    return post(base + "/foto", fd);
                }).then(function (j) {
                    if (j && j.ok) { addFigure(j.id, j.url); refresh(); say(fotoMsg, "Fotografija je dodata ✓"); }
                    else say(fotoMsg, (j && j.poruka) || "Greška pri otpremanju.", true);
                });
            });
        });
        return chain;
    }

    input.addEventListener("change", function () {
        if (input.files && input.files.length) uploadList(input.files).then(function () { input.value = ""; });
    });

    grid.addEventListener("click", function (e) {
        var b = e.target.closest("[data-del]");
        if (!b) return;
        if (!window.confirm(tr("Obrisati ovu fotografiju?"))) return;
        var id = b.getAttribute("data-del");
        post(base + "/foto/" + id + "/obrisi").then(function (j) {
            if (j && j.ok) {
                var fig = b.closest(".pz-photo");
                if (fig) fig.remove();
                refresh(); say(fotoMsg, "");
            } else say(fotoMsg, (j && j.poruka) || "Greška.", true);
        });
    });

    // ---------- Muzika ----------
    var tracks = document.getElementById("pzTracks");
    var muzMsg = document.getElementById("pzMuzMsg");
    var player = new Audio();
    player.preload = "none";
    var playingBtn = null;

    function stopPreview() {
        player.pause();
        if (playingBtn) { playingBtn.textContent = "▶"; playingBtn = null; }
    }
    player.addEventListener("ended", stopPreview);

    tracks.addEventListener("click", function (e) {
        var b = e.target.closest(".pz-play");
        if (!b) return;
        e.preventDefault(); e.stopPropagation(); // dugme je unutar label-a, ne mijenja izbor
        var src = b.getAttribute("data-src");
        if (!src) return;
        if (playingBtn === b) { stopPreview(); return; }
        stopPreview();
        player.src = src;
        player.volume = 0.7;
        player.play().then(function () { b.textContent = "❚❚"; playingBtn = b; }).catch(function () { });
    });

    tracks.addEventListener("change", function (e) {
        var r = e.target;
        if (!r || r.name !== "muzika") return;
        var fd = new FormData();
        fd.append("izbor", r.value);
        say(muzMsg, "Čuvanje…");
        post(base + "/muzika", fd).then(function (j) {
            say(muzMsg, j && j.ok ? "Sačuvano ✓" : ((j && j.poruka) || "Greška."), !(j && j.ok));
        });
    });

    var muzInput = document.getElementById("pzMuzInput");
    var prava = document.getElementById("pzPrava");
    var mojaRed = document.getElementById("pzMojaRed");
    var mojaPlay = document.getElementById("pzMojaPlay");
    var ukloni = document.getElementById("pzMojaUkloni");

    muzInput.addEventListener("click", function (e) {
        if (!prava.checked) {
            e.preventDefault();
            say(muzMsg, "Prvo potvrdite da imate pravo korištenja pjesme.", true);
        }
    });

    muzInput.addEventListener("change", function () {
        var f = muzInput.files && muzInput.files[0];
        if (!f) return;
        if (!prava.checked) { say(muzMsg, "Prvo potvrdite da imate pravo korištenja pjesme.", true); muzInput.value = ""; return; }
        if (f.size > 10 * 1024 * 1024) { say(muzMsg, "Pjesma je prevelika (najviše 10 MB).", true); muzInput.value = ""; return; }
        var fd = new FormData();
        fd.append("fajl", f);
        fd.append("potvrdaPrava", "true");
        say(muzMsg, "Otpremanje…");
        post(base + "/muzika/otpremi", fd).then(function (j) {
            muzInput.value = "";
            if (j && j.ok) {
                // Stranica se osvježava da prikaže novi red i izbor
                window.location.reload();
            } else say(muzMsg, (j && j.poruka) || "Greška pri otpremanju.", true);
        });
    });

    if (ukloni) {
        ukloni.addEventListener("click", function () {
            if (!window.confirm(tr("Ukloniti vašu pjesmu?"))) return;
            post(base + "/muzika/ukloni").then(function (j) {
                if (j && j.ok) window.location.reload();
                else say(muzMsg, (j && j.poruka) || "Greška.", true);
            });
        });
    }
})();
