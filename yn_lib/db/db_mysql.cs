using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.Common;
using System.Xml;
using dn_lib.tools;

namespace dn_lib.db
{
  public class db_mysql : db_provider
  {
    public db_mysql (string name, string conn_string, string prov_name, int timeout, string des, string date_format, string key = "", string sql_key = "")
      : base(name, conn_string, prov_name, timeout, des, date_format, key, sql_key) { }

    //public MySqlConnection myconn { get { return (MySqlConnection)_conn; } }

    #region base functions

    public string db_name() { return key_conn("database"); }

    public override void drop_table(string table) { exec("DROP TABLE " + table); }

    //public override void drop_function(string func) { exec("DROP FUNCTION " + func); }

    //public override void drop_procedure(string proc) { exec("DROP PROCEDURE " + proc); }

    //public DataSet sp(string spName, List<parameter> pars) {
    //  bool opened = false;
    //  try {
    //    if (open_conn()) opened = true;

    //    SqlCommand command = ((SqlConnection)_conn).CreateCommand();
    //    command.CommandTimeout = 60;
    //    command.Connection = (SqlConnection)_conn;
    //    command.CommandText = spName;
    //    command.CommandType = CommandType.StoredProcedure;

    //    foreach (parameter par in pars) {
    //      System.Data.Common.DbParameter param = command.CreateParameter();
    //      param.ParameterName = par.Name;
    //      param.Value = par.IsNull ? DBNull.Value : par.Value;
    //      //??? if (par.Out) DBManagDBType.Cursor
    //      command.Parameters.Add(param);
    //    }

    //    SqlDataAdapter adapter = new SqlDataAdapter(command);

    //    log.log_sql("exec " + spName); // ...finire

    //    DataSet ds = new DataSet();
    //    adapter.Fill(ds, "dsStoreProc");
    //    return ds;
    //  }
    //  catch (Exception ex) { log.log_err(ex, string.Format("s.p. {0}({1})", spName, string.Join(", ", pars.Select(x => x.Value.ToString())))); throw ex; }
    //  finally { if (opened) close_conn(); }
    //}

    //public override System.Data.Common.DbParameter get_par(schema_field col) {
    //  if (col.OriginalType == "datetime") return new SqlParameter("@" + col.Name.ToUpper(), SqlDbType.DateTime);
    //  else if (col.OriginalType == "datetime2") return new SqlParameter("@" + col.Name.ToUpper(), SqlDbType.DateTime2);
    //  else if (col.OriginalType == "int") return new SqlParameter("@" + col.Name.ToUpper(), SqlDbType.Int);
    //  else if (col.OriginalType == "smallint") return new SqlParameter("@" + col.Name.ToUpper(), SqlDbType.SmallInt);
    //  else if (col.OriginalType == "bigint") return new SqlParameter("@" + col.Name.ToUpper(), SqlDbType.BigInt);
    //  else if (col.OriginalType == "long") return new SqlParameter("@" + col.Name.ToUpper(), SqlDbType.BigInt);
    //  else if (col.OriginalType == "char" || col.OriginalType == "varchar" || col.OriginalType == "nchar" || col.OriginalType == "nvarchar")
    //    return col.MaxLength.Value > 0 ? new SqlParameter("@" + col.Name.ToUpper(), SqlDbType.VarChar, col.MaxLength.Value)
    //        : new SqlParameter("@" + col.Name.ToUpper(), SqlDbType.VarChar);
    //  else if (col.OriginalType == "text" || col.OriginalType == "ntext") return new SqlParameter("@" + col.Name.ToUpper(), SqlDbType.Text);
    //  else if (col.OriginalType == "xml") return new SqlParameter("@" + col.Name.ToUpper(), SqlDbType.Xml);
    //  else if (col.OriginalType == "bit") return new SqlParameter("@" + col.Name.ToUpper(), SqlDbType.Bit);
    //  else if (col.OriginalType == "decimal" || col.OriginalType == "numeric") 
    //    return new SqlParameter("@" + col.Name.ToUpper(), SqlDbType.Decimal);
    //  else if (col.OriginalType == "float" || col.OriginalType == "real" || col.OriginalType == "money" || col.OriginalType == "smallmoney")
    //    return new SqlParameter("@" + col.Name.ToUpper(), SqlDbType.Float);
    //  else
    //    throw new Exception("tipo di dati '" + col.OriginalType + "' non supportato per l'importazione dei dati");
    //}

