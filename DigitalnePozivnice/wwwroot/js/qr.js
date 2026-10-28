/* Mali QR kodirnik (bajtni nacin, korekcija greske M, verzije 1-10).
   Radi potpuno lokalno, bez spoljnih biblioteka i bez interneta.
   Upotreba: var qr = QR.make("https://..."); qr.size; qr.get(x, y); qr.toSvg(); qr.toCanvas(canvas, px) */
(function (root) {
    "use strict";

    // [ukupno kodnih rijeci po bloku, podatkovnih po bloku, broj blokova] (nivo M)
    var BLOCKS = {
        1: [[26, 16, 1]], 2: [[44, 28, 1]], 3: [[70, 44, 1]], 4: [[50, 32, 2]], 5: [[67, 43, 2]],
        6: [[43, 27, 4]], 7: [[49, 31, 4]], 8: [[60, 38, 2], [61, 39, 2]], 9: [[58, 36, 3], [59, 37, 2]],
        10: [[69, 43, 4], [70, 44, 1]]
    };
    var ALIGN = { 1: [], 2: [6, 18], 3: [6, 22], 4: [6, 26], 5: [6, 30], 6: [6, 34], 7: [6, 22, 38], 8: [6, 24, 42], 9: [6, 26, 46], 10: [6, 28, 50] };

    // ---------- Galoisovo polje GF(256) ----------
    var EXP = new Array(512), LOG = new Array(256);
    (function () {
        var x = 1;
        for (var i = 0; i < 255; i++) { EXP[i] = x; LOG[x] = i; x <<= 1; if (x & 256) x ^= 0x11d; }
        for (var j = 255; j < 512; j++) EXP[j] = EXP[j - 255];
    })();
    function gmul(a, b) { return (a === 0 || b === 0) ? 0 : EXP[LOG[a] + LOG[b]]; }

    function rsGenerator(n) {
        var g = [1];
        for (var i = 0; i < n; i++) {
            var next = new Array(g.length + 1).fill(0);
            for (var j = 0; j < g.length; j++) {
                next[j] ^= g[j];
                next[j + 1] ^= gmul(g[j], EXP[i]);
            }
            g = next;
        }
        return g;
    }
    function rsEncode(data, ecLen) {
        var gen = rsGenerator(ecLen);
        var res = data.concat(new Array(ecLen).fill(0));
        for (var i = 0; i < data.length; i++) {
            var c = res[i];
            if (c !== 0) for (var j = 0; j < gen.length; j++) res[i + j] ^= gmul(gen[j], c);
        }
        return res.slice(data.length);
    }

    function utf8(str) {
        var out = [];
        var s = unescape(encodeURIComponent(str));
        for (var i = 0; i < s.length; i++) out.push(s.charCodeAt(i));
        return out;
    }

    function dataCapacity(v) { return BLOCKS[v].reduce(function (a, b) { return a + b[1] * b[2]; }, 0); }

    function buildCodewords(bytes, v) {
        var bits = [];
        function put(val, len) { for (var i = len - 1; i >= 0; i--) bits.push((val >>> i) & 1); }
        put(4, 4);
        put(bytes.length, v < 10 ? 8 : 16);
        bytes.forEach(function (b) { put(b, 8); });
        var cap = dataCapacity(v) * 8;
        for (var t = 0; t < 4 && bits.length < cap; t++) bits.push(0);
        while (bits.length % 8) bits.push(0);
        var words = [];
        for (var i = 0; i < bits.length; i += 8) {
            var w = 0; for (var k = 0; k < 8; k++) w = (w << 1) | bits[i + k];
            words.push(w);
        }
        var pad = [0xec, 0x11], p = 0;
        while (words.length < dataCapacity(v)) words.push(pad[p++ & 1]);

        // blokovi + Reed-Solomon
        var blocks = [], pos = 0, ecLen = 0;
        BLOCKS[v].forEach(function (g) {
            for (var n = 0; n < g[2]; n++) {
                var d = words.slice(pos, pos + g[1]); pos += g[1];
                ecLen = g[0] - g[1];
                blocks.push({ data: d, ec: rsEncode(d, ecLen) });
            }
        });
        var out = [], maxData = Math.max.apply(null, blocks.map(function (b) { return b.data.length; }));
        for (var i2 = 0; i2 < maxData; i2++) blocks.forEach(function (b) { if (i2 < b.data.length) out.push(b.data[i2]); });
        for (var j = 0; j < ecLen; j++) blocks.forEach(function (b) { out.push(b.ec[j]); });
        return out;
    }

    function formatBits(mask) {
        var data = (0 << 3) | mask; // M = 00
        var rem = data << 10;
        for (var i = 14; i >= 10; i--) if ((rem >>> i) & 1) rem ^= 0x537 << (i - 10);
        return ((data << 10) | rem) ^ 0x5412;
    }
    function versionBits(v) {
        var rem = v << 12;
        for (var i = 17; i >= 12; i--) if ((rem >>> i) & 1) rem ^= 0x1f25 << (i - 12);
        return (v << 12) | rem;
    }

    function make(text) {
        var bytes = utf8(String(text));
        var v = 1;
        while (v <= 10 && Math.floor((dataCapacity(v) * 8 - 4 - (v < 10 ? 8 : 16)) / 8) < bytes.length) v++;
        if (bytes.length > 200 || v > 10) throw new Error("Tekst je predug za QR kod.");

        var size = 17 + 4 * v;
        var mod = [], fn = [];
        for (var y = 0; y < size; y++) { mod.push(new Array(size).fill(false)); fn.push(new Array(size).fill(false)); }
        function set(x, y, val, isFn) { if (x < 0 || y < 0 || x >= size || y >= size) return; mod[y][x] = !!val; if (isFn) fn[y][x] = true; }

        function finder(cx, cy) {
            for (var dy = -4; dy <= 4; dy++) for (var dx = -4; dx <= 4; dx++) {
                var d = Math.max(Math.abs(dx), Math.abs(dy));
                set(cx + dx, cy + dy, d !== 2 && d !== 4, true);
            }
        }
        finder(3, 3); finder(size - 4, 3); finder(3, size - 4);

        // poravnavajuci uzorci
        var ap = ALIGN[v];
        ap.forEach(function (ay) { ap.forEach(function (ax) {
            if ((ax === 6 && ay === 6) || (ax === 6 && ay === size - 7) || (ax === size - 7 && ay === 6)) return;
            for (var dy = -2; dy <= 2; dy++) for (var dx = -2; dx <= 2; dx++)
                set(ax + dx, ay + dy, Math.max(Math.abs(dx), Math.abs(dy)) !== 1, true);
        }); });

        // vremenski uzorci
        for (var i = 8; i < size - 8; i++) { set(i, 6, i % 2 === 0, true); set(6, i, i % 2 === 0, true); }
        // tamni modul
        set(8, size - 8, true, true);

        // rezervisi prostor za format i verziju
        for (var r = 0; r < 9; r++) { fn[8][r] = true; fn[r][8] = true; }
        for (var r2 = 0; r2 < 8; r2++) { fn[8][size - 1 - r2] = true; fn[size - 1 - r2][8] = true; }
        if (v >= 7) for (var a = 0; a < 6; a++) for (var b = 0; b < 3; b++) { fn[a][size - 11 + b] = true; fn[size - 11 + b][a] = true; }

        // podaci u cik-cak redoslijedu
        var cw = buildCodewords(bytes, v);
        var bitsArr = [];
        cw.forEach(function (w) { for (var k = 7; k >= 0; k--) bitsArr.push((w >>> k) & 1); });
        var bi = 0, up = true;
        for (var col = size - 1; col > 0; col -= 2) {
            if (col === 6) col--;
            for (var n = 0; n < size; n++) {
                var yy = up ? size - 1 - n : n;
                for (var c2 = 0; c2 < 2; c2++) {
                    var xx = col - c2;
                    if (fn[yy][xx]) continue;
                    mod[yy][xx] = bi < bitsArr.length ? bitsArr[bi++] === 1 : false;
                }
            }
            up = !up;
        }

        var MASKS = [
            function (x, y) { return (x + y) % 2 === 0; },
            function (x, y) { return y % 2 === 0; },
            function (x, y) { return x % 3 === 0; },
            function (x, y) { return (x + y) % 3 === 0; },
            function (x, y) { return (Math.floor(y / 2) + Math.floor(x / 3)) % 2 === 0; },
            function (x, y) { return (x * y) % 2 + (x * y) % 3 === 0; },
            function (x, y) { return ((x * y) % 2 + (x * y) % 3) % 2 === 0; },
            function (x, y) { return ((x + y) % 2 + (x * y) % 3) % 2 === 0; }
        ];

        function applyMask(m) {
            var out = mod.map(function (row) { return row.slice(); });
            for (var y = 0; y < size; y++) for (var x = 0; x < size; x++)
                if (!fn[y][x] && MASKS[m](x, y)) out[y][x] = !out[y][x];
            // format
            var f = formatBits(m);
            function fb(i) { return ((f >>> i) & 1) === 1; }
            for (var i = 0; i <= 5; i++) out[i][8] = fb(i);
            out[7][8] = fb(6); out[8][8] = fb(7); out[8][7] = fb(8);
            for (var i2 = 9; i2 < 15; i2++) out[8][14 - i2] = fb(i2);
            for (var i3 = 0; i3 < 8; i3++) out[8][size - 1 - i3] = fb(i3);
            for (var i4 = 8; i4 < 15; i4++) out[size - 15 + i4][8] = fb(i4);
            out[size - 8][8] = true;
            // verzija
            if (v >= 7) {
                var vb = versionBits(v);
                for (var k = 0; k < 18; k++) {
                    var bit = ((vb >>> k) & 1) === 1, ra = Math.floor(k / 3), rb = k % 3 + size - 11;
                    out[ra][rb] = bit; out[rb][ra] = bit;
                }
            }
            return out;
        }

        function penalty(g) {
            var p = 0, x, y, run, prev;
            for (y = 0; y < size; y++) {
                run = 1; prev = g[y][0];
                for (x = 1; x < size; x++) { if (g[y][x] === prev) run++; else { if (run >= 5) p += run - 2; run = 1; prev = g[y][x]; } }
                if (run >= 5) p += run - 2;
            }
            for (x = 0; x < size; x++) {
                run = 1; prev = g[0][x];
                for (y = 1; y < size; y++) { if (g[y][x] === prev) run++; else { if (run >= 5) p += run - 2; run = 1; prev = g[y][x]; } }
                if (run >= 5) p += run - 2;
            }
            for (y = 0; y < size - 1; y++) for (x = 0; x < size - 1; x++) {
                var c = g[y][x];
                if (c === g[y][x + 1] && c === g[y + 1][x] && c === g[y + 1][x + 1]) p += 3;
            }
            var pat1 = [1, 0, 1, 1, 1, 0, 1, 0, 0, 0, 0], pat2 = [0, 0, 0, 0, 1, 0, 1, 1, 1, 0, 1];
            function match(get, pat) { for (var k = 0; k < 11; k++) if ((get(k) ? 1 : 0) !== pat[k]) return false; return true; }
            for (y = 0; y < size; y++) for (x = 0; x <= size - 11; x++) {
                if (match(function (k) { return g[y][x + k]; }, pat1)) p += 40;
                if (match(function (k) { return g[y][x + k]; }, pat2)) p += 40;
            }
            for (x = 0; x < size; x++) for (y = 0; y <= size - 11; y++) {
                if (match(function (k) { return g[y + k][x]; }, pat1)) p += 40;
                if (match(function (k) { return g[y + k][x]; }, pat2)) p += 40;
            }
            var dark = 0; for (y = 0; y < size; y++) for (x = 0; x < size; x++) if (g[y][x]) dark++;
            p += Math.floor(Math.abs(dark * 100 / (size * size) - 50) / 5) * 10;
            return p;
        }

        var best = null, bestP = Infinity;
        for (var m = 0; m < 8; m++) {
            var g = applyMask(m), pp = penalty(g);
            if (pp < bestP) { bestP = pp; best = g; }
        }

        var api = {
            size: size, version: v,
            get: function (x, y) { return x >= 0 && y >= 0 && x < size && y < size && best[y][x]; },
            toSvg: function (opts) {
                opts = opts || {}; var quiet = opts.quiet == null ? 4 : opts.quiet, color = opts.color || "#000000";
                var total = size + quiet * 2, d = "";
                for (var y = 0; y < size; y++) {
                    var x = 0;
                    while (x < size) {
                        if (best[y][x]) { var s = x; while (x < size && best[y][x]) x++; d += "M" + (s + quiet) + " " + (y + quiet) + "h" + (x - s) + "v1h-" + (x - s) + "z"; }
                        else x++;
                    }
                }
                return '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 ' + total + ' ' + total + '" shape-rendering="crispEdges">' +
                    '<rect width="' + total + '" height="' + total + '" fill="#ffffff"/><path d="' + d + '" fill="' + color + '"/></svg>';
            },
            toCanvas: function (canvas, px, opts) {
                opts = opts || {}; var quiet = opts.quiet == null ? 4 : opts.quiet, color = opts.color || "#000000";
                var total = size + quiet * 2, scale = Math.max(1, Math.floor((px || 512) / total));
                canvas.width = canvas.height = total * scale;
                var ctx = canvas.getContext("2d");
                ctx.fillStyle = "#ffffff"; ctx.fillRect(0, 0, canvas.width, canvas.height);
                ctx.fillStyle = color;
                for (var y = 0; y < size; y++) for (var x = 0; x < size; x++)
                    if (best[y][x]) ctx.fillRect((x + quiet) * scale, (y + quiet) * scale, scale, scale);
                return canvas;
            }
        };
        return api;
    }

    root.QR = { make: make };
})(window);
