using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Xml;
using dn_lib.xml;
using dn_lib.tools;

namespace dn_lib.db
{
  public class db_sqlserver : db_provider
  {
    public db_sqlserver (string name, string conn_string, string prov_name, int timeout, string des, string date_format, string key = "", string sql_key = "")
      : base(name, conn_string, prov_name, timeout, des, date_format, key, sql_key) { }

    public SqlConnection sqlconn { get { return (SqlConnection)_conn; } }

    #region base functions

    public override void drop_table(string table) { exec("DROP TABLE " + table); }

    public override void drop_function(string func) { exec("DROP FUNCTION " + func); }

    public override void drop_procedure(string proc) { exec("DROP PROCEDURE " + proc); }

    public DataSet sp(string spName, List<parameter> pars) {
      bool opened = false;
      try {
        if (open_conn()) opened = true;

        SqlCommand command = ((SqlConnection)_conn).CreateCommand();
        if (_timeout > 0) command.CommandTimeout = _timeout;
        command.Connection = (SqlConnection)_conn;
        command.CommandText = spName;
        command.CommandType = CommandType.StoredProcedure;

        foreach (parameter par in pars) {
          System.Data.Common.DbParameter param = command.CreateParameter();
          param.ParameterName = par.name;
          param.Value = par.is_null ? DBNull.Value : par.val;
          //??? if (par.Out) DBManagDBType.Cursor
          command.Parameters.Add(param);
        }

        SqlDataAdapter adapter = new SqlDataAdapter(command);

        log.log_sql("exec " + spName); // ...finire

        DataSet ds = new DataSet();
        adapter.Fill(ds, "dsStoreProc");
        return ds;
      }
      catch (Exception ex) { log.log_err_sql(ex, string.Format("s.p. {0}({1})", spName, string.Join(", ", pars.Select(x => x.val.ToString())))); throw ex; }
      finally { if (opened) close_conn(); }
    }

    public override System.Data.Common.DbDataReader dt_reader (string sql, bool open_key = false) {
      bool opened = false;
      try {
        if (open_conn()) opened = true;

        log.log_sql(sql);

        SqlCommand command = ((SqlConnection)_conn).CreateCommand();
        command.CommandText = (open_key && _sql_key != "" ? _sql_key : "") + sql;
        if (_timeout > 0) command.CommandTimeout = _timeout;
        command.CommandType = CommandType.Text;

        return command.ExecuteReader();
      } catch (Exception ex) { log.log_err_sql(ex, sql); throw ex; }
      finally { if (opened) close_conn(); }
    }

    public override System.Data.Common.DbParameter get_par(schema_field col) {
      if (col.original_type == "datetime") return new SqlParameter("@" + col.name.ToUpper(), SqlDbType.DateTime);
      else if (col.original_type == "datetime2") return new SqlParameter("@" + col.name.ToUpper(), SqlDbType.DateTime2);
      else if (col.original_type == "int") return new SqlParameter("@" + col.name.ToUpper(), SqlDbType.Int);
      else if (col.original_type == "smallint") return new SqlParameter("@" + col.name.ToUpper(), SqlDbType.SmallInt);
      else if (col.original_type == "bigint") return new SqlParameter("@" + col.name.ToUpper(), SqlDbType.BigInt);
      else if (col.original_type == "long") return new SqlParameter("@" + col.name.ToUpper(), SqlDbType.BigInt);
      else if (col.original_type == "char" || col.original_type == "varchar" || col.original_type == "nchar" || col.original_type == "nvarchar")
        return col.max_length.Value > 0 ? new SqlParameter("@" + col.name.ToUpper(), SqlDbType.VarChar, col.max_length.Value)
            : new SqlParameter("@" + col.name.ToUpper(), SqlDbType.VarChar);
      else if (col.original_type == "text" || col.original_type == "ntext") return new SqlParameter("@" + col.name.ToUpper(), SqlDbType.Text);
      else if (col.original_type == "xml") return new SqlParameter("@" + col.name.ToUpper(), SqlDbType.Xml);
      else if (col.original_type == "bit") return new SqlParameter("@" + col.name.ToUpper(), SqlDbType.Bit);
      else if (col.original_type == "decimal" || col.original_type == "numeric") 
        return new SqlParameter("@" + col.name.ToUpper(), SqlDbType.Decimal);
      else if (col.original_type == "float" || col.original_type == "real" || col.original_type == "money" || col.original_type == "smallmoney")
        return new SqlParameter("@" + col.name.ToUpper(), SqlDbType.Float);
      else
        throw new Exception("tipo di dati '" + col.original_type + "' non supportato per l'importazione dei dati");
    }

