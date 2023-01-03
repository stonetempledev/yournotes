using System;
using System.Data;
using System.Collections.Generic;
using System.Web;
using System.Data.Common;
using System.Xml;
using System.Linq;
using System.Text;
using dn_lib.tools;

namespace dn_lib.db {
  public enum dbType { none, oledb, odbc, sqlserver, mysql, access, xml }

  public enum fieldType {
    LONG, BINARY, BOOL, MONEY, SMALLDATETIME, DATE, DATETIME,
    DATETIME2, TIMESTAMP, GUID, DOUBLE, SINGLE, SMALLINT, INTEGER,
    DECIMAL, VARCHAR, CHAR, XML, IMAGE, TEXT, VARIANT
  }

  public enum catField { NUMERIC, TEXT, DATE }

  public class db_provider {
    protected string _name = "", _conn_string = "", _provider = "", _des = "", _date_format = "", _sql_key = "", _key = "";
    protected dbType _dbType = dbType.none;
    protected int _timeout = -1;
    protected DbConnection _conn = null;
    protected DbTransaction _trans = null;
    protected Dictionary<string, string> _keys = null;

    public DbConnection db_conn { get { return _conn; } }

    public db_provider(dn_lib.tools.config.conn cnn) :
      this(cnn.name, cnn.conn_string, cnn.provider, cnn.timeout > 0 ? cnn.timeout : -1, cnn.des, cnn.date_format) { }

    public db_provider(string name, string conn_string, string prov_name, int timeout = -1, string des = ""
      , string date_format = "", string key = "", string sql_key = "") {
      _dbType = db_provider.type_from_provider(conn_string, prov_name); _name = name;
      _timeout = timeout; _conn_string = conn_string; _provider = prov_name; _des = des;
      _date_format = date_format != "" ? date_format : "yyyy-MM-dd HH:mm:ss"; _key = key; _sql_key = sql_key;
      _keys = new Dictionary<string, string>();
      foreach (string value in _conn_string.Split(';')) {
        int iUguale = value.IndexOf('=');
        if (iUguale > 0) _keys.Add(value.Substring(0, iUguale).ToLower(),
          value.Substring(iUguale + 1, value.Length - iUguale - 1));
      }
    }

    public static db_provider create_provider(dn_lib.tools.config.conn cnn) {
      return create_provider(cnn.name, cnn.conn_string, cnn.provider, cnn.timeout > 0 ? cnn.timeout : -1, cnn.des, cnn.date_format, cnn.key, cnn.sql_key);
    }

    public static db_provider create_provider(string name, string conn_string, string provider, int timeout = -1, string des = "", string date_format = "", string key = "", string sql_key = "") {
      dbType type = db_provider.type_from_provider(conn_string, provider);
      return type == dbType.access ? new db_access(name, conn_string, provider, timeout, des, date_format, key, sql_key)
          : type == dbType.odbc ? new db_odbc(name, conn_string, provider, timeout, des, date_format, key, sql_key)
          : type == dbType.sqlserver ? new db_sqlserver(name, conn_string, provider, timeout, des, date_format, key, sql_key)
          : type == dbType.mysql ? new db_mysql(name, conn_string, provider, timeout, des, date_format, key, sql_key)
          : new db_provider(name, conn_string, provider, timeout, des, date_format, key, sql_key);
    }

    protected static DbProviderFactory get_factory(dbType tp) {
      return DbProviderFactories.GetFactory(tp == dbType.oledb || tp == dbType.access ? "System.Data.OleDb"
          : tp == dbType.odbc ? "System.Data.Odbc" : tp == dbType.sqlserver ? "System.Data.SqlClient"
          : tp == dbType.mysql ? "MySql.Data.MySqlClient" : "");
    }

