using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using dn_lib.db;

namespace dn_client {
  public class db_conn {
    public enum conn_type { PRODUZIONE, TEST, SVILUPPO, LABORATORIO }
    public int id_conn { get; set; }
    public string conn_name { get; set; }
    public string conn_des { get; set; }
    public conn_type type { get; set; }
    public string conn_string { get; set; }
    public string provider { get; set; }

    public db_conn(string name, string conn_string, string provider) {
      this.conn_name = name; this.conn_string = conn_string; this.provider = provider;
    }

    public db_conn(DataRow dr) {
      this.id_conn = db_provider.int_val(dr["id_conn"]);
      this.conn_name = db_provider.str_val(dr["conn_name"]);
      this.type = (conn_type)Enum.Parse(typeof(conn_type), db_provider.str_val(dr["conn_type"]));
      this.conn_string = db_provider.str_val(dr["conn_string"]);
      this.provider = db_provider.str_val(dr["provider"]);
      this.conn_des = db_provider.str_val(dr["conn_des"]);
    }
  }
}
