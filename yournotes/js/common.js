function sel_onfocus(el) { $(el).select(); }
function do_post_back(action, args) {
  $("#__action").val(action ? action : ""); $("#__args").val(args ? args : "");
  $("form[mainform=true]").submit();
}
function is_url(str) {
  var pattern = new RegExp('^(https?:\\/\\/)?' + // protocol
  '((([a-z\\d]([a-z\\d-]*[a-z\\d])*)\\.?)+[a-z]{2,}|' + // domain name
  '((\\d{1,3}\\.){3}\\d{1,3}))' + // OR ip (v4) address
  '(\\:\\d+)?(\\/[-a-z\\d%_.~+]*)*' + // port and path
  '(\\?[;&a-z\\d%_.~+=-]*)?' + // query string
  '(\\#[-a-z\\d_]*)?$', 'i'); // fragment locator
  return pattern.test(str);
}

function get_param(name) {
  var results = new RegExp('[\?&]' + name + '=([^&#]*)').exec(window.location.href);
  return results == null ? "" : results[1];
}

function set_param(par, val, href) {
  var url = href == null ? window.location.href : href, hash = location.hash;
  url = url.replace(hash, '');
  if (url.indexOf(par + "=") >= 0) {
    var prefix = url.substring(0, url.indexOf(par + "=")), suffix = url.substring(url.indexOf(par + "="));
    suffix = suffix.substring(suffix.indexOf("=") + 1);
    suffix = (suffix.indexOf("&") >= 0) ? suffix.substring(suffix.indexOf("&")) : "";
    url = prefix + par + "=" + val + suffix;
  } else { if (url.indexOf("?") < 0) url += "?" + par + "=" + val; else url += "&" + par + "=" + val; }

  return url + hash;
}

function get_page() { return [location.protocol, '//', location.host, location.pathname].join(''); }

function is_mobile() { return navigator.userAgent.match(/(iPhone|iPod|iPad|Android|BlackBerry)/) != null; }

function check_bool(par, def) { if (par == null || typeof (par) == 'undefined') return def ? def : false; return par; }

function check_str(par, def) { if (par == null || typeof (par) == 'undefined') return def ? def : ""; return par; }

// formatUnicorn
//  "Hello, {name}, are you feeling {adjective}?".formatUnicorn({name:"Gabriel", adjective: "OK"})
String.prototype.formatUnicorn = String.prototype.formatUnicorn ||
function () {
  "use strict";
  var str = this.toString();
  if (arguments.length) {
    var t = typeof arguments[0];
    var key;
    var args = ("string" === t || "number" === t) ?
            Array.prototype.slice.call(arguments)
            : arguments[0];

    for (key in args) {
      str = str.replace(new RegExp("\\{" + key + "\\}", "gi"), args[key]);
    }
  }

  return str;
};