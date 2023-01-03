/* Bootstrap contextmenu
* 
* @package     bootstrap-contextmenu
* @version     1.0
* @author      Jochem Stoel
* @url         http://jochemstoel.github.io/
* @license     The MIT License (MIT)
*/

$.fn.contextmenu = function ($target, $callback, $pre) {

  function getMenuPosition($mouse, $direction, $scrollDir) {
    $win = $(window)[$direction]();
    $scroll = $(window)[$scrollDir]();
    $menu = $($target)[$direction]();
    $position = $mouse + $scroll;

    if ($mouse + $menu > $win && $menu < $mouse) {
      $position -= $menu;
    }

    return $position;
  }

  $(this).on("contextmenu", function ($e) {
    if ($e.ctrlKey) return;

    $("[menu=true],[sub_menu=true]").hide();

    // build menu
    if ($pre) $pre.call(this, $(this), $($target));

    // show
    $($target)
        .data("invokedOn", $($e.target))
        .show()
        .css({
          position: "absolute",
          left: getMenuPosition($e.clientX, 'width', 'scrollLeft'),
          top: getMenuPosition($e.clientY, 'height', 'scrollTop')
        })
        .off('click')
        .on('click', function ($e) {
          $(this).hide();
          var $invokedOn = $(this).data("invokedOn"), $selectedMenu = $($e.target);
          $callback.call(this, $invokedOn, $selectedMenu);
        });

    return false;
  });

  $(document).click(function () {
    $("[menu=true],[sub_menu=true]").hide();
  });
};

function show_context(target, id_menu, f_sel, f_pre, force_posx) {
  try {
    var el = $(target.target);
    $("[menu=true],[sub_menu=true]").hide();

    // elab menu
    if (f_pre) f_pre(el, $(id_menu));

    // show
    $(id_menu)
          .data("invokedOn", el)
              .show()
              .css({
                position: "absolute",
                left: menu_position(id_menu, force_posx ? force_posx : target.clientX, 'width', 'scrollLeft'),
                top: menu_position(id_menu, target.clientY, 'height', 'scrollTop')
              })
          .off('click')
          .on('click', function ($e) {
            $(this).hide();

            var $invokedOn = $(this).data("invokedOn");
            var $selectedMenu = $($e.target);

            f_sel(el, $selectedMenu);
          });
  } catch (e) { alert(e.message); }
}

function menu_position($target, $mouse, $direction, $scrollDir) {
  $win = $(window)[$direction]();
  $scroll = $(window)[$scrollDir]();
  $menu = $($target)[$direction]();
  $position = $mouse + $scroll;

  if ($mouse + $menu > $win && $menu < $mouse) {
    $position -= $menu;
  }

  return $position;
}

function selected_val(selected) {
  return $(selected).closest("[menu-value]").length > 0 ?
    $(selected).closest("[menu-value]").attr("menu-value") : selected.attr("value");
}