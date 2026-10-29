(function (global) {
    "use strict";

    var reduced = document.documentElement.classList.contains("reduce-motion");

    var SHAPES = {
        heart: '<svg viewBox="0 0 24 24" fill="currentColor"><path d="M12 21.35l-1.45-1.32C5.4 15.36 2 12.28 2 8.5 2 5.42 4.42 3 7.5 3c1.74 0 3.41.81 4.5 2.09C13.09 3.81 14.76 3 16.5 3 19.58 3 22 5.42 22 8.5c0 3.78-3.4 6.86-8.55 11.54L12 21.35z"/></svg>',
        envelope: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.4" stroke-linejoin="round" stroke-linecap="round"><rect x="2.5" y="5" width="19" height="14" rx="2"/><path d="M3 7l9 6.5L21 7"/></svg>',
        sparkle: '<svg viewBox="0 0 24 24" fill="currentColor"><path d="M12 1.5l2.3 7.2 7.2 2.3-7.2 2.3L12 20.5l-2.3-7.2L2.5 11l7.2-2.3z"/></svg>',
        star: '<svg viewBox="0 0 24 24" fill="currentColor"><path d="M12 2.5l2.9 6.3 6.9.8-5.1 4.7 1.4 6.8L12 17.6 5.9 21.1l1.4-6.8L2.2 9.6l6.9-.8z"/></svg>',
        flower: '<svg viewBox="0 0 24 24" fill="currentColor"><g><ellipse cx="12" cy="6.2" rx="3.1" ry="4.6"/><ellipse cx="12" cy="6.2" rx="3.1" ry="4.6" transform="rotate(72 12 12)"/><ellipse cx="12" cy="6.2" rx="3.1" ry="4.6" transform="rotate(144 12 12)"/><ellipse cx="12" cy="6.2" rx="3.1" ry="4.6" transform="rotate(216 12 12)"/><ellipse cx="12" cy="6.2" rx="3.1" ry="4.6" transform="rotate(288 12 12)"/></g><circle cx="12" cy="12" r="2.3" fill="#fff" fill-opacity=".55"/></svg>',
        leaf: '<svg viewBox="0 0 24 24" fill="currentColor"><path d="M12 1.5C5.5 6.5 4.5 14 12 22.5 19.5 14 18.5 6.5 12 1.5z"/><path d="M12 6v15" stroke="#fff" stroke-opacity=".5" stroke-width=".9" fill="none"/></svg>',
        petal: '<svg viewBox="0 0 24 24" fill="currentColor"><path d="M12 3c4.5 2.5 7 7 5 12-1.2 3-3.2 5-5 6-1.8-1-3.8-3-5-6-2-5 .5-9.5 5-12z"/></svg>',
        ring: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5"><circle cx="9" cy="15" r="5.5"/><circle cx="15" cy="15" r="5.5"/><path d="M12 2.5l1.8 2.4L12 7.3 10.2 4.9z" fill="currentColor"/></svg>',
        diamond: '<svg viewBox="0 0 24 24" fill="currentColor"><path d="M12 2l8.5 10L12 22 3.5 12z"/></svg>',
        note: '<svg viewBox="0 0 24 24" fill="currentColor"><path d="M9 3.2v12.3A3.6 3.6 0 1 0 11 18.7V8.2l8-1.9v7.2a3.6 3.6 0 1 0 2 3.2V2z"/></svg>',
        dot: '<svg viewBox="0 0 24 24" fill="currentColor"><circle cx="12" cy="12" r="6"/></svg>'
    };

    function rand(a, b) { return a + Math.random() * (b - a); }

    function clean(kinds) {
        var list = (kinds || []).filter(function (k) { return SHAPES[k]; });
        return list.length ? list : ["heart"];
    }

    /**
     * Pokrece beskonacno plutanje ikonica u zadatom kontejneru.
     * opts: kinds[], colors[], count, min, max, opacity[min,max], dir: "up" | "down" | "mixed"
     */
    function mount(container, opts) {
        if (!container || reduced) return;
        opts = opts || {};

        var kinds = clean(opts.kinds);
        var colors = opts.colors && opts.colors.length ? opts.colors : ["currentColor"];
        var small = global.innerWidth < 700;
        var count = opts.count || (small ? 9 : 16);
        var min = opts.min || 16, max = opts.max || 44;
        var op = opts.opacity || [0.12, 0.3];
        var dir = opts.dir || "up";

        var frag = document.createDocumentFragment();

        for (var i = 0; i < count; i++) {
            var el = document.createElement("span");
            var size = rand(min, max);
            var depth = (size - min) / (max - min); // 0 = daleko, 1 = blizu
            var dur = rand(26, 52) - depth * 8;
            var down = dir === "down" || (dir === "mixed" && i % 2 === 0);

            el.className = "fl" + (down ? " fl-down" : "");
            var x = rand(-2, 97);
            if (opts.sides && global.innerWidth > 800) x = i % 2 ? rand(1, 24) : rand(74, 96);
            el.style.left = x.toFixed(1) + "%";
            el.style.width = el.style.height = size.toFixed(0) + "px";
            el.style.color = colors[i % colors.length];
            el.style.setProperty("--dur", dur.toFixed(1) + "s");
            el.style.setProperty("--delay", "-" + (opts.sides ? ((i / count) * dur + rand(0, dur / count)) : rand(0, dur)).toFixed(1) + "s");
            el.style.setProperty("--o", rand(op[0], op[1]).toFixed(2));
            el.style.setProperty("--sway", rand(5, 10).toFixed(1) + "s");
            el.style.setProperty("--sd", "-" + rand(0, 8).toFixed(1) + "s");
            el.style.setProperty("--amp", rand(14, 44).toFixed(0) + "px");
            el.style.setProperty("--rot", rand(8, 30).toFixed(0) + "deg");
            if (i % 4 === 0) el.style.filter = "blur(" + (depth > 0.6 ? 2.5 : 1.2) + "px)";

            var useImage = opts.image && i % 2 === 0;
            el.innerHTML = '<i class="fl-i">' + (useImage
                ? '<img src="' + opts.image + '" alt="">'
                : SHAPES[kinds[i % kinds.length]]) + "</i>";
            frag.appendChild(el);
        }

        container.appendChild(frag);
    }

    /** Eksplozija ikonica iz sredine elementa (konfeti). */
    function burst(origin, opts) {
        if (reduced || !origin || !origin.getBoundingClientRect || !origin.animate) return;
        opts = opts || {};

        var rect = origin.getBoundingClientRect();
        var cx = rect.left + rect.width / 2;
        var cy = rect.top + rect.height / 2;
        var cs = global.getComputedStyle(origin);
        var colors = opts.colors || [
            cs.getPropertyValue("--c-main").trim() || "#c9a961",
            cs.getPropertyValue("--c-accent").trim() || "#e8c9c4",
            "#ffffff"
        ];
        var kinds = clean(opts.kinds || ["heart", "sparkle", "star", "diamond", "dot"]);
        var n = opts.count || 34;

        var layer = document.createElement("div");
        layer.className = "fl-burst";
        document.body.appendChild(layer);

        for (var i = 0; i < n; i++) {
            var el = document.createElement("span");
            var size = rand(10, 26);
            el.style.cssText = "left:" + cx + "px;top:" + cy + "px;width:" + size + "px;height:" + size +
                "px;color:" + colors[i % colors.length];
            el.innerHTML = SHAPES[kinds[i % kinds.length]];
            layer.appendChild(el);

            var angle = rand(0, Math.PI * 2);
            var dist = rand(110, 340);
            var dx = Math.cos(angle) * dist;
            var dy = Math.sin(angle) * dist * 0.8 - 70;

            el.animate([
                { transform: "translate(-50%,-50%) scale(.3) rotate(0deg)", opacity: 1 },
                { transform: "translate(calc(-50% + " + dx + "px), calc(-50% + " + dy + "px)) scale(1) rotate(" + rand(-200, 200) + "deg)", opacity: 1, offset: 0.55 },
                { transform: "translate(calc(-50% + " + dx * 1.15 + "px), calc(-50% + " + (dy + 150) + "px)) scale(.8) rotate(" + rand(-320, 320) + "deg)", opacity: 0 }
            ], { duration: rand(1400, 2200), easing: "cubic-bezier(.2,.8,.3,1)", fill: "forwards" });
        }

        setTimeout(function () { layer.remove(); }, 2500);
    }

    global.Floaters = { mount: mount, burst: burst };
})(window);
