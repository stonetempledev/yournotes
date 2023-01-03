using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;
using dn_lib.xml;
using dn_lib.db;

namespace dn_lib.tools {
  public class config {

    #region structure

    public class base_class {
      bool _for_page = false;
      public bool for_page { get { return _for_page; } }

      string _doc_key = "";
      public string doc_key { get { return _doc_key; } }

      public base_class(string doc_key, bool for_pg) { _doc_key = doc_key; _for_page = for_pg; }
    }

    // var
    public class var : base_class {
      string _name, _value;
      public var(string doc_key, string name, string value, bool for_pg = false) : base(doc_key, for_pg) { _name = name; _value = value; }

      public string name { get { return _name; } }
      public string value { get { return _value; } set { _value = value; } }
    }

    // folder
    public class folder : base_class {
      string _name, _path;
      public folder(string doc_key, string name, string path, bool for_pg = false) : base(doc_key, for_pg) { _name = name; _path = path; }

      public string name { get { return _name; } }
      public string path { get { return _path; } set { _path = value; } }
    }

    // conn
    public class conn : base_class {
      string _name, _des, _conn, _provider, _date_format, _sql_key, _key;
      int _timeout = 0;
      public conn(string doc_key, string name, string conn, string provider
        , string des, string date_format, int timeout, string key, string sql_key, bool for_pg = false)
        : base(doc_key, for_pg) {
        _name = name; _conn = conn; _provider = provider;
        _des = des; _date_format = date_format; _timeout = timeout; _key = ""; _sql_key = sql_key;
      }

      public string name { get { return _name; } }
      public string sql_key { get { return _sql_key; } }
      public string key { get { return _key; } }
      public string des { get { return _des; } set { _des = value; } }
      public string conn_string { get { return _conn; } set { _conn = value; } }
      public string provider { get { return _provider; } set { _provider = value; } }
      public string date_format { get { return _date_format; } set { _date_format = value; } }
      public int timeout { get { return _timeout; } set { _timeout = value; } }
    }

    // query
    public class query : base_class {
      public enum tp_query { normal, do_while }
      tp_query _tp = tp_query.normal;
      string _name, _des, _do, _while;
      List<string> _queries = new List<string>();
      Dictionary<string, string> _conds = new Dictionary<string, string>();
      public query(string doc_key, string name, string txt, string des = "", bool for_pg = false)
        : base(doc_key, for_pg) {
        _name = name; _des = des; _queries.Add(txt);
      }
      public query(string doc_key, string name, string des = "", bool for_pg = false)
        : base(doc_key, for_pg) {
        _name = name; _des = des;
      }
      public tp_query tp { get { return _tp; } set { _tp = value; } }
      public string text_do { get { return _do; } set { _do = value; } }
      public string text_while { get { return _while; } set { _while = value; } }
      public string des { get { return _des; } }
      public string name { get { return _name; } }
      public string text { get { return _queries[0]; } }
      public void add_query(string txt) { _queries.Add(txt); }
      public List<string> queries { get { return _queries; } }
      public int count { get { return _queries.Count(); } }
      public Dictionary<string, string> conds { get { return _conds; } }
      public void add_cond(string name, string txt) {
        if (_conds.ContainsKey(name))
          throw new Exception("c'è già una condizione '" + name + "' nella query '" + _name + "'!");
        _conds.Add(name, txt);
      }
      public string get_cond(string name) {
        if (!_conds.ContainsKey(name))
          throw new Exception("non c'è la condizione '" + name + "' nella query '" + _name + "'!");
        return _conds[name];
      }
    }

    // table
    public class table : base_class {
      string _name; List<string> _cols; List<table_row> _rows;

      public table(string doc_key, string name, xml_node tbl, bool for_pg = false)
        : base(doc_key, for_pg) {
        _name = name; _rows = new List<table_row>();
        _cols = tbl.get_attr("cols").Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        foreach (xml_node row in tbl.nodes("rows/row"))
          _rows.Add(new table_row(this, _cols.Select(c => row.get_attr(c)).ToList()));
      }

      public string name { get { return _name; } }
      public int i_col(string name) { return _cols.IndexOf(name); }
      public List<table_row> rows { get { return _rows; } }
      public List<table_row> rows_ordered(string col) { return _rows.OrderBy(r => r.field(col)).ToList(); }
      public List<table_row> rows_ordered(string col, string col2) { return _rows.OrderBy(r => r.field(col) + r.field(col2)).ToList(); }
      public List<table_row> rows_ordered(string col, string col2, string col3) { return _rows.OrderBy(r => r.field(col) + r.field(col2) + r.field(col3)).ToList(); }
      public table_row find_row(Dictionary<string, string> keys) {
        foreach (table_row r in _rows) if (is_row(r, keys)) return r; return null;
      }
      public List<table_row> find_rows(Dictionary<string, string> keys) {
        return _rows.Where(r => is_row(r, keys)).ToList();
      }
      protected bool is_row(table_row tr, Dictionary<string, string> keys) {
        foreach (KeyValuePair<string, string> kv in keys)
          if (tr.field(kv.Key) != kv.Value) return false;
        return true;
      }
    }

