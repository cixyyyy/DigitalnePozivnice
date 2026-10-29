/* Fotomat: dugme -> štampa se traka -> "Otvori pozivnicu" -> kolaž */
(function () {
    "use strict";
    var root = document.documentElement;
    var tpl = document.querySelector("[data-fotomat]");
    if (!tpl) return;

    var reduced = root.classList.contains("reduce-motion");
    var intro = tpl.querySelector("[data-ft-intro]");
    var board = tpl.querySelector("[data-ft-board]");

    function showBoard() {
        if (!board) return;
        var go = function () { board.classList.add("is-in"); };
        if (reduced) { go(); return; }
        requestAnimationFrame(function () { requestAnimationFrame(go); });
    }

    if (!intro || tpl.classList.contains("ft-nointro")) {
        if (intro) intro.remove();
        showBoard();
        return;
    }

    root.classList.add("intro-lock");

    var printBtns = intro.querySelectorAll("[data-ft-print]");
    var openBtn = intro.querySelector("[data-ft-open]");
    var printing = false, opened = false;

    function print() {
        if (printing) return;
        printing = true;
        intro.classList.add("is-printing");
        // isti dodir pokreće i muziku (data-ft-print je u popisu u intro.js)
        setTimeout(function () { intro.classList.add("is-printed"); }, reduced ? 120 : 5500);
    }

    function open() {
        if (opened) return;
        if (!printing) { print(); return; }
        if (!intro.classList.contains("is-printed")) return;
        opened = true;
        intro.classList.add("is-gone");
        root.classList.remove("intro-lock");
        window.scrollTo(0, 0);
        showBoard();
        document.dispatchEvent(new Event("intro:opened"));
        setTimeout(function () { intro.remove(); }, reduced ? 50 : 1100);
    }

    printBtns.forEach(function (b) { b.addEventListener("click", print); });
    if (openBtn) openBtn.addEventListener("click", open);
    var out = intro.querySelector(".ft-out");
    if (out) out.addEventListener("click", open);
})();
