using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace dn_lib.db
{
  public class schema_field
  {
    protected dbType _type_db = dbType.none;
    protected string _name = "", _attr_name = "", _original_type = "", _default = "";
    protected bool _request = false, _auto_number = false, _primary = false;
    protected int? _max_length = null, _num_prec = null, _num_scale = null;
    protected int _index = 0;

    public schema_field(dbType type_db, string name, string original_type, bool nullable, int? max_length = null, int? num_prec = null, int? num_scale = null
        , string def_val = "", bool auto_number = false, bool primary = false, int index = 0, string attr_name = "") {
      _type_db = type_db; _name = name; _original_type = original_type; _request = !nullable;
      _max_length = max_length; _num_prec = num_prec; _num_scale = num_scale;
      _default = def_val; _auto_number = auto_number; _primary = primary; _index = index; _attr_name = attr_name;
    }

    public dbType type_db { get { return _type_db; } }
    public string name { get { return _name; } }
    public string attr_name { get { return _attr_name; } set { _attr_name = value; } }
    public string original_type { get { return _original_type; } }
    public string def_val { get { return _default; } }
    public bool request { get { return _request; } }
    public bool nullable { get { return !_request; } set { _request = !value; } }
    public bool auto_number { get { return _auto_number; } }
    public int? max_length { get { return _max_length; } }
    public int? num_prec { get { return _num_prec; } }
    public int? num_scale { get { return _num_scale; } }
    public bool primary { get { return _primary; } }
    public int index { get { return _index; } }

    public static fieldType original_to_type(dbType type_db, string type) {
      string tp = type.ToLower();

      if (type_db == dbType.access) {
        if (tp == "binary" || tp == "varbinary" || tp == "longvarbinary") return fieldType.BINARY;
        else if (tp == "boolean") return fieldType.BOOL;
        else if (tp == "currency") return fieldType.MONEY;
        else if (tp == "date" || tp == "dbtime" || tp == "dbtimestamp"
            || tp == "filetime" || tp == "dbdate") return fieldType.DATETIME;
        else if (tp == "guid") return fieldType.GUID;
        else if (tp == "double") return fieldType.DOUBLE;
        else if (tp == "single") return fieldType.SINGLE;
        else if (tp == "smallint" || tp == "unsignedsmallint" || tp == "tinyint" || tp == "unsignedtinyint")
          return fieldType.SMALLINT;
        else if (tp == "integer" || tp == "bigint"
            || tp == "unsignedbigint" || tp == "unsignedint") return fieldType.INTEGER;
        else if (tp == "varnumeric" || tp == "decimal" || tp == "numeric") return fieldType.DECIMAL;
        else if (tp == "bstr" || tp == "varchar" || tp == "longvarchar" || tp == "varwchar" || tp == "longvarwchar")
          return fieldType.VARCHAR;
        else if (tp == "char" || tp == "wchar") return fieldType.CHAR;
      } else if (type_db == dbType.sqlserver) {
        if (tp == "smalldatetime") return fieldType.SMALLDATETIME;
        else if (tp == "sql_variant") return fieldType.VARIANT;
        else if (tp == "timestamp") return fieldType.TIMESTAMP;
        else if (tp == "date") return fieldType.DATE;
        else if (tp == "datetime") return fieldType.DATETIME;
        else if (tp == "datetime2") return fieldType.DATETIME2;
        else if (tp == "tinyint" || tp == "smallint") return fieldType.SMALLINT;
        else if (tp == "int") return fieldType.INTEGER;
        else if (tp == "bigint") return fieldType.LONG;
        else if (tp == "bit") return fieldType.BOOL;
        else if (tp == "real" || tp == "float" || tp == "bit") return fieldType.DOUBLE;
        else if (tp == "money" || tp == "smallmoney") return fieldType.MONEY;
        else if (tp == "numeric" || tp == "decimal") return fieldType.DECIMAL;
        else if (tp == "xml") return fieldType.XML;
        else if (tp == "text" || tp == "ntext") return fieldType.TEXT;
        else if (tp == "image") return fieldType.IMAGE;
        else if (tp == "varchar" || tp == "nvarchar") return fieldType.VARCHAR;
        else if (tp == "varbinary") return fieldType.BINARY;
        else if (tp == "char" || tp == "nchar") return fieldType.CHAR;
        else if (tp == "uniqueidentifier") return fieldType.GUID;
      } else if (type_db == dbType.mysql) {
        if (tp == "bigint") return fieldType.LONG;
        else if (tp == "binary") return fieldType.BINARY;
        else if (tp == "bit") return fieldType.BOOL;
        else if (tp == "blob") return fieldType.BINARY;
        else if (tp == "char") return fieldType.CHAR;
        else if (tp == "date") return fieldType.DATETIME;
        else if (tp == "datetime") return fieldType.DATETIME;
        else if (tp == "decimal") return fieldType.DECIMAL;
        else if (tp == "double" || tp == "float") return fieldType.DOUBLE;
        else if (tp == "enum") return fieldType.VARCHAR;
        else if (tp == "geometry") return fieldType.BINARY;
        else if (tp == "int") return fieldType.INTEGER;
        else if (tp == "longblob") return fieldType.BINARY;
        else if (tp == "longtext") return fieldType.TEXT;
        else if (tp == "mediumblob") return fieldType.BINARY;
        else if (tp == "mediumint") return fieldType.INTEGER;
        else if (tp == "mediumtext") return fieldType.TEXT;
        else if (tp == "set") return fieldType.VARCHAR;
        else if (tp == "smallint" || tp == "tinyint") return fieldType.SMALLINT;
        else if (tp == "text") return fieldType.TEXT;
        else if (tp == "time") return fieldType.DATETIME;
        else if (tp == "timestamp") return fieldType.DATETIME;
        else if (tp == "varbinary") return fieldType.BINARY;
        else if (tp == "varchar") return fieldType.VARCHAR;
        else if (tp == "year") return fieldType.INTEGER;
      } else if (type_db == dbType.odbc) {
        if (tp == "system.string") return fieldType.VARCHAR;
        else if (tp == "system.double") return fieldType.DOUBLE;
        else if (tp == "system.datetime") return fieldType.DATETIME;
        else if (tp == "system.boolean") return fieldType.BOOL;
      }

      throw new Exception("type field '" + type + "' not supported for '" + type_db.ToString() + "'");
    }

    public void set_original_type(dbType type_db, fieldType type) { _original_type = type_to_original(type_db, type); }

    public static string type_to_original(dbType type_db, fieldType type) {

      if (type_db == dbType.access) {
        if (type == fieldType.BINARY) return "binary";
        else if (type == fieldType.BOOL) return "boolean";
        else if (type == fieldType.MONEY) return "currency";
        else if (type == fieldType.DATETIME) return "date";
        else if (type == fieldType.GUID) return "guid";
        else if (type == fieldType.DOUBLE) return "double";
        else if (type == fieldType.SINGLE) return "single";
        else if (type == fieldType.SMALLINT) return "smallint";
        else if (type == fieldType.INTEGER) return "integer";
        else if (type == fieldType.DECIMAL) return "decimal";
        else if (type == fieldType.VARCHAR) return "varchar";
        else if (type == fieldType.CHAR) return "char";
      } else if (type_db == dbType.sqlserver) {
        if (type == fieldType.BINARY) return "varbinary";
        else if (type == fieldType.BOOL) return "bit";
        else if (type == fieldType.MONEY) return "money";
        else if (type == fieldType.SMALLDATETIME) return "smalldatetime";
        else if (type == fieldType.VARIANT) return "sql_variant";
        else if (type == fieldType.TIMESTAMP) return "timestamp";
        else if (type == fieldType.DATE) return "date";
        else if (type == fieldType.DATETIME) return "datetime";
        else if (type == fieldType.DOUBLE) return "float";
        else if (type == fieldType.SMALLINT) return "smallint";
        else if (type == fieldType.INTEGER) return "int";
        else if (type == fieldType.LONG) return "bigint";
        else if (type == fieldType.DECIMAL) return "decimal";
        else if (type == fieldType.VARCHAR) return "varchar";
        else if (type == fieldType.CHAR) return "char";
        else if (type == fieldType.TEXT) return "text";
        else if (type == fieldType.IMAGE) return "image";
        else if (type == fieldType.XML) return "xml";
        else if (type == fieldType.GUID) return "uniqueidentifier";
      } else if (type_db == dbType.mysql) {
        if (type == fieldType.BINARY) return "varbinary";
        else if (type == fieldType.BOOL) return "bit";
        else if (type == fieldType.MONEY) return "decimal";
        else if (type == fieldType.DATETIME) return "datetime";
        else if (type == fieldType.DOUBLE) return "double";
        else if (type == fieldType.SMALLINT) return "smallint";
        else if (type == fieldType.INTEGER) return "int";
        else if (type == fieldType.LONG) return "bigint";
        else if (type == fieldType.DECIMAL) return "decimal";
        else if (type == fieldType.VARCHAR) return "varchar";
        else if (type == fieldType.CHAR) return "char";
        else if (type == fieldType.TEXT) return "text";
        else if (type == fieldType.XML) return "varchar";
      } else if (type_db == dbType.odbc) {
        if (type == fieldType.VARCHAR) return "System.String";
        else if (type == fieldType.DOUBLE) return "System.Double";
        else if (type == fieldType.DATETIME) return "System.DateTime";
        else if (type == fieldType.BOOL) return "System.Boolean";
      }

      throw new Exception("type field '" + type.ToString() + "' not supported for '" + type_db.ToString() + "'");
    }

    public fieldType type_field { get { return original_to_type(_type_db, _original_type); } }

    static public schema_field find(List<schema_field> list, string fieldName) {
      for (int i = 0; i < list.Count; i++)
        if (list[i].name.ToLower() == fieldName.ToLower())
          return list[i];

      return null;
    }

    static public bool remove_field(List<schema_field> list, string fld_name) {
      bool result = false;

      for (int i = 0; i < list.Count; i++) {
        if (list[i].name.ToLower() == fld_name.ToLower()) {
          list.RemoveAt(i);
          result = true;
          i--;
        }
      }

      return result;
    }

    public string fld_sql_server() {
      return fld_sql_server(name, _original_type, num_prec.HasValue ? num_prec.ToString() : ""
          , num_scale.HasValue ? num_scale.ToString() : "", max_length.HasValue ? max_length.ToString() : "", nullable, def_val, _auto_number);
    }

    public string fld_access() {
      return fld_access(name, type_field, num_prec.HasValue ? num_prec.ToString() : ""
          , num_scale.HasValue ? num_scale.ToString() : "", max_length.HasValue ? max_length.ToString() : ""
          , !request, def_val, auto_number);
    }

    static public string fld_access(string name, fieldType type, string numprec, string numscale, string maxlength, bool nullable, string defaultval, bool autonumber) {
      string sql = "[" + name + "]";

      if (autonumber) sql += " AUTOINCREMENT";
      else if (type == fieldType.BINARY) sql += " BINARY";
      else if (type == fieldType.BOOL) sql += " BIT";
      else if (type == fieldType.SMALLINT) sql += " SMALLINT";
      else if (type == fieldType.MONEY) sql += " MONEY";
      else if (type == fieldType.DATETIME) sql += " DATETIME";
      else if (type == fieldType.GUID) sql += " UNIQUEIDENTIFIER";
      else if (type == fieldType.DOUBLE) sql += " DOUBLE";
      else if (type == fieldType.SINGLE) sql += " SINGLE";
      else if (type == fieldType.INTEGER) sql += " INTEGER";
      else if (type == fieldType.LONG) sql += " LONG";
      else if (type == fieldType.DECIMAL) sql += " DECIMAL(" + numprec + ", " + numscale + ")";
      else if (type == fieldType.VARCHAR || type == fieldType.CHAR) {
        if (maxlength == "0") sql += " MEMO";
        else sql += " VARCHAR(" + (maxlength == "-1" ? "255" : maxlength) + ")";
      } else
        throw new Exception("il campo '" + type.ToString() + "' di access non viene gestito per l'aggiornamento struttura tabelle");

      if (defaultval != "") sql += " DEFAULT " + defaultval;
      if (!nullable) sql += " NOT NULL";

      return sql;
    }

    public static bool is_fld_num_sql_server(string type) { return (type.ToLower() == "tinyint" || type.ToLower() == "int" || type.ToLower() == "bigint" || type.ToLower() == "smallint") ? true : false; }

    static public string fld_sql_server(string name, string type, string numprec, string numscale, string maxlength, bool nullable, string defaultval, bool autonumber) {
      string sql = "[" + name + "]";

      if (type == "smalldatetime" || type == "date" || type == "datetime" || type == "datetime2" || type == "timestamp"
        || type == "image" || type == "text" || type == "ntext" || type == "xml"
        || type == "tinyint" || type == "int" || type == "bigint" || type == "smallint" || type == "real"
        || type == "float" || type == "bit" || type == "money" || type == "smallmoney" || type == "uniqueidentifier" || type == "sql_variant")
        sql += " [" + type + "]";
      else if (type == "numeric" || type == "decimal")
        sql += " [" + type + "](" + numprec + ", " + numscale + ")";
      else if (type == "char" || type == "varchar" || type == "nchar" || type == "nvarchar" || type == "varbinary")
        sql += " [" + type + "](" + (maxlength == "-1" ? "max" : maxlength) + ")";
      else throw new Exception("il campo '" + type + "' di sql server non viene gestito per l'aggiornamento struttura tabelle");

      sql += autonumber ? " IDENTITY(1, 1)" : (nullable ? " NULL" : " NOT NULL" + (defaultval != "" ? " DEFAULT " + defaultval : ""));

      return sql;
    }
  }
}