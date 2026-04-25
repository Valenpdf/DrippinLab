// =============================================================
//  home.js — Drippin
//  Responsabilidades:
//    1. Swiper del producto destacado
//    2. Menú de filtros (toggle)
//    3. Inserción responsiva de banners de Tanda en el grid
// =============================================================


// --- 1. Swiper del Producto Destacado ---
document.addEventListener("DOMContentLoaded", function () {

    if (document.querySelector('.swiper-destacado')) {
        new Swiper('.swiper-destacado', {
            direction: 'horizontal',
            slidesPerView: 1,
            spaceBetween: 10,
            autoplay: {
                delay: 3000,
                disableOnInteraction: false
            },
            loop: true,
            pagination: { el: '.swiper-pagination' },
            navigation: {
                nextEl: '.swiper-button-next',
                prevEl: '.swiper-button-prev',
            },
            scrollbar: { el: '.swiper-scrollbar' },
            breakpoints: {
                768: {
                    slidesPerView: 2,
                    spaceBetween: 10,
                },
            },
        });
    }

    // --- 2. Inicialización del sistema de Tandas responsivo ---
    TandasManager.init();

});


// --- 2. Menú de Filtros ---
function toggleMenu() {
    var menu = document.getElementById("menu-filtros");
    if (menu) {
        menu.classList.toggle("oculto");
    }
}


// =============================================================
//  TandasManager
//  Mueve los banners de tanda desde el #tanda-stash al
//  #productos-grid en la posición correcta según el breakpoint.
//
//  Breakpoints (alineados con cards.css):
//    < 768px  → 2 cols → trigger cada  4 productos (2 filas)
//    768–1023 → 3 cols → trigger cada  8 productos (~2 filas)  [tablet]
//    ≥ 1024px → 5 cols → trigger cada 10 productos (2 filas)
// =============================================================
var TandasManager = (function () {

    // Mapa de instancias Swiper activas, indexado por clase CSS.
    var _swiperInstances = {};

    // Último trigger aplicado (para evitar re-renders innecesarios en resize).
    var _lastTrigger = null;

    // Timer de debounce para el evento resize.
    var _resizeTimer = null;


    // ─── Determina el trigger según el ancho de ventana ───────────────────
    function _getTrigger() {
        var w = window.innerWidth;
        if (w < 768) return 4;   // mobile:  2 cols × 2 filas
        if (w < 1024) return 9;   // tablet:  3 cols, ~2 filas
        return 10;                // desktop: 5 cols × 2 filas
    }


    // ─── Destruye todas las instancias Swiper de tanda ────────────────────
    function _destroyTandaSwipers() {
        Object.keys(_swiperInstances).forEach(function (key) {
            var sw = _swiperInstances[key];
            if (sw && typeof sw.destroy === 'function') {
                sw.destroy(true, true);
            }
        });
        _swiperInstances = {};
    }


    // ─── Inicializa Swipers para todos los banners visibles en el grid ────
    function _initTandaSwipers() {
        var grid = document.getElementById('productos-grid');
        if (!grid) return;

        grid.querySelectorAll('.tanda-banner').forEach(function (banner) {
            var tandaId = banner.getAttribute('data-tanda-id');
            if (!tandaId) return;

            var swiperEl = banner.querySelector('.swiper-tanda-' + tandaId);
            var prevEl = banner.querySelector('.tanda-prev-' + tandaId);
            var nextEl = banner.querySelector('.tanda-next-' + tandaId);

            if (!swiperEl || _swiperInstances['tanda-' + tandaId]) return;

            _swiperInstances['tanda-' + tandaId] = new Swiper(swiperEl, {
                slidesPerView: 'auto',
                spaceBetween: 16,
                freeMode: true,
                grabCursor: true,
                autoplay: {
                    delay: 3500,
                    disableOnInteraction: false,
                    pauseOnMouseEnter: true,
                },
                loop: false,
                navigation: {
                    prevEl: prevEl,
                    nextEl: nextEl,
                },
            });
        });
    }


    // ─── Reposiciona los banners en el grid ───────────────────────────────
    function _reordenar() {
        var trigger = _getTrigger();

        // Salir si el trigger no cambió (resize sin cambio de breakpoint)
        if (trigger === _lastTrigger) return;
        _lastTrigger = trigger;

        var grid = document.getElementById('productos-grid');
        var stash = document.getElementById('tanda-stash');
        if (!grid || !stash) return;

        // 1. Destruir swipers antes de mover los DOM nodes
        _destroyTandaSwipers();

        // 2. Devolver todos los banners que ya estén en el grid al stash
        grid.querySelectorAll('.tanda-banner').forEach(function (banner) {
            stash.appendChild(banner);
        });

        // 3. Tomar las cards de producto y los banners disponibles
        var cards = Array.from(grid.querySelectorAll('.card-producto'));
        var tandas = Array.from(stash.querySelectorAll('.tanda-banner'));

        if (tandas.length === 0) return; // Sin tandas, nada que hacer

        var tandaIdx = 0;

        // 4. Después de cada bloque de `trigger` cards, insertar un banner
        cards.forEach(function (card, i) {
            var posicion = i + 1; // 1-indexado
            if (posicion % trigger === 0 && tandaIdx < tandas.length) {
                // Insertar el banner DESPUÉS de esta card
                card.parentNode.insertBefore(tandas[tandaIdx], card.nextSibling);
                tandaIdx++;
            }
        });

        // 5. Tandas sobrantes (si hay menos productos que el trigger): al final
        while (tandaIdx < tandas.length) {
            grid.appendChild(tandas[tandaIdx]);
            tandaIdx++;
        }

        // 6. Inicializar Swipers en los banners ya insertados
        _initTandaSwipers();
    }


    // ─── API pública ──────────────────────────────────────────────────────
    return {
        init: function () {
            // Primer render al cargar la página
            _reordenar();

            // Re-render con debounce al cambiar el tamaño de ventana
            window.addEventListener('resize', function () {
                clearTimeout(_resizeTimer);
                _resizeTimer = setTimeout(_reordenar, 200);
            });
        }
    };

}());