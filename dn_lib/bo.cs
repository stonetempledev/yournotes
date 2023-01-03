using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using dn_lib;
using dn_lib.db;
using dn_lib.tools;

namespace dn_lib {

  // business object
  public class bo {
    public db_provider db_conn { get; protected set; }
    public core core { get; protected set; }
    public config config { get; protected set; }
    public int user_id { get; protected set; }
    public string user_name { get; protected set; }

    public bo(db_provider conn, core c, config cfg, int user_id = 0, string user_name = "") {
      this.db_conn = conn; this.core = c; this.config = cfg; this.user_id = user_id; this.user_name = user_name;
    }

    // SETTINGS

    public string set_setting(string setting, string val) {
      db_conn.exec(core.parse_query("lib-base.set-setting"
        , new string[,] { { "setting", setting }, { "value", string.IsNullOrEmpty(val) ? "" : val }, { "user_id", user_id.ToString() } }));
      return val;
    }

    // CACHE

    public bool set_cache_var(string var_name, string var_value, int user_id)
    {
      return string.IsNullOrEmpty(var_value) ? reset_cache_var(var_name, user_id)
        : set_cache_var2(var_name, var_value, user_id);
    }

    private bool set_cache_var2(string var_name, string var_value, int user_id)
    {
      db_conn.exec(core.parse(config.get_query("lib-base.set-cache-var").text
        , new Dictionary<string, object>() { { "user_id", user_id }, { "var_name", var_name }, { "var_value", var_value } }));
      return true;
    }

    protected bool reset_cache_var(string var_name, int user_id)
    {
      db_conn.exec(core.parse(config.get_query("lib-base.reset-cache-var").text
        , new Dictionary<string, object>() { { "user_id", user_id }, { "var_name", var_name } }));
      return true;
    }

    public string get_cache_var(string var_name, int user_id, string def = "")
    {
      DataRow dr = db_conn.first_row(core.parse(config.get_query("lib-base.get-cache-var").text
        , new Dictionary<string, object>() { { "user_id", user_id }, { "list_vars", "'" + var_name + "'" } }));
      return dr != null ? db_provider.str_val(dr["var_value"], def) : def;
    }

    //protected Dictionary<string, string> get_cache_vars(string var_names, int user_id)
    //{
    //  Dictionary<string, string> res = new Dictionary<string, string>();
    //  foreach(DataRow dr in db_conn.dt_table(core.parse(config.get_query("lib-base.get-cache-var").text
    //    , new Dictionary<string, object>() { { "user_id", user_id }
    //    , { "list_vars", string.Join(", ", var_names.Split(new char[]{','}, StringSplitOptions.RemoveEmptyEntries).Select(s => "'" + s + "'")) } })).Rows) {
    //    res.Add(db_provider.str_val(dr["var_name"]), db_provider.str_val(dr["var_value"]));
    //  }
    //  return res;
    //}


  }
}