    public override System.Data.Common.DbCommand dt_command(string sql) {
      bool opened = false;
      try {
        if (open_conn()) opened = true;

        return new SqlCommand(sql, ((SqlConnection)_conn));
        //if (_timeout > 0) result.CommandTimeout = _timeout;
      } catch (Exception ex) { log.log_err_sql(ex, sql); throw ex; }
      finally { if (opened) close_conn(); }
    }

    #endregion

    #region base special functions

    public override bool there_schema(string schema) { return dt_table("SELECT SCHEMA_ID('" + schema + "') AS SCHEMA_NAME").Rows[0]["SCHEMA_NAME"] != DBNull.Value; }

    public override void create_schema(string schemaName) {
      exec("CREATE SCHEMA " + schemaName);
    }

    public override idx_table create_index(idx_table index) {
      exec(!index.primary ? "CREATE " + (index.unique ? "UNIQUE" : "") + " " + (!index.clustered ? "NONCLUSTERED" : "CLUSTERED")
          + " INDEX [" + index.name + "] ON " + index.table_name + " ("
          + string.Join(", ", index.fields.Select(x => "[" + x.name + "]" + (x.ascending ? " ASC" : " DESC"))) + ")"
          : "ALTER TABLE " + index.table_name + " ADD " + (index.name.ToUpper() != "PRIMARY" ? " CONSTRAINT " + index.name : "") + " PRIMARY KEY "
          + (!index.clustered ? "NONCLUSTERED" : "CLUSTERED") + " ("
          + string.Join(",", index.fields.Select(x => "[" + x.name + "]")) + ")");
      return index;
    }

    public override void rename_table(string old_name, string new_name) {
      execute(string.Format("sp_rename '{0}', '{1}'", old_name, new_name));
    }

    public override void add_field(schema_field tblField, string tableName) {
      exec("ALTER TABLE " + tableName + " ADD " + tblField.fld_sql_server());
    }

    public override void del_field(string field, string table) {
      exec("ALTER TABLE " + table + " DROP COLUMN " + field);
    }

    public override void upd_field(schema_field tblField, string tableName) {
      exec("ALTER TABLE " + tableName + " ALTER COLUMN " + tblField.fld_sql_server());
    }

    public override bool exist_field(string table, string col) {
      DataTable count = dt_table("select COUNT(*) CONTEGGIO from INFORMATION_SCHEMA.COLUMNS WHERE COLUMN_NAME = '" + col + "'"
       + (table.IndexOf('.') >= 0 ? " AND TABLE_SCHEMA + '.' + TABLE_NAME = '" + table + "'"
       : " AND TABLE_NAME = '" + table + "'"));
      return int.Parse(count.Rows[0]["CONTEGGIO"].ToString()) > 0;
    }

    public override bool exist_table(string tableName) {
      return int.Parse(dt_table("SELECT COUNT(*) CONTEGGIO FROM sys.tables WHERE type = 'U' "
        + (tableName.IndexOf('.') >= 0 ? " AND SCHEMA_NAME(schema_id) + '.' + name  = '" + tableName + "'"
        : " AND name  = '" + tableName + "'")).Rows[0]["CONTEGGIO"].ToString()) > 0;
    }

    public override bool exist_procedure(string procedure) {
      return int.Parse(dt_table("SELECT COUNT(*) CONTEGGIO FROM sys.all_objects where type = 'p' and is_ms_shipped = 0"
       + (procedure.IndexOf('.') >= 0 ? " AND SCHEMA_NAME(schema_id) + '.' + name  = '" + procedure + "'"
       : " AND name  = '" + procedure + "'")).Rows[0]["CONTEGGIO"].ToString()) > 0;
    }

    public override bool exist_function(string function) {
      DataTable count = dt_table("SELECT COUNT(*) CONTEGGIO FROM sys.objects WHERE type IN ('FN', 'IF', 'TF') "
       + (function.IndexOf('.') >= 0 ? " AND SCHEMA_NAME(schema_id) + '.' + name  = '" + function + "'"
       : " AND name  = '" + function + "'"));
      return int.Parse(count.Rows[0]["CONTEGGIO"].ToString()) > 0;
    }

    public bool existSynonim(string synonim) {
      DataTable count = dt_table("SELECT COUNT(*) CONTEGGIO FROM sys.synonyms WHERE NAME = '" + synonim + "'");
      return int.Parse(count.Rows[0]["CONTEGGIO"].ToString()) > 0;
    }