    //public override System.Data.Common.DbCommand dt_command(string sql) {
    //  bool opened = false;
    //  try {
    //    if (open_conn()) opened = true;

    //    return new MySqlCommand(sql, ((MySqlConnection)_conn));
    //    //if (_timeout > 0) result.CommandTimeout = _timeout;
    //  } catch (Exception ex) { log.log_err(ex, sql); throw ex; } finally { if (opened) close_conn(); }
    //}

    #endregion

    #region base special functions

    //public override idx_table create_index(idx_table index) {
    //  exec(!index.Primary ? "CREATE " + (index.Unique ? "UNIQUE" : "") + " " + (!index.Clustered ? "NONCLUSTERED" : "CLUSTERED")
    //      + " INDEX [" + index.Name + "] ON " + index.TableName + " ("
    //      + string.Join(", ", index.Fields.Select(x => "[" + x.Name + "]" + (x.Ascending ? " ASC" : " DESC"))) + ")"
    //    : "ALTER TABLE " + index.TableName + " ADD CONSTRAINT " + index.Name + " PRIMARY KEY "
    //      + (!index.Clustered ? "NONCLUSTERED" : "CLUSTERED") + " ("
    //      + string.Join(",", index.Fields.Select(x => "[" + x.Name + "]")) + ")");
    //  return index;
    //}

    public override void rename_table(string old_name, string new_name) {
      execute(string.Format("RENAME TABLE {0} TO {1};", old_name, new_name));
    }

    //public override void add_field(schema_field tblField, string tableName) {
    //  exec("ALTER TABLE " + tableName + " ADD " + tblField.getFieldSqlServer());
    //}

    //public override void upd_field(schema_field tblField, string tableName) {
    //  exec("ALTER TABLE " + tableName + " ALTER COLUMN " + tblField.getFieldSqlServer());
    //}

    public override bool exist_field(string table, string col) {
      return get_count(string.Format("SELECT count(*) FROM information_schema.columns "
       + " WHERE TABLE_SCHEMA = '{0}' AND TABLE_NAME = '{1}' AND COLUMN_NAME = '{2}'", db_name(), table, col)) > 0;
    }

    public override bool exist_table(string tbl_name) {
      return get_count(string.Format("SELECT COUNT(*) CONTEGGIO FROM information_schema.tables "
       + " where lower(table_schema) = '{0}' and table_name = '{1}';", db_name().ToLower(), tbl_name)) > 0;
    }

    public override bool exist_procedure(string procedure) {
      return int.Parse(dt_table("SELECT COUNT(*) CONTEGGIO FROM INFORMATION_SCHEMA.ROUTINES "
       + " where lower(routine_name) = '" + procedure.ToLower() + "' routine_type = 'PROCEDURE' "
       + "  and lower(routine_schema) = '" + db_name().ToLower() + "'").Rows[0]["CONTEGGIO"].ToString()) > 0;
    }

    public override bool exist_function(string function) {
      return int.Parse(dt_table("SELECT COUNT(*) CONTEGGIO FROM INFORMATION_SCHEMA.ROUTINES "
       + " where lower(routine_name) = '" + function.ToLower() + "' routine_type = 'FUNCTION' "
       + "  and lower(routine_schema) = '" + db_name().ToLower() + "'").Rows[0]["CONTEGGIO"].ToString()) > 0;
    }

    //public bool existSynonim(string synonim) {
    //  DataTable count = dt_table("SELECT COUNT(*) CONTEGGIO FROM sys.synonyms WHERE NAME = '" + synonim + "'");
    //  return int.Parse(count.Rows[0]["CONTEGGIO"].ToString()) > 0;
    //}

    //public override List<string> synonims(string likeName = "") {
    //  return dt_table("SELECT name FROM sys.synonyms " + (likeName != ""
    //    ? " WHERE NAME like '" + likeName + "'" : ""))
    //    .Rows.Cast<DataRow>().Select(row => row["NAME"].ToString()).ToList();
    //}

