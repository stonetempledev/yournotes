using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;
using dn_lib.tools;
using dn_lib.xml;
using dn_lib.db;

namespace dn_lib
{
  public class core
  {
    protected System.Xml.XPath.XPathNavigator _eval = new System.Xml.XPath.XPathDocument(new System.IO.StringReader("<r/>")).CreateNavigator();
    Dictionary<string, string> _cfg_keys = new Dictionary<string, string>();

    protected bool? _mobile = null;
    public bool? mobile { get { return _mobile; } set { _mobile = value; } }

    protected string _base_path = "";
    public string base_path { get { return _base_path; } }

    protected string _base_url = "";
    public string base_url { get { return _base_url; } set { _base_url = value; } }

    protected config _config = null;
    public config config { get { return _config; } }

    string _network = null;
    public string network_key() { if(string.IsNullOrEmpty(_network)) throw new Exception("network non individuato!"); return _network; }

    public core(string base_path, string base_url = "", bool? mobile = null)
    {
      _base_path = base_path;
      _mobile = mobile;
      _config = new config(this);
      read_base_settings();
    }

    protected void read_base_settings()
    {
      xml_doc bdoc = new xml_doc(app_setting("settings-file", false));

      // network
      _network = bdoc.node("/base/networks/network").text;

      // vars
      Dictionary<string, config.var> vars = new Dictionary<string, config.var>();
      _config.read_vars(bdoc, vars, xpath: "/base/vars");
      _base_url = parse(vars["base_url"].value);
    }

    #region configs

    public void reset_configs() { _cfg_keys.Clear(); _config.reset(); }

    public void load_base_config(xml_doc doc, string doc_key, string vars_key = "", bool page = false)
    {
      try {
        if(!_cfg_keys.Keys.Contains(doc_key)) _cfg_keys.Add(doc_key, "doc_path");
        _config.load_base_config(doc_key, vars_key, doc, page);
      } catch(Exception ex) { _cfg_keys.Clear(); throw new Exception("caricamento documento '" + doc.path + "': " + ex.Message); }
    }

    public void load_config(xml_doc doc, string doc_key, db_provider conn, Dictionary<string, object> keys = null, string vars_key = "", bool page = false)
    {
      try {
        if(!_cfg_keys.Keys.Contains(doc_key)) _cfg_keys.Add(doc_key, "doc_path");
        _config.load_doc(doc_key, vars_key, doc, conn, keys, page);
      } catch(Exception ex) { _cfg_keys.Clear(); throw new Exception("caricamento documento '" + doc.path + "': " + ex.Message); }
    }

    public void load_page_config(xml_doc doc, string doc_key, db_provider conn, Dictionary<string, object> keys)
    {
      load_config(doc, doc_key, conn, keys, page: true);
    }

    public List<xml_node> parse_nodes(List<xml_node> l, Dictionary<string, object> keys, DataRow dr = null)
    {
      l.ForEach(x => parse_node(x, keys, dr)); return l;
    }

    public xml_node parse_node(xml_node n, Dictionary<string, object> keys, DataRow dr = null)
    {

      // text
      if(!string.IsNullOrEmpty(n.text)) n.text = parse(n.text, keys, dr);

      // attributes
      foreach(string a in n.get_attrs())
        n.set_attr(a, parse(n.get_attr(a), keys, dr));

      // childs
      foreach(xml_node nc in n.childs)
        parse_node(nc, keys, dr);

      return n;
    }

    public void reset_page_config() { _config.remove_for_page(); }

    public List<string> config_keys { get { return _cfg_keys.Keys.ToList(); } }

    public string app_setting(string name, bool throw_err = true)
    {
      if(System.Configuration.ConfigurationManager.AppSettings[name] == null) {
        if(throw_err) throw new Exception("non c'è la variabile '" + name + "' nel config!");
        return "";
      }
      return parse(System.Configuration.ConfigurationManager.AppSettings[name] != null ?
        System.Configuration.ConfigurationManager.AppSettings[name].ToString() : "");
    }