    public class table_row {
      table _tbl; List<string> _fields;
      public table_row(table tbl, List<string> vals) { _tbl = tbl; _fields = new List<string>(vals); }
      public string field(string col) { return _fields[_tbl.i_col(col)]; }
      public string this[string col] { get { return field(col); } }
      public bool fld_bool(string col) {
        string val = _fields[_tbl.i_col(col)];
        return val != "" ? bool.Parse(val) : false;
      }
    }

    // html-block
    public class html_block : base_class {
      public string name { get; set; }
      public string content { get; set; }
      public html_block(string doc_key, string name, string content, bool for_pg = false)
        : base(doc_key, for_pg) {
        this.name = name; this.content = content; 
      }

      // conds
      Dictionary<string, string> _conds = new Dictionary<string, string>();
      public Dictionary<string, string> conds { get { return _conds; } }
      public void add_cond(string name, string txt) {
        if (_conds.ContainsKey(name))
          throw new Exception("c'è già una condizione '" + name + "' nel blocco '" + this.name + "'!");
        _conds.Add(name, txt);
      }
      public string get_cond(string name) {
        if (!_conds.ContainsKey(name))
          throw new Exception("non c'è la condizione '" + name + "' nel blocco '" + this.name + "'!");
        return _conds[name];
      }

    }

    #endregion

    core _core = null;
    Dictionary<string, folder> _folders = new Dictionary<string, folder>();
    Dictionary<string, var> _vars = new Dictionary<string, var>();
    Dictionary<string, conn> _conns = new Dictionary<string, conn>();
    Dictionary<string, table> _tables = new Dictionary<string, table>();
    Dictionary<string, html_block> _html_blocks = new Dictionary<string, html_block>();
    Dictionary<string, query> _queries = new Dictionary<string, query>();

    public config(core cr) { _core = cr; }

    public void reset() { _tables.Clear(); _folders.Clear(); _vars.Clear(); _conns.Clear(); _html_blocks.Clear(); _queries.Clear(); }

    public void read_vars(xml_doc doc, Dictionary<string, var> dv, string var_key = "", string doc_key = "", bool for_pg = false
      , string xpath = "")
    {
      // vars
      string nkey = "";
      try {
        foreach(xml_node vars in doc.nodes(xpath == "" ? "/config/vars" : xpath)) {
          string network = vars.get_attr("network"), bname = vars.get_attr("name");
          foreach(xml_node var in vars.nodes("var")) {
            string network2 = var.get_attr("network") != "" ? var.get_attr("network") : network;
            if(network2 != "" && _core.network_key() != network2.ToLower()) continue;
            nkey = var_key + bname + var.get_attr("name");
            if(dv.Keys.Contains(nkey)) dv.Remove(nkey);
            dv.Add(nkey, new var(doc_key, nkey, _core.parse(var.get_val()), for_pg));
          }
        }
      } catch(Exception ex) { throw new Exception("chiave vars.'" + nkey + "' - " + ex.Message); }
    }


    public void load_base_config(string doc_key, string vars_key, xml_doc doc, bool for_pg = false) {
      string var_key = !string.IsNullOrEmpty(vars_key) ? vars_key + "." : "";

      // vars
      read_vars(doc, _vars, var_key, doc_key, for_pg);

      // conns
      string nkey = "";
      try {
        foreach (xml_node var in doc.nodes("/config//conns/conn")) {
          nkey = var_key + var.get_attr("name");
          _conns.Add(nkey, new conn(doc_key, nkey, var.get_attr("conn-string"), var.get_attr("provider"), var.get_attr("des")
            , var.get_attr("date-format"), var.get_int("timeout", 0), var.get_attr("key"), var.sub_node("sql_key").text, for_pg));
        }
      } catch (Exception ex) { throw new Exception("chiave lib-conns.'" + nkey + "' - " + ex.Message); }
    }