    public override List<string> store_procedures(string likeName = "") {
      return dt_table("SELECT routine_name FROM INFORMATION_SCHEMA.ROUTINES "
       + " where routine_type = 'PROCEDURE' and lower(routine_schema) = '" + db_name().ToString() + "'")
        .Rows.Cast<DataRow>().Select(row => row["name"].ToString()).ToList();
    }

    public override List<string> functions(string likeName = "") {
      return dt_table("SELECT routine_name FROM INFORMATION_SCHEMA.ROUTINES "
       + " where routine_type = 'FUNCTION' and lower(routine_schema) = '" + db_name().ToLower() + "'")
        .Rows.Cast<DataRow>().Select(row => row["name"].ToString()).ToList();
    }

    public override string module_text(string procName) {
      System.Text.StringBuilder result = new System.Text.StringBuilder();
      try {
        DataTable dt = dt_table("SELECT ROUTINE_DEFINITION FROM INFORMATION_SCHEMA.ROUTINES "
         + " where lower(routine_name) = '" + procName.ToLower() + "'");
        return dt.Rows.Count > 0 ? dt.Rows[0]["ROUTINE_DEFINITION"].ToString() : "";
      } catch (Exception ex) { log.log_err_sql(ex, "module name: " + procName); }

      return result.ToString();
    }

    public override List<string> tables(string likeName = "", string list = "") {
      return dt_table("SELECT * FROM information_schema.tables "
        + " WHERE lower(table_schema) = '" + db_name().ToLower() + "' " + (likeName != "" ? " AND lower(table_name) like '" + likeName.ToLower() + "'" : "")
        + (list != "" ? " AND lower(table_name) in (" + list.ToLower() + ")" : "") + " ORDER BY table_name")
        .Rows.Cast<DataRow>().Select(row => row["table_name"].ToString()).ToList();
    }

    public override void truncate_table(string table) { exec("TRUNCATE TABLE " + table); }

    // tableFields
    public override List<schema_field> table_fields(string table, string nameField = "") {

      // sql
      string sql = "SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, COLUMN_DEFAULT "
          + " , CHARACTER_MAXIMUM_LENGTH, NUMERIC_PRECISION, NUMERIC_SCALE "
          + " , instr(EXTRA, 'auto_increment') as identity "
          + " FROM INFORMATION_SCHEMA.COLUMNS WHERE lower(TABLE_SCHEMA) = '" + db_name().ToLower() + "'" 
          + "  AND lower(TABLE_NAME) = '" + table.ToLower() + "'" + (nameField != "" ? " AND lower(COLUMN_NAME) = '" + nameField.ToLower() + "'" : "");

      // ciclo campi
      List<schema_field> result = new List<schema_field>();
      int i = 0;
      foreach (DataRow field in dt_table(sql).Rows)
        result.Add(new schema_field(_dbType, field["COLUMN_NAME"].ToString(), field["DATA_TYPE"].ToString()
          , !(field["IS_NULLABLE"].ToString() == "NO"), field["CHARACTER_MAXIMUM_LENGTH"] != DBNull.Value ? int.Parse(field["CHARACTER_MAXIMUM_LENGTH"].ToString()) : (int?)null
          , field["NUMERIC_PRECISION"] != DBNull.Value ? int.Parse(field["NUMERIC_PRECISION"].ToString()) : (int?)null
          , field["NUMERIC_SCALE"] != DBNull.Value ? int.Parse(field["NUMERIC_SCALE"].ToString()) : (int?)null
          , field["COLUMN_DEFAULT"] != DBNull.Value ? field["COLUMN_DEFAULT"].ToString() : "", field["IDENTITY"] != DBNull.Value && field["IDENTITY"].ToString() != "0"
          , false, i++));

      return result;
    }

    public override void drop_index(string indexName, string tableName) {
      exec("DROP INDEX " + indexName + " ON " + tableName);
    }

    public override void set_identity(string table, bool on) {
      //exec("SET IDENTITY_INSERT " + table + " " + (on ? "ON" : "OFF"));
    }

    //public override void create_table(XmlNode tableNode, bool createIndexes = true, string nameCreated = "", List<string> flds_null = null) {
    //  string tableName = nameCreated != "" ? nameCreated : tableNode.Attributes["name"].Value;