    public override List<string> synonims(string likeName = "") {
      return dt_table("SELECT name FROM sys.synonyms " + (likeName != ""
        ? " WHERE NAME like '" + likeName + "'" : ""))
        .Rows.Cast<DataRow>().Select(row => row["NAME"].ToString()).ToList();
    }

    public override List<string> store_procedures(string likeName = "") {
      DataTable dt = dt_table("select SCHEMA_NAME(schema_id) as schema_name, name from sys.all_objects "
        + " where type='p' and is_ms_shipped = 0"
        + (likeName != "" ? " and SCHEMA_NAME(schema_id) + '.' + name like '" + likeName + "'" : "")
        + " ORDER BY SCHEMA_NAME(schema_id) + '.' + name");
      return dt.Rows.Cast<DataRow>().Select(row => row["SCHEMA_NAME"].ToString() + "." + row["NAME"].ToString()).ToList();
    }

    public override List<string> functions(string likeName = "") {
      DataTable dt = dt_table("SELECT SCHEMA_NAME(schema_id) as schema_name, name FROM sys.objects "
          + " WHERE type IN ('FN', 'IF', 'TF')"
          + (likeName != "" ? " and SCHEMA_NAME(schema_id) + '.' + name like '" + likeName + "'" : "")
          + " ORDER BY SCHEMA_NAME(schema_id) + '.' + name");
      return dt.Rows.Cast<DataRow>().Select(row => row["SCHEMA_NAME"].ToString() + "." + row["NAME"].ToString()).ToList();
    }

    public override string module_text(string procName) {
      System.Text.StringBuilder result = new System.Text.StringBuilder();
      try {
        foreach (DataRow dr in sp("sys.sp_helptext", new List<parameter>() { new parameter("objname", procName) })
          .Tables[0].Rows) result.Append(dr[0].ToString());
      } catch (Exception ex) { log.log_err_sql(ex, "proc. name: " + procName); }

      return result.ToString();
    }

    public override List<string> tables(string likeName = "", string list = "") {
      DataTable dt = dt_table("SELECT SCHEMA_NAME(schema_id) AS SchemaTable, name FROM sys.tables "
        + " WHERE type = 'U' " + (likeName != "" ? " AND SCHEMA_NAME(schema_id) + '.' + NAME LIKE '" + likeName + "'" : "")
        + (list != "" ? " AND SCHEMA_NAME(schema_id) + '.' + NAME IN (" + list + ")" : "")
        + "  ORDER BY SCHEMA_NAME(schema_id) + '.' + NAME");
      return dt.Rows.Cast<DataRow>().Select(row =>
        row["SchemaTable"].ToString().ToUpper() != "DBO" ? row["SchemaTable"].ToString() + "." + row["NAME"].ToString()
        : row["NAME"].ToString()).ToList();
    }

    public override void truncate_table(string table) { exec("TRUNCATE TABLE " + table); }

    // tableFields
    public override List<schema_field> table_fields(string table, string nameField = "") {

      if (table.IndexOf(".") < 0) table = "dbo." + table;

      // sql
      string sql = "SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, COLUMN_DEFAULT, CHARACTER_MAXIMUM_LENGTH"
          + " , NUMERIC_PRECISION, NUMERIC_SCALE "
          + " , ColumnProperty(object_id(table_schema + '.' + table_name), column_name, 'IsIdentity') as [IDENTITY] "
          + " FROM INFORMATION_SCHEMA.COLUMNS "
          + " WHERE " + (table.IndexOf('.') >= 0 ? " TABLE_SCHEMA + '.' + TABLE_NAME = '" + table + "'"
          : " TABLE_NAME = '" + table + "'") + (nameField != "" ? " AND COLUMN_NAME = '" + nameField + "'" : "");

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
      exec("DROP INDEX " + tableName + "." + indexName);
    }

    public override void set_identity(string table, bool on) {
      exec("SET IDENTITY_INSERT " + table + " " + (on ? "ON" : "OFF"));
    }