    public void load_doc(string doc_key, string vars_key, xml_doc doc, db_provider conn, Dictionary<string, object> keys = null, bool for_pg = false) {

      string var_key = !string.IsNullOrEmpty(vars_key) ? vars_key + "." : "";

      // sql-select
      if (doc.exists("//sql-select")) {
        foreach (xml_node s in doc.nodes("//sql-select")) {
          xml_node ref_node = s;
          foreach (DataRow r in conn.dt_table(_core.parse(s.get_attr("qry"), keys)).Rows) {
            foreach (xml_node n in _core.parse_nodes(s.clone_childs(s), keys, r))
              ref_node = ref_node.add_after(n);
          }
        }
        while (true) { xml_node s = doc.node("//sql-select"); if (!s.remove()) break; }
      }

      // aggiungo
      string nkey = "";
      try {
        foreach (xml_node var in doc.nodes("/config//folders/folder")) {
          nkey = var_key + var.get_attr("name");
          _folders.Add(nkey, new folder(doc_key, nkey, _core.parse(var.get_val()), for_pg));
        }
      } catch (Exception ex) { throw new Exception("chiave folders.'" + nkey + "' - " + ex.Message); }

      nkey = "";
      try {
        foreach (xml_node tbl in doc.nodes("/config//tables/table")) {
          nkey = var_key + tbl.get_attr("name");
          _tables.Add(nkey, new table(doc_key, nkey, tbl, for_pg));
        }
      } catch (Exception ex) { throw new Exception("chiave tables.'" + nkey + "' - " + ex.Message); }

      nkey = "";
      try {
        foreach (xml_node tbl in doc.nodes("/config/html-blocks/html-block")) {
          nkey = var_key + tbl.get_attr("name");
          html_block b = new html_block(doc_key, nkey, tbl.text, for_pg);
          foreach (xml_node f in tbl.nodes("cond"))
            b.add_cond(f.get_attr("name"), f.text);
          _html_blocks.Add(nkey, b);
        }
      } catch (Exception ex) { throw new Exception("chiave html-blocks.'" + nkey + "' - " + ex.Message); }

      nkey = "";
      try {
        foreach (xml_node qry in doc.nodes("/config//queries/*")) {
          if (qry.name == "query") {
            nkey = var_key + qry.get_attr("name");
            query q = new query(doc_key, nkey, qry.text, qry.get_attr("des"), for_pg);
            foreach (xml_node f in qry.nodes("cond"))
              q.add_cond(f.get_attr("name"), f.text);
            _queries.Add(nkey, q);
          } else if (qry.name == "query_do") {
            nkey = var_key + qry.get_attr("name");
            _queries.Add(nkey, new query(doc_key, nkey, qry.get_attr("des"), for_pg) {
              tp = query.tp_query.do_while,
              text_do = qry.sub_node("do").text, text_while = qry.sub_node("while").text
            });
          } else if (qry.name == "queries") {
            nkey = var_key + qry.get_attr("name");
            query q = new query(doc_key, nkey, qry.get_attr("des"), for_pg);
            _queries.Add(nkey, q);
            foreach (xml_node q2 in qry.nodes("*")) {
              if (q2.name == "query") q.add_query(q2.text);
              else if (q2.name == "exec_query") {
                foreach (string q3 in _queries[q2.get_attr("name")].queries) q.add_query(q3);
              }
            }
          }
        }
      } catch (Exception ex) { throw new Exception("chiave queries.'" + nkey + "' - " + ex.Message); }
    }

    public void remove_for_page() {
      string k = _folders.FirstOrDefault(x => x.Value.for_page == true).Key;
      while (k != null) { _folders.Remove(k); k = _folders.FirstOrDefault(x => x.Value.for_page == true).Key; }

      k = _vars.FirstOrDefault(x => x.Value.for_page == true).Key;
      while (k != null) { _vars.Remove(k); k = _vars.FirstOrDefault(x => x.Value.for_page == true).Key; }

      k = _conns.FirstOrDefault(x => x.Value.for_page == true).Key;
      while (k != null) { _conns.Remove(k); k = _conns.FirstOrDefault(x => x.Value.for_page == true).Key; }

      k = _tables.FirstOrDefault(x => x.Value.for_page == true).Key;
      while (k != null) { _tables.Remove(k); k = _tables.FirstOrDefault(x => x.Value.for_page == true).Key; }

      k = _html_blocks.FirstOrDefault(x => x.Value.for_page == true).Key;
      while (k != null) { _html_blocks.Remove(k); k = _html_blocks.FirstOrDefault(x => x.Value.for_page == true).Key; }

      k = _queries.FirstOrDefault(x => x.Value.for_page == true).Key;
      while (k != null) { _queries.Remove(k); k = _queries.FirstOrDefault(x => x.Value.for_page == true).Key; }
    }

    public bool exists_var(string name) { return _vars.ContainsKey(name); }
    public string var_value(string name) { if (!_vars.ContainsKey(name)) throw new Exception("la variabile '" + name + "' non esiste!"); return _vars[name].value; }
    public bool var_bool(string name) { string val = var_value(name); if (string.IsNullOrEmpty(val) || val.ToLower() == "false" || val == "0") return false; return true; }
    public var get_var(string name) { if (!_vars.ContainsKey(name)) throw new Exception("la variabile '" + name + "' non esiste!"); return _vars[name]; }
    public string var_value_par(string name, string par) { if (!_vars.ContainsKey(name)) throw new Exception("la variabile '" + name + "' non esiste!"); return _vars[name].value.Replace("[@par]", par); }
    public folder get_folder(string name) { if (!_folders.ContainsKey(name)) throw new Exception("il folder '" + name + "' non esiste!"); return _folders[name]; }
    public conn get_conn(string name) { if (!_conns.ContainsKey(name)) throw new Exception("la connessione '" + name + "' non esiste!"); return _conns[name]; }
    public table get_table(string name) { if (!_tables.ContainsKey(name)) throw new Exception("la tabella '" + name + "' non esiste!"); return _tables[name]; }
    public html_block get_html_block(string name) { if (!_html_blocks.ContainsKey(name)) throw new Exception("il blocco html '" + name + "' non esiste!"); return _html_blocks[name]; }
    public query get_query(string name) { if (!_queries.ContainsKey(name)) throw new Exception("la query '" + name + "' non esiste!"); return _queries[name]; }
  }
}