    static public dbType type_from_provider(string conn_string, string provider_name) {
      return provider_name == "System.Data.SqlClient" ? dbType.sqlserver :
        (provider_name == "MySql.Data.MySqlClient" ? dbType.mysql :
        (provider_name == "System.Data.OleDb" ? (conn_string.ToLower().IndexOf("provider=microsoft.jet.oledb.4.0") >= 0 ? dbType.access : dbType.oledb) :
        (provider_name == "System.Data.Odbc" ? dbType.odbc :
        (provider_name == "db.xml" ? dbType.xml : dbType.none))));
    }

    public string date_format { get { return _date_format; } }
    public dbType type { get { return _dbType; } }
    public string des { get { return _des; } }
    public string conn { get { return _conn_string; } }
    public string name { get { return _name; } }
    public Dictionary<string, string> conn_keys { get { return _keys; } }

    static public catField cat_field(fieldType tp) {
      return tp == fieldType.VARCHAR || tp == fieldType.CHAR || tp == fieldType.XML || tp == fieldType.TEXT ? catField.TEXT :
        tp == fieldType.DATETIME ? catField.DATE : catField.NUMERIC;
    }

    #region tools

    static public string list_in(DataRow dr, string fields) {
      if (dr == null) return "";
      string res = "";
      foreach (string fld in fields.Split(new char[] { ',' })) {
        string val = str_val(dr[fld]); if (val != "") res += (res != "" ? "," : "") + val;
      }
      return res;
    }

    static public string str_val(object fld, string def = "") { return fld == null || fld == DBNull.Value ? def : fld.ToString(); }

    static public int int_val(object fld, int def = 0) { return fld == null || fld == DBNull.Value ? def : Convert.ToInt32(fld); }

    static public float float_val(object fld, float def = 0) { return fld == null || fld == DBNull.Value ? def : float.Parse(fld.ToString()); }

    static public long long_val(object fld, long def = 0) { return fld == null || fld == DBNull.Value ? def : Convert.ToInt64(fld); }

    static public long? long_val_null(object fld) { return fld == null || fld == DBNull.Value ? (long?)null : Convert.ToInt64(fld); }

    static public int? int_val_null(object fld) { return fld == null || fld == DBNull.Value ? (int?)null : Convert.ToInt32(fld); }

    static public DateTime? dt_val(object fld) {
      return fld == null || fld == DBNull.Value || (fld is string && fld.ToString() == "")
        ? (DateTime?)null : Convert.ToDateTime(fld);
    }

    public static string row_to_csv(DataRow dr) {
      string res = "";
      foreach (DataColumn dc in dr.Table.Columns) {
        if (dc.DataType == typeof(decimal) || dc.DataType == typeof(double))
          res += dr[dc] != DBNull.Value ? Convert.ToDouble(dr[dc]).ToString(System.Globalization.CultureInfo.InvariantCulture) + ";" : "null;";
        else if (dc.DataType == typeof(DateTime))
          res += dr[dc] != DBNull.Value ? ((DateTime)dr[dc]).ToString("yyyy-MM-dd") + ";" : "null;";
        else if (dc.DataType == typeof(bool))
          res += dr[dc] != DBNull.Value ? ((bool)dr[dc] ? 0 : 1) + ";" : "0;";
        else if (dc.DataType == typeof(string))
          res += dr[dc] != DBNull.Value ? "\"" + dr[dc].ToString().Replace("'", "''").Replace("\n", " ").Replace("\r", " ") + "\"" + ";" : "\"\";";
        else res += dr[dc] != DBNull.Value ? dr[dc].ToString() + ";" : "null;";
      }

      return res;
    }

    public static string str_qry(string value) {
      if (value == null) return "NULL";
      return "'" + value.Replace("'", "''") + "'";
    }

    public static string dt_qry(DateTime value) {
      if (value == null || value == DateTime.MinValue) return "NULL";
      return string.Format("convert(datetime, '{0}', 120)", value.ToString("yyyy-MM-dd HH:mm:ss"));
    }

    #endregion

    #region base functions

