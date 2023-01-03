using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;
using dn_lib.tools;

namespace dn_lib.db
{
  public class db_odbc : db_provider
  {
    public db_odbc (string name, string conn_string, string prov_name, int timeout, string des, string date_format, string key = "", string sql_key = "")
      : base(name, conn_string, prov_name, timeout, des, date_format, key, sql_key) { }

    public OdbcConnection odbcconn { get { return (OdbcConnection)_conn; } }

    #region base functions

    public override bool exist_table(string tableName) { return false; }

    public override System.Data.Common.DbDataReader dt_reader(string sql, bool open_key = false) {
      bool opened = false;
      try {
        if (open_conn()) opened = true;

        log.log_sql(sql);

        OdbcCommand command = ((OdbcConnection)_conn).CreateCommand();
        command.CommandText = sql;
        if (_timeout > 0) command.CommandTimeout = _timeout;
        command.CommandType = CommandType.Text;

        //if (_timeout > 0) command.CommandTimeout = _timeout;

        return command.ExecuteReader();
      }
      catch (Exception ex) { log.log_err(ex); throw ex; }
      finally { if (opened) close_conn(); }
    }

    // tableFields
    public override List<schema_field> table_fields(string table, string nameField = "") {

      // ciclo campi
      List<schema_field> result = new List<schema_field>();
      DataTable dt = dt_schema("select * from " + table);
      foreach (DataColumn c in dt.Columns) {
        result.Add(new schema_field(dbType.odbc, c.ColumnName, c.DataType.ToString()
          , c.AllowDBNull, c.MaxLength == 1073741824 ? -1 : c.MaxLength, null, null, "", c.AutoIncrement, false, c.Ordinal));
      }
   
      return result;
    }

    public override List<idx_table> table_idxs(string table, bool? uniques = null, string index_name = "") {
      return new List<idx_table>();
    }

    public override List<string> tables(string likeName = "", string list = "") {
      bool opened = false;
      try {
        if (open_conn()) opened = true;

        if (likeName != "" || list != "") throw new Exception("parametro non gestito in ODBC - tables");

        if (_conn.DataSource == "DBASE") 
          return new List<string>(System.IO.Directory.EnumerateFiles(_conn.Database, "*.dbf").Select(x => System.IO.Path.GetFileNameWithoutExtension(x)));
        else throw new Exception("il provider ODBC '" + _conn.DataSource + "' non supporta la funzionalità tables");
      } catch (Exception ex) { log.log_err(ex); throw ex; } finally { if (opened) close_conn(); }
    }

    public override List<string> store_procedures(string likeName = "") { return new List<string>(); }

    public override List<string> functions(string likeName = "") { return new List<string>(); }

    #endregion
  }
}