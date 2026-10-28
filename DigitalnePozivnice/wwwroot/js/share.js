/* Details: QR kod i izvorno dijeljenje */
(function () {
    "use strict";

    var tr = window.tr || function (s) { return s; };

    // ---------- QR kod ----------
    var canvas = document.getElementById("pzQr");
    if (canvas && window.QR) {
        var url = canvas.getAttribute("data-url");
        var qr = null;
        try { qr = QR.make(url); } catch (e) { qr = null; }

        var card = document.getElementById("pzQrCard");
        if (!qr) {
            if (card) card.hidden = true;
        } else {
            qr.toCanvas(canvas, 336);
            canvas.style.width = canvas.style.height = "168px";

            function save(name, blob) {
                var a = document.createElement("a");
                a.href = URL.createObjectURL(blob);
                a.download = name;
                document.body.appendChild(a);
                a.click();
                setTimeout(function () { URL.revokeObjectURL(a.href); a.remove(); }, 500);
            }

            var png = document.getElementById("pzQrPng");
            if (png) png.addEventListener("click", function () {
                // visoka rezolucija za štampu (oko 2000 px)
                var big = document.createElement("canvas");
                qr.toCanvas(big, 2000);
                big.toBlob(function (b) { save("qr-pozivnica.png", b); }, "image/png");
            });

            var svg = document.getElementById("pzQrSvg");
            if (svg) svg.addEventListener("click", function () {
                save("qr-pozivnica.svg", new Blob([qr.toSvg()], { type: "image/svg+xml" }));
            });
        }
    }

    // ---------- Izvorno dijeljenje (telefon) ----------
    var box = document.querySelector("[data-share-url]");
    var btn = document.getElementById("pzNativeShare");
    if (box && btn && navigator.share) {
        btn.hidden = false;
        btn.addEventListener("click", function () {
            navigator.share({
                title: box.getAttribute("data-share-title"),
                text: box.getAttribute("data-share-text"),
                url: box.getAttribute("data-share-url")
            }).catch(function () { });
        });
    }
})();