    public bool check_begin_trans(bool open_key = false) {
      if (this.is_opened()) return false;
      this.begin_trans(open_key);
      return true;
    }

    public void begin_trans(bool open_key = false) {
      try {
        if (!is_opened()) throw new Exception("begin trans - la connessione non è aperta");
        if (_trans != null) throw new Exception("begin transaction annullata la transazione è già aperta!");

        log.log_sql("begin transaction");
        _trans = _conn.BeginTransaction();
        if (open_key && _sql_key != "") exec(_sql_key);
      } catch (Exception ex) { log.log_err(ex); throw ex; }
    }

    public void commit() {
      try {
        if (!is_opened()) throw new Exception("commit annullata la connessione non è aperta!");
        if (_trans == null) throw new Exception("commit annullata la transazione non è aperta!");

        log.log_sql("commit transaction");
        _trans.Commit();
        _trans = null;
      } catch (Exception ex) { log.log_err(ex); throw ex; }
    }

    public void rollback() {
      try {
        if (!is_opened()) throw new Exception("rollback annullata la connessione non è aperta!");
        if (_trans == null) throw new Exception("rollback annullata la transazione non è aperta!");

        log.log_sql("rollback transaction");
        _trans.Rollback();
        _trans = null;
      } catch (Exception ex) { log.log_err(ex); }
    }

    public bool open_conn() { return open_conn(false); }

    public bool open_conn_trans() { return open_conn(true); }

    protected bool open_conn(bool withBeginTrans) {
      if (_conn != null)
        return false;

      log.log_sql("open conn '" + _conn_string + "'");

      _conn = create_conn(_provider, _conn_string);

      _conn.Open();

      if (withBeginTrans && !is_trans())
        begin_trans();

      return true;
    }

    protected static DbConnection create_conn(string provider, string conn_string) {
      if (string.IsNullOrEmpty(conn_string)) throw new Exception("non è stata specificata la stringa di connessione!");

      DbProviderFactory factory = DbProviderFactories.GetFactory(provider);
      DbConnection connection = factory.CreateConnection();
      connection.ConnectionString = conn_string;
      return connection;
    }

    public bool is_opened() { return _conn != null; }

    public bool is_trans() { return _trans != null; }

    public void close_conn() { close_conn(true); }

    public void close_conn_roll() { close_conn(false); }

    protected virtual void close_conn(bool cmt) {
      if (_conn == null) return;

      if (is_trans()) {
        if (cmt) commit();
        else rollback();
      }

      log.log_sql("close connection");

      _conn.Close();
      _conn = null;
    }

    public virtual DataSet dt_set(string sql, bool throwerr = true, bool open_key = false) { return data_set(sql, throwerr, "", open_key); }
    public virtual DataSet dt_set(string sql, string table_name, bool throwerr = true, bool open_key = false) { return data_set(sql, throwerr, table_name, open_key); }
    //public virtual DataTable dt_table (string sql, core cr, Dictionary<string, object> flds = null) {
    //  return data_table(cr.parse(sql, flds));
    //}
    public virtual DataTable dt_table(string sql, bool throwerr = true, bool open_key = false) {
      return data_table(sql, "", throwerr, open_key);
    }
    public virtual DataTable dt_table(string sql, string table_name, bool throwerr = true, bool open_key = false) { return data_table(sql, table_name, throwerr, open_key); }
    public virtual DataTable dt_schema(string sql, string table_name = "", bool throwerr = true) { return open_schema(sql, throwerr, table_name); }
    public virtual DataRow first_row(string sql, bool throwerr = true, bool open_key = false) { DataTable dt = dt_table(sql, throwerr, open_key); return dt != null && dt.Rows.Count > 0 ? dt.Rows[0] : null; }
    public virtual DbDataReader dt_reader(string sql, bool open_key = false) {
      bool opened = false;
      try {
        if (open_conn()) opened = true;

        log.log_sql(sql);

        DbCommand cmd = _conn.CreateCommand();
        cmd.CommandText = (open_key && _sql_key != "" ? _sql_key : "") + sql;
        cmd.CommandType = CommandType.Text;

        return cmd.ExecuteReader();
      } catch (Exception ex) { log.log_err_sql(ex, sql); throw ex; } finally { if (opened) close_conn(); }
    }

