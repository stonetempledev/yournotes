var __dragscroll_move = false;
function bind_drag($container, $scroller) {
    var $window = $(window), x = 0, y = 0;
    var x2 = 0, y2 = 0, t = 0;

    $container.on("mousedown touchstart", down);
    $container.on("click", preventDefault);
    $scroller.on("mousewheel", horizontalMouseWheel); // prevent macbook trigger prev/next page while scrolling

    function down(evt) {
        if (evt.button === 0) {
            t = Date.now(); x = x2 = evt.pageX; y = y2 = evt.pageY;
            __dragscroll_move = false;
            $container.addClass("down"); $window.on("mousemove touchmove", move); $window.on("mouseup touchend", up);
            evt.preventDefault();
        }
    }

    function move(evt) {

        if ($container.hasClass("down")) {
            __dragscroll_move = true;
            var _x = evt.pageX, _y = evt.pageY, deltaX = _x - x, deltaY = _y - y;

            $scroller[0].scrollTop -= deltaY; $scroller[0].scrollLeft -= deltaX;

            x = _x; y = _y;
        }

    }

    function up(evt) {
        $window.off("mousemove", move); $window.off("mouseup", up);
        t = 0; $container.removeClass("down");
    }

    function preventDefault(evt) {
        if (x2 !== evt.pageX || y2 !== evt.pageY) {
            evt.preventDefault();
            return false;
        }
    }

    function horizontalMouseWheel(evt) {
        evt = evt.originalEvent;
        var x = $scroller.scrollLeft();
        var max = $scroller[0].scrollWidth - $scroller[0].offsetWidth;
        var dir = (evt.deltaX || evt.wheelDeltaX);
        var stop = dir > 0 ? x >= max : x <= 0;
        if (stop && dir) evt.preventDefault();
    }

}