    //  // schema
    //  if (tableName.IndexOf('.') >= 0) {
    //    string schemaName = tableName.Substring(0, tableName.IndexOf('.'));
    //    if (!there_schema(schemaName)) create_schema(schemaName);
    //  }

    //  // creo la tabella
    //  exec("CREATE TABLE " + tableName + "("
    //    + string.Join(",", tableNode.SelectNodes("cols/col").Cast<XmlNode>().Select(x => schema_field.getFieldSqlServer(x.Attributes["name"].Value
    //      , x.Attributes["type"].Value, xmlDoc.nodeValue(x, "numprec"), xmlDoc.nodeValue(x, "numscale"), xmlDoc.nodeValue(x, "maxlength")
    //      , xmlDoc.nodeBool(x, "nullable", false), xmlDoc.nodeValue(x, "default"), xmlDoc.nodeBool(x, "autonumber")))) + ")");

    //  // creo gli indici
    //  if (createIndexes) schema_doc.table_indexes(tableNode).ForEach(i => {
    //    i.TableName = tableName;
    //    if (nameCreated != "") i.Name = i.Name + "_" + nameCreated.Replace(".", "");
    //    create_index(i);
    //  });
    //}

    public override List<idx_table> table_idxs(string table, bool? uniques = null, string index_name = "") {
      List<idx_table> indexes = new List<idx_table>();

      DataTable rows = dt_table("select distinct ind.index_name, ind.index_type, ind.non_unique "
       + " from information_schema.statistics ind "
       + " where ind.table_schema = '" + db_name() + "' and ind.table_name = '" + table + "' "
       + (index_name != "" ? " and ind.index_name = '" + index_name + "'" : ""));
      foreach (DataRow row in rows.Rows) {

        // clustered
        bool? clustered = false;

        // filtro uniques
        bool unique = row["non_unique"].ToString() == "0", primary = row["index_name"].ToString() == "PRIMARY";
        if (idx_table.filter_unique(uniques, unique, primary)) {
          idx_table index = new idx_table(table, row["index_name"].ToString(), clustered.Value, unique, primary);

          int i = 0;
          index.fields.AddRange(dt_table("SELECT ind.column_name as colname, 0 as is_descending_key "
           + " FROM information_schema.statistics ind "
           + " WHERE ind.table_schema = '" + db_name() + "' and ind.table_name = '" + table + "' and ind.index_name = '" + index.name + "' "
           + " ORDER BY ind.seq_in_index").Rows.Cast<DataRow>()
           .Select(dr => new idx_field(dr["colname"].ToString(), !(dr["is_descending_key"].ToString() == "1"), i++)));

          indexes.Add(index);
        }
      }

      return indexes;
    }

    public override string val_toqry(string value, fieldType coltype, dbType type, string nullstr = null, bool add_operator = false, bool tostring = false) {
      if (value == null || (nullstr != null && value == nullstr))
        return (add_operator ? " IS " : "") + "NULL";

      string op = add_operator ? " = " : "", ap = !tostring ? "'" : "";
      if (coltype == fieldType.DATETIME) return op + ap + DateTime.Parse(value).ToString(_date_format) + ap;
      else if (coltype == fieldType.INTEGER || coltype == fieldType.LONG
          || coltype == fieldType.SINGLE || coltype == fieldType.SMALLINT) return op + value;
      else if (coltype == fieldType.CHAR || coltype == fieldType.TEXT || coltype == fieldType.VARCHAR)
        return op + ap + (!tostring ? value.Replace("'", "''") : value) + ap;
      else if (coltype == fieldType.XML) return op + ap + (!tostring ? value.Replace("'", "''") : value) + ap;
      else if (coltype == fieldType.BOOL) return op + (bool.Parse(value) ? "1" : "0");
      else if (coltype == fieldType.DECIMAL || coltype == fieldType.DOUBLE || coltype == fieldType.MONEY)
        return op + value.Replace(",", ".");
      else
        throw new Exception("tipo di dati '" + coltype.ToString() + "' non supportato per l'importazione dei dati");
    }

    #endregion
  }
}