    protected DataSet data_set(string sql, bool throwerr = true, string table_name = "", bool open_key = false) {
      return (DataSet)open_set(sql, throwerr, true, table_name, open_key);
    }

    protected DataTable data_table(string sql, string table_name = "", bool throwerr = true, bool open_key = false) {
      return (DataTable)open_set(sql, throwerr, false, table_name, open_key);
    }

    protected object open_set(string sql, bool throwerr = true, bool dataset = true, string table_name = "", bool open_key = false) {
      bool opened = false;
      try {
        opened = open_conn();

        log.log_sql(sql);

        DbCommand cmd = _conn.CreateCommand();
        if (_trans != null) cmd.Transaction = _trans;
        cmd.CommandText = (open_key && _sql_key != "" ? _sql_key : "") + sql;
        if (_timeout > 0) cmd.CommandTimeout = _timeout;
        cmd.CommandType = CommandType.Text;

        DbDataAdapter ad = get_factory(_dbType).CreateDataAdapter();
        ad.SelectCommand = cmd;

        object ds = dataset ? (object)new DataSet() :
          (table_name != "" ? (object)new DataTable(table_name) : (object)new DataTable());
        if (dataset) ad.Fill((DataSet)ds, table_name != "" ? table_name : "table1");
        else ad.Fill((DataTable)ds);
        return ds;
      } catch (Exception ex) { log.log_err(sql); log.log_err(ex); if (throwerr) throw ex; else return null; } finally { if (opened) close_conn(); }
    }

    protected DataTable open_schema(string sql, bool throwerr = true, string table_name = "") {
      bool opened = false;
      try {
        opened = open_conn();

        log.log_sql(sql);

        DbCommand cmd = _conn.CreateCommand();
        if (_trans != null) cmd.Transaction = _trans;
        cmd.CommandText = sql;
        if (_timeout > 0) cmd.CommandTimeout = _timeout;
        cmd.CommandType = CommandType.Text;

        DbDataAdapter ad = get_factory(_dbType).CreateDataAdapter();
        ad.SelectCommand = cmd;

        DataTable dt = table_name != "" ? new DataTable(table_name) : new DataTable();
        ad.FillSchema(dt, SchemaType.Mapped);
        return dt;
      } catch (Exception ex) { log.log_err(sql); log.log_err(ex); if (throwerr) throw ex; else return null; } finally { if (opened) close_conn(); }
    }

    public bool check_exists(config.query qry, core cr, Dictionary<string, object> flds = null) {
      if (qry.queries.Count > 1)
        for (int n = 0; n < qry.queries.Count - 1; n++) exec(qry.queries[n]);
      DataTable dt = dt_table(cr.parse(qry.queries[qry.queries.Count() - 1], flds));
      return dt != null && dt.Rows.Count > 0;
    }

    public DataTable dt_qry(config.query qry, core cr, Dictionary<string, object> flds = null) {
      return dt_table(cr.parse(qry.text, flds));
    }

    public string exec_qry(config.query qry, core cr, Dictionary<string, object> flds = null, bool getidentity = false
      , DbParameter[] pars = null) {
      string res = null;
      if (qry.tp == config.query.tp_query.do_while) {
        foreach (DataRow dr in dt_table(qry.text_do).Rows)
          res = exec(cr.parse(qry.text_while, flds, dr), getidentity, pars: pars);
      } else {
        foreach (string txt in qry.queries) res = exec(cr.parse(txt, flds), getidentity, pars: pars);
      }
      return res;
    }

