using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Data;
using dn_lib;
using dn_lib.db;
using dn_lib.tools;

namespace deepanotes {
  // business object
  public class bo {
    tl_page _page = null;
    public db_provider db_conn { get { return _page.db_conn; } }
    public core core { get { return _page.core; } }
    public config config { get { return _page.core.config; } }
    public int user_id { get { return _page.user.id; } }
    public string user_name { get { return _page.user.name; } }

    public bo() {
      _page = HttpContext.Current.Handler as tl_page;
      if (_page == null) throw new Exception("pagina attiva non valida!");
    }

    // SETTINGS

    public string set_setting(string setting, string val) {
      db_conn.exec(core.parse_query("lib-base.set-setting"
        , new string[,] { { "setting", setting }, { "value", string.IsNullOrEmpty(val) ? "" : val }, { "user_id", user_id.ToString() } }));
      return val;
    }

  }
}