    public string parse_query(string key) { return parse(_config.get_query(key).text); }

    public string parse_query(string key, string[,] flds)
    {
      Dictionary<string, object> dict = new Dictionary<string, object>();
      for(int i = 0; i < flds.GetLength(0); i++)
        dict.Add(flds[i, 0], flds[i, 1]);
      config.query q = _config.get_query(key);
      return parse(q.text, dict, conds: q.conds);
    }

    public string parse_query(string key, Dictionary<string, object> flds)
    {
      config.query q = _config.get_query(key);
      return parse(q.text, flds, conds: q.conds);
    }

    public string parse_html_block(string key) { return parse(_config.get_html_block(key).content); }

    public string parse_html_block(string key, string[,] flds)
    {
      Dictionary<string, object> dict = new Dictionary<string, object>();
      for(int i = 0; i < flds.GetLength(0); i++)
        dict.Add(flds[i, 0], flds[i, 1]);

      config.html_block b = _config.get_html_block(key);
      return parse(b.content, dict, conds: b.conds);
    }

    public string parse_html_block(string key, Dictionary<string, object> flds)
    {
      config.html_block b = _config.get_html_block(key);
      return parse(b.content, flds, conds: b.conds);
    }

    #endregion

    #region parse

    public string parse(string text, Dictionary<string, object> flds = null, DataRow dr = null
      , Dictionary<string, string> conds = null)
    {

      try {

        int nstart = text != null ? text.IndexOf("{@") : -1; //string from = log.log_debugging && nstart >= 0 ? text : null;
        while(nstart >= 0) {
          int nend = text.IndexOf("}", nstart + 2);
          string cnt = text.Substring(nstart + 2, nend - nstart - 2);
          int nuguale = cnt.IndexOf("='");

          // chiavi secche: {@key}
          if(nuguale < 0) {
            switch(cnt) {
              // {@basepath}
              case "basepath": text = text.Replace("{@basepath}", _base_path); break;
              // {@baseurl}
              case "baseurl": text = text.Replace("{@baseurl}", _base_url.Replace("\\", "/")); break;
              // {@machine-ip}
              case "machine-ip": text = text.Replace("{@machine-ip}", sys.machine_ip()); break;
              // {@machine-name}
              case "machine-name": text = text.Replace("{@machine-name}", sys.machine_name()); break;
              default: throw new Exception("chiave '" + cnt + "' inaspettata");
            }
          }
          // chiavi con uno o più parametri: {@key='value'} oppure {@key='value','value 2'}
          else if(nuguale > 0) {
            string cmd = cnt.Substring(0, nuguale), par = cnt.Substring(nuguale + 2, cnt.Length - nuguale - 3), value = "";
            switch(cmd) {
              // {@field='<FIELD NAME>'}, {@field='<FIELD NAME>','<DEF. VALUE>'}
              case "field": {
                if(par.Contains(',')) {
                  string par21 = par.Split(new string[] { "','" }, StringSplitOptions.RemoveEmptyEntries)[0]
                    , par22 = par.Split(new string[] { "','" }, StringSplitOptions.RemoveEmptyEntries)[1];
                  value = get_val(par21, flds, dr, par22);
                } else value = get_val(par, flds, dr);
                break;
              }
              // {@cond_val='<FIELD NAME>','<COND NAME>'}
              case "cond_val": {
                string par21 = par.Split(new string[] { "','" }, StringSplitOptions.RemoveEmptyEntries)[0]
                  , par22 = par.Split(new string[] { "','" }, StringSplitOptions.RemoveEmptyEntries)[1]
                  , val = get_val(par21, flds, dr);
                if(val == "") {
                  value = "";
                } else {
                  if(conds == null)
                    throw new Exception("non sono stati specificate le condizioni per l'istruzione '{@" + cmd + "='" + par + "'}'");
                  if(!conds.ContainsKey(par22))
                    throw new Exception("la condizione '" + par22 + "' non è specificata per l'istruzione '{@" + cmd + "='" + par + "'}'");
                  value = parse(conds[par22], flds, dr);
                }

                break;
              }
              // {@cond_bool='<FIELD NAME>','<COND NAME>','<NO COND NAME>'}
              case "cond_bool": {
                string[] pars = par.Split(new string[] { "','" }, StringSplitOptions.None);
                string par21 = pars[0], par22 = pars[1], par23 = pars.Length > 2 ? pars[2] : "", val = get_val(par21, flds, dr);
                // falso
                if(val == "" || !bool.Parse(val)) {
                  if(par23 != "" && conds == null)
                    throw new Exception("non sono stati specificate le condizioni per l'istruzione '{@" + cmd + "='" + par + "'}'");
                  if(par23 != "" && !conds.ContainsKey(par23))
                    throw new Exception("la condizione '" + par23 + "' non è specificata per l'istruzione '{@" + cmd + "='" + par + "'}'");
                  value = par23 != "" ? parse(conds[par23], flds, dr) : "";
                } // vero
                else if(par22 != "") {
                  if(conds == null)
                    throw new Exception("non sono stati specificate le condizioni per l'istruzione '{@" + cmd + "='" + par + "'}'");
                  if(!conds.ContainsKey(par22))
                    throw new Exception("la condizione '" + par22 + "' non è specificata per l'istruzione '{@" + cmd + "='" + par + "'}'");
                  value = parse(conds[par22], flds, dr);
                }

                break;
              }
              // {@null='<FIELD NAME>'}
              case "null": {
                value = get_val(par, flds, dr, "null");
                break;
              }
              // {@txtqry='<FIELD NAME>'}
              case "txtqry": {
                string val = get_val(par, flds, dr).Replace("'", "''").Replace("\r\n", " ").Replace("\r", " ").Replace("\n", " ");
                value = val != "" ? string.Format("'{0}'", val) : "NULL";
                break;
              }
              // {@dateqry='<FIELD NAME>'}
              case "dateqry": {
                DateTime dt;
                value = DateTime.TryParse(get_val(par, flds, dr), out dt) ? db_provider.dt_qry(dt) : "NULL";
                break;
              }
              // {@txtvoid='<FIELD NAME>'}
              case "txtvoid": {
                string val = get_val(par, flds, dr).Replace("'", "''").Replace("\r\n", " ").Replace("\r", " ").Replace("\n", " ");
                value = val != "" ? string.Format("'{0}'", val) : "''";
                break;
              }
              // {@txtqrycr='<FIELD NAME>'}
              case "txtqrycr": {
                string val = get_val(par, flds, dr).ToString().Replace("'", "''").Replace("\r\n", "' + char(13) + '").Replace("\r", " ").Replace("\n", " ");
                value = val != "" ? string.Format("'{0}'", val) : "NULL";
                break;
              }
              // {@html-block='<NAME KEY>'}
              case "html-block": {
                value = parse(config.get_html_block(par).content, flds, dr, conds);
                break;
              }
              // {@query-text='<NAME QUERY'}
              case "query-text": value = parse(config.get_query(par).text, flds, dr, conds); break;
              // {@var='<NAME KEY>'}
              case "var": value = config.get_var(par).value; break;
              // {@varurl='<NAME KEY>','<PARAMETER ENCODED>'}
              case "varurl": {
                string par21 = par.Split(new string[] { "','" }, StringSplitOptions.RemoveEmptyEntries)[0]
                  , par22 = par.Split(new string[] { "','" }, StringSplitOptions.RemoveEmptyEntries)[1];
                value = config.var_value_par(par21, System.Web.HttpUtility.UrlEncode(par22));
                break;
              }
              // {@setting='<NAME KEY>'}
              case "setting": value = app_setting(par); break;
              // {@date='<FORMAT STRING>'}
              case "date": value = DateTime.Now.ToString(par); break;
              // {@date_field='<FIELD NAME>','<FORMAT STRING>'}
              case "date_field": {
                string par21 = par.Split(new string[] { "','" }, StringSplitOptions.RemoveEmptyEntries)[0]
                  , par22 = par.Split(new string[] { "','" }, StringSplitOptions.RemoveEmptyEntries)[1];
                DateTime? dt = db_provider.dt_val(get_val(par21, flds, dr));
                value = dt.HasValue ? dt.Value.ToString(par22) : ""; break;
              }
              // {@date_fld='<FIELD NAME>','<FORMAT STRING>'}
              case "date_fld": {
                string par21 = par.Split(new string[] { "','" }, StringSplitOptions.RemoveEmptyEntries)[0]
                  , par22 = par.Split(new string[] { "','" }, StringSplitOptions.RemoveEmptyEntries)[1];
                object val = get_obj(par21, flds, dr);
                value = val != null && val is DateTime ? ((DateTime)val).ToString(par22) : "";
                break;
              }
              default: throw new Exception("chiave con parametro '" + cmd + "' inaspettata!");
            }

            text = text.Replace("{@" + cmd + "='" + par + "'}", value);
          }
          nstart = text.IndexOf("{@");
        }

        //if (from != null) log.log_debug(string.Format("parsed '{0}' -> '{1}'", from, text));

        return text;
      } catch(Exception ex) { log.log_err(ex); throw ex; }
    }