    public override void create_table(XmlNode tableNode, bool createIndexes = true, string nameCreated = "", List<string> flds_null = null, dbType tp_original = dbType.none) {
      string tableName = nameCreated != "" ? nameCreated : tableNode.Attributes["name"].Value;

      // schema
      if (tableName.IndexOf('.') >= 0) {
        string schemaName = tableName.Substring(0, tableName.IndexOf('.'));
        if (!there_schema(schemaName)) create_schema(schemaName);
      }

      List<idx_table> idxs = schema_doc.table_indexes(tableNode);

      // creo la tabella
      exec("CREATE TABLE " + tableName + "("
        + string.Join(",", tableNode.SelectNodes("cols/col").Cast<XmlNode>().Select(x => schema_field.fld_sql_server(x.Attributes["name"].Value
          , original_typefld(x.Attributes["type"].Value, tp_original != dbType.none ? tp_original : _dbType), xml_node.node_val(x, "numprec"), xml_node.node_val(x, "numscale"), xml_node.node_val(x, "maxlength")
          , (flds_null != null && flds_null.FirstOrDefault(s => s.ToLower() == x.Attributes["name"].Value.ToLower()) != null) ? true 
           : (schema_doc.is_primary(x.Attributes["name"].Value, idxs) ? false : xml_node.node_bool(x, "nullable", false))
          , xml_node.node_val(x, "default"), xml_node.node_bool(x, "autonumber")))) + ")");
      
      // creo gli indici
      if (createIndexes) idxs.ForEach(i => {
        i.table_name = tableName;
        if (nameCreated != "") i.name = i.name + "_" + nameCreated.Replace(".", "");
        create_index(i);
      });
    }

    public override List<idx_table> table_idxs(string table, bool? uniques = null, string index_name = "") {
      List<idx_table> indexes = new List<idx_table>();

      if (table.IndexOf(".") < 0) table = "dbo." + table;

      string sql = "SELECT distinct ind.name, ind.type_desc, ind.is_unique, ind.is_primary_key "
       + " FROM sys.indexes ind "
       + " INNER JOIN sys.tables t ON ind.object_id = t.object_id "
       + (table.IndexOf('.') > 0 ? " WHERE SCHEMA_NAME(T.schema_id) + '.' + T.NAME = '" + table + "' AND IND.NAME IS NOT NULL"
        : " WHERE T.NAME = '" + table + "' AND IND.NAME IS NOT NULL") + (index_name != "" ? " AND IND.NAME = '" + index_name + "'" : "");

      DataTable rows = dt_table(sql);
      foreach (DataRow row in rows.Rows) {

        // clustered
        bool? clustered = row["type_desc"].ToString() == "CLUSTERED" ? true
          : row["type_desc"].ToString() == "NONCLUSTERED" ? false : (bool?)null;
        if (!clustered.HasValue) {
          log.log_warning("il tipo di indice '" + type + "' non viene gestito verrà adottato il tipo 'NONCLUSTERED'!");
          clustered = false;
        }

        // filtro uniques
        bool unique = bool.Parse(row["is_unique"].ToString()), primary = bool.Parse(row["is_primary_key"].ToString());
        if (idx_table.filter_unique(uniques, unique, primary)) {
          idx_table index = new idx_table(table, row["name"].ToString(), clustered.Value, unique, primary);

          int i = 0;
          index.fields.AddRange(dt_table("SELECT col.name as colname, ic.is_descending_key "
           + " FROM sys.indexes ind "
           + " INNER JOIN sys.index_columns ic ON  ind.object_id = ic.object_id and ind.index_id = ic.index_id "
           + " INNER JOIN sys.columns col ON ic.object_id = col.object_id and ic.column_id = col.column_id "
           + " INNER JOIN sys.tables t ON ind.object_id = t.object_id "
           + ((table.IndexOf('.') >= 0) ? " WHERE SCHEMA_NAME(T.schema_id) + '.' + t.name = '" + table + "' and ind.name = '" + index.name + "' "
           : " WHERE t.name = '" + table + "' and ind.name = '" + index.name + "' ")
           + " ORDER BY t.name, ind.name, ind.index_id, ic.index_column_id ").Rows.Cast<DataRow>()
           .Select(dr => new idx_field(dr["colname"].ToString(), !bool.Parse(dr["is_descending_key"].ToString()), i++)));

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
      else if (coltype == fieldType.DECIMAL || coltype == fieldType.DOUBLE|| coltype == fieldType.MONEY)
        return op + value.Replace(",", ".");
      else
        throw new Exception("tipo di dati '" + coltype.ToString() + "' non supportato per l'importazione dei dati");
    }

    public override string quadra(string element) { return string.Format("[{0}]", element);  }

    #endregion

    #region tools

    public override string enc_qry (string val) { return string.Format("EncryptByKey(Key_GUID('tl01'), '{0}')", val.Replace("'", "''")); }

    #endregion
  }
}