    public void exec_script(string sql_script, bool open_trans = false) {
      bool opened = false, trans = false;
      try {
        if (open_conn()) opened = true;

        if (open_trans && this.check_begin_trans()) trans = true;

        StringBuilder sb = new StringBuilder();
        foreach (string line in sql_script.Split(new string[2] { "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries)) {
          if (line.ToUpper().Trim() == "GO") {
            if (sb.Length > 0) this.exec(sb.ToString());
            sb.Clear();
          } else sb.AppendLine(line);
        }

        if (trans) this.commit();
      } catch (Exception ex) { if (trans) this.rollback(); log.log_err_sql(ex, sql_script); throw ex; } finally { if (opened) close_conn(); }

    }

    public string exec(string sql, bool getidentity = false, bool noidentity = false, bool open_key = false, DbParameter[] pars = null) {
      return execute(sql, getidentity, noidentity, open_key, pars);
    }

    protected string execute(string sql, bool getidentity = false, bool noidentity = false, bool open_key = false, DbParameter[] pars = null) {
      if (getidentity && _dbType != dbType.sqlserver)
        throw new Exception("GET IDENTITY supportato solo in SQLServer");

      string cur_sql = "";
      bool opened = false;
      try {
        if (open_conn()) opened = true;

        if (getidentity && !noidentity) sql += ";select SCOPE_IDENTITY();";

        DbCommand cmd = _conn.CreateCommand();
        if (_timeout > 0) cmd.CommandTimeout = _timeout;
        if (_trans != null) cmd.Transaction = _trans;

        if (pars != null) { foreach (DbParameter p in pars) cmd.Parameters.Add(p); }

        if (!getidentity) {
          if (_dbType == dbType.sqlserver) {
            long cc = 0;
            foreach (var s in sql.Split(new[] { "GO\r\n" }, StringSplitOptions.RemoveEmptyEntries)) {
              cur_sql = s; log.log_sql(cur_sql); cmd.CommandText = (open_key && _sql_key != "" ? _sql_key : "") + cur_sql; cc += (long)cmd.ExecuteNonQuery();
            }
            return cc.ToString();
          } else { cur_sql = sql; log.log_sql(cur_sql); cmd.CommandText = (open_key && _sql_key != "" ? _sql_key : "") + cur_sql; return ((long)cmd.ExecuteNonQuery()).ToString(); }
        } else { log.log_sql(sql); cmd.CommandText = (open_key && _sql_key != "" ? _sql_key : "") + sql; }

        DbDataReader reader = cmd.ExecuteReader();
        if (reader.HasRows) {
          reader.Read();
          string result = "";
          for (int i = 0; i < reader.FieldCount; i++)
            result += (i > 0 ? ";" : "") + (reader[i] != DBNull.Value ? Convert.ToString(reader[i]) : "");

          reader.Close();

          return result;
        } else reader.Close();

        return "";
      } catch (Exception ex) { log.log_err_sql(ex, cur_sql); throw ex; } finally { if (opened) close_conn(); }
    }

    public string key_conn(string key) { return _keys.ContainsKey(key.ToLower()) ? _keys[key.ToLower()] : ""; }

    public virtual System.Data.Common.DbParameter get_par(schema_field col) { throw new Exception("il provider " + _dbType.ToString() + " non supporta la funzionalità getParameter"); }

    public virtual System.Data.Common.DbCommand dt_command(string sql) { throw new Exception("il provider " + _dbType.ToString() + " non supporta la funzionalità openDataCommand"); }

    static public System.Xml.XmlDocument set_todoc(System.Data.DataTable dt) {

      // salvo xml 
      System.Xml.XmlDocument docSet = new System.Xml.XmlDocument();
      System.IO.StringWriter stringWriter = new System.IO.StringWriter();
      DataSet ds = new DataSet(); ds.Tables.Add(dt);
      ds.WriteXml(new System.Xml.XmlTextWriter(stringWriter), System.Data.XmlWriteMode.WriteSchema);
      docSet.LoadXml(stringWriter.ToString());

      System.Xml.XmlNamespaceManager nm = new System.Xml.XmlNamespaceManager(docSet.NameTable);
      nm.AddNamespace("xs", "http://www.w3.org/2001/XMLSchema");
      System.Xml.XmlNodeList fields = docSet.SelectNodes("/" + ds.DataSetName + "/xs:schema//xs:element[@name='" + ds.Tables[0].TableName
        + "']/xs:complexType/xs:sequence/xs:element", nm);
      if (fields == null || fields.Count == 0)
        return null;

      // costruzione xsl                
      System.Xml.Xsl.XslCompiledTransform xslDoc = new System.Xml.Xsl.XslCompiledTransform();
      string strXsl = "<?xml version='1.0'?>"
        + "<xsl:stylesheet version=\"1.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\">"
        + "<xsl:template match=\"/\">"
        + " <rows schema='xmlschema.datatable'>"
        + "  <xsl:for-each select=\"/" + ds.DataSetName + "/" + ds.Tables[0].TableName + "\"><row>"
        + string.Concat(fields.Cast<XmlNode>().Select(x =>
          string.Format("  <{0}><xsl:value-of select=\"{0}\"/></{0}>", x.Attributes["name"].Value)))
        + "  </row></xsl:for-each>"
        + " </rows></xsl:template></xsl:stylesheet>";

      System.Xml.XmlReader reader = System.Xml.XmlReader.Create(new System.IO.StringReader(strXsl));
      xslDoc.Load(reader);

      // trasformazione xml                                 
      System.Xml.XmlDocument docResult = new System.Xml.XmlDocument();
      System.IO.StringWriter xmlText = new System.IO.StringWriter();
      xslDoc.Transform(docSet, System.Xml.XmlWriter.Create(xmlText));
      //System.Text.Encoding e = xslDoc.OutputSettings.Encoding;

      docResult.LoadXml(xmlText.ToString());

      return docResult;
    }

    public bool like(string str, string wildcard) {
      wildcard = wildcard.Replace("%", "*");
      return new System.Text.RegularExpressions.Regex("^" + System.Text.RegularExpressions.Regex.Escape(wildcard).Replace(@"\*", ".*").Replace(@"\?", ".") + "$",
          System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline).IsMatch(str);
    }

    static public string name_field(string field) { return (field.IndexOf('.') >= 0 ? field.Substring(field.IndexOf('.') + 1) : field).Replace("[", "").Replace("]", ""); }

    #endregion

    #region base special functions

    public virtual void add_field(schema_field tblField, string tableName) { throw new Exception("il provider " + _dbType.ToString() + " non supporta la funzionalità add_field"); }

    public virtual void del_field(string field, string table) { throw new Exception("il provider " + _dbType.ToString() + " non supporta la funzionalità del_field"); }

    public virtual void upd_field(schema_field tblField, string tableName) { throw new Exception("il provider " + _dbType.ToString() + " non supporta la funzionalità upd_field"); }

    public virtual bool exist_field(string table, string col) { throw new Exception("il provider " + _dbType.ToString() + " non supporta la funzionalità exist_field"); }

    public virtual bool exist_table(string tableName) { throw new Exception("il provider " + _dbType.ToString() + " non supporta la funzionalità exist_table"); }

    public virtual List<string> tables(string likeName = "", string list = "") { throw new Exception("il provider " + _dbType.ToString() + " non supporta la funzionalità tables"); }

    public virtual List<string> synonims(string likeName) { return new List<string>(); }

    public virtual string module_text(string procName) { throw new Exception("il provider " + _dbType.ToString() + " non supporta la funzionalità module_text"); }

    public virtual List<string> functions(string likeName = "") { throw new Exception("il provider " + _dbType.ToString() + " non supporta la funzionalità functions"); }

    public virtual void drop_function(string func) { throw new Exception("il provider " + _dbType.ToString() + " non supporta la funzionalità drop_function"); }

    public virtual List<string> store_procedures(string likeName = "") { throw new Exception("il provider " + _dbType.ToString() + " non supporta la funzionalità store_procedures"); }

    public virtual bool there_schema(string schema) { throw new Exception("il provider " + _dbType.ToString() + " non supporta la funzionalità there_schema"); }

    public virtual void create_schema(string schemaName) { throw new Exception("il provider " + _dbType.ToString() + " non supporta la funzionalità create_schema"); }

    public virtual bool exist_function(string function) { throw new Exception("il provider " + _dbType.ToString() + " non supporta la funzionalità exist_function"); }

    public virtual bool exist_procedure(string procedure) { throw new Exception("il provider " + _dbType.ToString() + " non supporta la funzionalità exist_procedure"); }

    public virtual void drop_procedure(string proc) { throw new Exception("il provider " + _dbType.ToString() + " non supporta la funzionalità drop_procedure"); }

    public virtual void drop_table(string table) { throw new Exception("il provider " + _dbType.ToString() + " non supporta la funzionalità drop_table"); }

    public virtual void rename_table(string old_name, string new_name) { throw new Exception("il provider " + _dbType.ToString() + " non supporta la funzionalità rename_table"); }

    public virtual void truncate_table(string table) { throw new Exception("il provider " + _dbType.ToString() + " non supporta la funzionalità truncate_table"); }

    public virtual void set_identity(string table, bool on) { throw new Exception("il provider " + _dbType.ToString() + " non supporta la funzionalità set_identity"); }

    // truncate table
    public virtual void clean_table(string table, string where = null) {
      exec("DELETE FROM " + table + ""
          + (where != null && where != "" ? " WHERE " + where : ""));
    }

    public schema_field field_table(string table, string nameField) {
      List<schema_field> list = table_fields(table, nameField);
      return list.Count == 0 ? null : list[0];
    }

    // tableFields
    public virtual List<schema_field> table_fields(string table, string nameField = "") { throw new Exception("il provider " + _dbType.ToString() + " non supporta la funzionalità tableFields"); }

    public schema_field find_field(List<schema_field> list, string fieldName) {
      for (int i = 0; i < list.Count; i++)
        if (list[i].name.ToLower() == fieldName.ToLower())
          return list[i];

      return null;
    }

    public idx_table find_index(List<idx_table> list, string indexName, string tableName) {
      for (int i = 0; i < list.Count; i++)
        if (list[i].name.ToLower() == indexName.ToLower()
            && list[i].table_name.ToLower() == tableName.ToLower())
          return list[i];

      return null;
    }

    public bool remove_field(List<schema_field> list, string fieldName) {
      bool result = false;

      for (int i = 0; i < list.Count; i++) {
        if (list[i].name.ToLower() == fieldName.ToLower()) {
          list.RemoveAt(i);
          result = true;
          i--;
        }
      }

      return result;
    }

    public virtual void drop_field(schema_field field, string tableName) {
      throw new Exception("il provider " + _dbType.ToString() + " non supporta la funzionalità dropFieldToTable");
    }

    public virtual void drop_index(string indexName, string tableName) {
      throw new Exception("il provider " + _dbType.ToString() + " non supporta la funzionalità dropIndex");
    }

    public virtual void drop_foreign(string foreignName, string tableName) {
      throw new Exception("il provider " + _dbType.ToString() + " non supporta la funzionalità dropForeignKey");
    }

    public virtual void alter_field(schema_field tblfield, string tableName) {
      throw new Exception("il provider " + _dbType.ToString() + " non supporta la funzionalità alterFieldTable");
    }

    public virtual void create_table(XmlNode tableNode, dbType tp_original) {
      create_table(tableNode, true, "", null, tp_original);
    }

    public virtual void create_table(XmlNode tableNode, string nameCreated, List<string> flds_null = null) {
      create_table(tableNode, true, nameCreated, flds_null);
    }

    public virtual void create_table(XmlNode tableNode, bool createIndexes = true, string nameCreated = "", List<string> flds_null = null, dbType tp_original = dbType.none) {
      throw new Exception("il provider " + _dbType.ToString() + " non supporta la funzionalità createTable");
    }

    public virtual idx_table create_index(idx_table index) {
      throw new Exception("il provider " + _dbType.ToString() + " non supporta la funzionalità createIndex");
    }

    public virtual List<idx_table> table_idxs(string table, bool? uniques = null, string index_name = "") {
      throw new Exception("il provider " + _dbType.ToString() + " non supporta la funzionalità tableIndexes");
    }

    public idx_table table_pk(string table) { return table_idxs(table).FirstOrDefault(x => x.primary); }

    public virtual void repair_autoincrements() {
      throw new Exception("il provider " + _dbType.ToString() + " non supporta la funzionalità repairAutoIncrements");
    }

    public string field_value(object val, string null_val = null) {
      return val == DBNull.Value || val == null ? null_val
        : (val.GetType().FullName == "System.DateTime" ? ((DateTime)val).ToString("yyyy-MM-dd HH:mm:ss.fffffff")
          : val.ToString());
    }

    public virtual string val_toqry(string value, fieldType coltype, dbType type, string nullstr = null, bool add_operator = false, bool tostring = false) {
      throw new Exception("il provider " + _dbType.ToString() + " non supporta la funzionalità valueToQuery");
    }

    public virtual string val_toqry(string value, fieldType coltype, string nullstr = null, bool add_operator = false, bool tostring = false) {
      return val_toqry(value, coltype, _dbType, nullstr, add_operator, tostring);
    }

    public virtual string quadra(string element) { return element; }

    public string original_typefld(string type, dbType tp_original) {
      return _dbType == tp_original ? type : schema_field.type_to_original(_dbType, schema_field.original_to_type(tp_original, type));
    }

    #endregion

    #region tools

    public string get_list(string sql, string field) { return string.Join(",", dt_table(sql).Rows.Cast<DataRow>().Select(r => r[field].ToString())); }

    public long get_count(string sql) {
      return dt_table(sql).Rows[0][0] != null && dt_table(sql).Rows[0][0] != DBNull.Value
        ? Convert.ToInt64(dt_table(sql).Rows[0][0]) : 0;
    }

    public long get_n_rows(string sql) { DataTable dt = dt_table(sql); return dt != null ? dt.Rows.Count : -1; }

    public object get_value(string sql) { return dt_table(sql).Rows[0][0]; }

    public DateTime? get_date(string sql) {
      object val = dt_table(sql).Rows[0][0];
      return val == null || val == DBNull.Value ? (DateTime?)null : DateTime.Parse(val.ToString());
    }

    public string get_string(string sql, string null_val = null) {
      object v = dt_table(sql).Rows[0][0];
      return v != null && v != DBNull.Value ? v.ToString() : null_val;
    }

    public static DateTime? max_date(DateTime? dt_1, DateTime? dt_2) {
      if (dt_1.HasValue && dt_2.HasValue) return dt_1 > dt_2 ? dt_1 : dt_2;
      else return dt_1.HasValue ? dt_1 : dt_2.HasValue ? dt_2 : (DateTime?)null;
    }

    public static string join_fields(DataRow row) {
      return string.Join(", ", row.Table.Columns.Cast<DataColumn>().Select(col => col.ColumnName + ": " + (row[col.ColumnName] != DBNull.Value
        ? row[col.ColumnName].ToString().Replace("\n", " ").Replace("\r", " ") : "")));
    }

    public static string join_fields(Dictionary<string, string> fields) {
      return string.Join(", ", fields.Select(kv => kv.Key + ": " + (kv.Value != null ? kv.Value : "")));
    }

    public virtual string enc_qry(string val) { return val.Replace("'", "''"); }

    #endregion
  }
}
