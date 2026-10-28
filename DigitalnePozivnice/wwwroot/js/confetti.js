/* Pravi konfeti: canvas, gravitacija, otpor vazduha, treperenje.
   Confetti.fire(elementIliNull)  – top sa obje strane + eksplozija iz elementa */
(function (global) {
    "use strict";

    var reduced = document.documentElement.classList.contains("reduce-motion");
    var canvas = null, ctx = null, parts = [], raf = null, last = 0;

    function rand(a, b) { return a + Math.random() * (b - a); }

    function resize() {
        var dpr = Math.min(2, global.devicePixelRatio || 1);
        canvas.width = Math.round(global.innerWidth * dpr);
        canvas.height = Math.round(global.innerHeight * dpr);
        ctx.setTransform(dpr, 0, 0, dpr, 0, 0);
    }

    function ensure() {
        if (canvas) return;
        canvas = document.createElement("canvas");
        canvas.setAttribute("aria-hidden", "true");
        canvas.style.cssText = "position:fixed;inset:0;width:100%;height:100%;pointer-events:none;z-index:9999";
        document.body.appendChild(canvas);
        ctx = canvas.getContext("2d");
        resize();
        global.addEventListener("resize", resize);
    }

    function spawn(x, y, angle, spread, vmin, vmax, count, colors) {
        for (var i = 0; i < count; i++) {
            var a = angle + rand(-spread / 2, spread / 2);
            var v = rand(vmin, vmax);
            parts.push({
                x: x, y: y,
                vx: Math.cos(a) * v, vy: Math.sin(a) * v,
                w: rand(7, 14), h: rand(4, 8),
                rot: rand(0, 6.28), vr: rand(-9, 9),
                phase: rand(0, 6.28), speed: rand(6, 12),
                shape: Math.floor(rand(0, 3)),
                color: colors[Math.floor(Math.random() * colors.length)],
                life: 0, max: rand(3.6, 5.8)
            });
        }
    }

    function draw(p) {
        var flutter = Math.cos(p.life * p.speed + p.phase);
        ctx.save();
        ctx.translate(p.x, p.y);
        ctx.rotate(p.rot);
        ctx.scale(1, flutter);
        ctx.globalAlpha = Math.max(0, Math.min(1, p.max - p.life));
        ctx.fillStyle = p.color;
        if (p.shape === 0) {
            ctx.fillRect(-p.w / 2, -p.h / 2, p.w, p.h);
        } else if (p.shape === 1) {
            ctx.beginPath(); ctx.arc(0, 0, p.h * 0.7, 0, 6.283); ctx.fill();
        } else {
            ctx.fillRect(-p.w / 2, -1.5, p.w * 1.3, 3); // traka
        }
        ctx.restore();
    }

    function loop(t) {
        var dt = Math.min(0.033, (t - last) / 1000 || 0.016);
        last = t;
        ctx.clearRect(0, 0, global.innerWidth, global.innerHeight);

        var drag = Math.pow(0.12, dt);
        for (var i = parts.length - 1; i >= 0; i--) {
            var p = parts[i];
            p.vx *= drag; p.vy = p.vy * drag + 720 * dt;
            p.x += p.vx * dt; p.y += p.vy * dt;
            p.rot += p.vr * dt; p.life += dt;
            if (p.life > p.max || p.y > global.innerHeight + 40) { parts.splice(i, 1); continue; }
            draw(p);
        }

        if (parts.length) {
            raf = requestAnimationFrame(loop);
        } else {
            raf = null;
            global.removeEventListener("resize", resize);
            canvas.remove(); canvas = null; ctx = null;
        }
    }

    function colorsFrom(el) {
        var cs = el ? global.getComputedStyle(el) : null;
        var main = cs && cs.getPropertyValue("--c-main").trim();
        var acc = cs && cs.getPropertyValue("--c-accent").trim();
        return [main || "#e3c27e", acc || "#e8c9c4", "#ffffff", "#f4d98a", "#ff9aae", "#9ad7ff"];
    }

    function fire(origin) {
        if (reduced) return;
        ensure();
        var colors = colorsFrom(document.querySelector(".inv") || origin);
        var w = global.innerWidth, h = global.innerHeight;
        var mobile = w < 700;
        var n = mobile ? 55 : 90;

        // topovi iz donjih uglova
        spawn(0, h, -Math.PI / 3.1, 0.55, 900, 1500, n, colors);
        spawn(w, h, -Math.PI + Math.PI / 3.1, 0.55, 900, 1500, n, colors);

        // eksplozija iz elementa
        if (origin && origin.getBoundingClientRect) {
            var r = origin.getBoundingClientRect();
            spawn(r.left + r.width / 2, r.top + r.height / 2, -Math.PI / 2, Math.PI * 1.1, 350, 900, mobile ? 40 : 70, colors);
        }

        // druga runda, malo kasnije
        setTimeout(function () {
            if (!canvas) return;
            spawn(0, h, -Math.PI / 3.4, 0.5, 800, 1300, 35, colors);
            spawn(w, h, -Math.PI + Math.PI / 3.4, 0.5, 800, 1300, 35, colors);
        }, 380);

        if (!raf) { last = performance.now(); raf = requestAnimationFrame(loop); }
    }

    global.Confetti = { fire: fire };
})(window);
