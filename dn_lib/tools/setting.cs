using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using dn_lib.db;
using dn_lib.tools;

namespace dn_lib
{
  public class setting
  {

    public int id { get; set; }
    public string name { get; set; }
    public string value { get; set; }
    public string machine_name { get; set; }

    public setting(int id, string name, string value, string machine_name)
    {
      this.id = id; this.name = name; this.value = value; this.machine_name = string.IsNullOrEmpty(machine_name) ? "" : machine_name.ToLower();
    }
  }

  public class settings
  {
    public List<setting> list { get; set; }
    public settings(List<setting> l)
    {
      this.list = l;
    }
    public static settings read_settings(core c, db_provider conn)
    {
      return new settings(conn.dt_table(c.config.get_query("lib-base.get-settings").text)
          .Rows.Cast<DataRow>().Select(r => new setting((int)r["setting_id"]
            , (string)r["setting_name"], db_provider.str_val(r["setting_var"])
            , db_provider.str_val(r["machine_name"]))).ToList());
    }
    public string get_value(string setting_name)
    {
      setting s = this.list.FirstOrDefault(x => x.name == setting_name && x.machine_name == sys.machine_name());
      return s != null ? s.value : this.list.FirstOrDefault(x => x.name == setting_name).value;
    }
    public static setting get_setting(core c, db_provider conn, string setting_name)
    {
      DataRow r = conn.first_row(c.parse_query("lib-base.get-setting", new string[,] { { "setting-name", setting_name } }));
      return r == null ? null : new setting((int)r["setting_id"], (string)r["setting_name"]
          , db_provider.str_val(r["setting_var"]), db_provider.str_val(r["machine_name"]));
    }
  }
}
