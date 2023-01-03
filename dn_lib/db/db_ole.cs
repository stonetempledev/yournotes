using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using dn_lib.tools;

namespace dn_lib.db
{
  public class db_ole : db_provider
  {
    public db_ole (string name, string conn_string, string prov_name, int timeout, string des, string date_format, string key = "", string sql_key = "")
      : base(name, conn_string, prov_name, timeout, des, date_format, key, sql_key) { }

    public OleDbConnection oleconn { get { return (OleDbConnection)_conn; } }

    #region base functions

    public override void drop_table(string table) { exec("DROP TABLE " + table); }

    public override System.Data.Common.DbDataReader dt_reader (string sql, bool open_key = false) {
      bool opened = false;
      try {
        if (open_conn()) opened = true;

        log.log_sql(sql);

        OleDbCommand command = ((OleDbConnection)_conn).CreateCommand();
        command.CommandText = sql;
        if (_timeout > 0) command.CommandTimeout = _timeout;
        command.CommandType = CommandType.Text;

        //if (_timeout > 0) command.CommandTimeout = _timeout;

        return command.ExecuteReader();
      }
      catch (Exception ex) { log.log_err(ex); throw ex; }
      finally { if (opened) close_conn(); }
    }

    #endregion
  }
}