    protected static string get_val(string par, Dictionary<string, object> flds = null, DataRow dr = null, string def = "")
    {
      object res = get_obj(par, flds, dr);
      return res != null && res.ToString() != "" ? res.ToString() : def;
    }

    protected static object get_obj(string par, Dictionary<string, object> flds = null, DataRow dr = null)
    {

      // non è un oggetto
      if(!par.Contains('.')) {
        if(flds == null && dr == null) throw new Exception("il campo '" + par + "' specificato non è stato impostato per assenza di parametri!");
        if(flds != null && flds.ContainsKey(par)) return flds[par] != null ? flds[par] : null;
        else if(dr != null && dr.Table.Columns.Contains(par)) return dr[par] != DBNull.Value ? dr[par] : null;
        throw new Exception("il campo '" + par + "' specificato non è stato impostato!");
      }

      // è un oggetto
      string cl = par.Substring(0, par.IndexOf('.')), pr = par.Substring(par.IndexOf('.') + 1);
      if(flds == null || !flds.ContainsKey(cl)) throw new Exception("l'oggetto '" + cl + "' non è stato trovato!");
      object o = flds[cl];
      if(o == null) return null;

      // funzione
      if(pr.Contains("(")) {
        string pars = pr.Substring(pr.IndexOf("(") + 1, pr.Length - pr.IndexOf("(") - 2);
        string[] values = pars.Split(new char[] { ';' });
        System.Reflection.MethodInfo mi = o.GetType().GetMethod(pr.Substring(0, pr.IndexOf("(")));
        List<object> pvals = new List<object>(); int i = 0;
        foreach(System.Reflection.ParameterInfo pi in mi.GetParameters()) {
          if(values.Length > i) pvals.Add(Convert.ChangeType(values[i], pi.ParameterType));
          else pvals.Add(null);
          i++;
        }
        object val = mi.Invoke(o, pvals.ToArray());
        return val != null ? val.ToString() : null;
      } // attributo
      else {
        if(o.GetType().GetProperty(pr) == null)
          throw new Exception("l'oggetto '" + cl + "' di tipo '" + o.GetType().FullName + "' non contiente la proprietà '" + pr + "'!");
        return o.GetType().GetProperty(pr).GetValue(o, null);
      }
    }

    public double eval_double(string expr) { try { return (double)_eval.Evaluate(string.Format("number({0})", expr)); } catch(Exception ex) { log.log_err(expr); throw ex; } }

    public bool eval_bool(string expr) { try { return (bool)_eval.Evaluate(string.Format("boolean({0})", expr)); } catch(Exception ex) { log.log_err(expr); throw ex; } }

    #endregion

    #region common

    public static string[] split(string lst) { return lst.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries); }
    public static int[] split_ints(string lst)
    {
      return lst.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => int.Parse(s)).ToArray();
    }

    #endregion
  }
}
