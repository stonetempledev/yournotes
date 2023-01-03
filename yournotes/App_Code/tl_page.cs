using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.Security;
using System.Net;
using System.Net.Mail;
using System.Net.Configuration;
using System.Configuration;
using System.IO;
using System.Data;
using dn_lib;
using dn_lib.tools;
using dn_lib.db;
using dn_lib.xml;
using deepanotes;

public class tl_page : System.Web.UI.Page {

  protected core _core = null;
  protected db_provider _db = null;
  protected user _user = null;
  protected bool _db_connected = false;
  protected bool _is_mobile = false;

  protected string _base_path = null;
  public string base_path { get { if (_base_path == null) _base_path = System.Web.HttpContext.Current.Server.MapPath("~"); return _base_path; } }
  //public string base_url_request {
  //  get { return Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/"; }
  //}
  public string abs_path { get { return HttpContext.Current.Request.Url.AbsolutePath; } }
  public string page_name { get { return (new FileInfo(abs_path)).Name; } }

  protected override void OnInit(EventArgs e) {
    base.OnInit(e);

    // base
    _is_mobile = this.master.is_mobile();
  }

  protected override void OnPreInit(EventArgs e) {

    // check user
    string u_name = ""; int u_id = -1;
    if (User.Identity.IsAuthenticated) {
      FormsIdentity id = (FormsIdentity)User.Identity;
      FormsAuthenticationTicket ticket = id.Ticket;
      u_name = ticket.Name.Split(new char[] { '|' })[0];
      u_id = int.Parse(ticket.Name.Split(new char[] { '|' })[1]);
    }

    try {

      // core
      bool reload_cfg = false;
      if (Cache["core_obj"] == null) {
        log.log_info("reload core");
        core cr = new core(base_path, mobile: master.is_mobile());
        reload_cfg = true;
        Cache["core_obj"] = cr;
      }

      // configs
      _core = (core)Cache["core_obj"];
      //_core.base_url = this.base_url;
      foreach (string key in _core.config_keys) if (Cache[key] == null) { reload_cfg = true; break; }
      reload_cfg = true;
      if (reload_cfg) {
        log.log_info("reload config docs");
        _core.reset_configs();

        // docs
        Dictionary<string, xml_doc> docs = new Dictionary<string, xml_doc>();
        Directory.EnumerateFiles(_core.app_setting("settings-folder")).ToList().ForEach(f => {
          string doc_key = strings.rel_path(base_path, f), vars_key = Path.GetFileNameWithoutExtension(f).ToLower();
          log.log_info("load xml config doc: " + doc_key + " - " + f);
          docs.Add(string.Format("{0};{1};{2}", doc_key, vars_key, f), Path.GetExtension(f) != _core.app_setting("enc-ext-xml") ? new xml_doc(f)
            : new xml_doc(cry.xml_decrypt(f, _core.app_setting("pwdcr-xml"))));
        });

        // vars
        foreach (KeyValuePair<string, xml_doc> d in docs) {
          try {
            string[] keys = d.Key.Split(new char[] { ';' });
            string doc_key = keys[0], vars_key = keys[1], f = keys[2];
            log.log_info("load vars doc: " + doc_key + " - " + f);
            _core.load_base_config(d.Value, doc_key, vars_key);
          } catch (Exception ex) { string err = ex.Message; }
        }

        // docs
        foreach (KeyValuePair<string, xml_doc> d in docs) {
          try {
            string[] keys = d.Key.Split(new char[] { ';' });
            string doc_key = keys[0], vars_key = keys[1], f = keys[2];
            log.log_info("load config doc: " + doc_key + " - " + f);
            _core.load_config(d.Value, doc_key, db_conn, new Dictionary<string, object>() { { "user_id", u_id } }, vars_key);
            if (Cache[doc_key] == null) Cache.Insert(doc_key, true, new System.Web.Caching.CacheDependency(f));
          } catch (Exception ex) { string err = ex.Message; }
        }

        Cache["core_obj"] = _core;
      }
    } catch (Exception ex) { throw ex; }

    // carico il config page
    string ap = abs_path, base_dir = Path.GetDirectoryName(HttpContext.Current.Server.MapPath(ap))
      , pn = Path.GetFileNameWithoutExtension(ap), xml = Path.Combine(base_dir, pn + ".xml")
      , xml_enc = Path.Combine(base_dir, pn + "." + _core.app_setting("enc-ext-xml")), dck = strings.rel_path(base_path, xml);

    xml_doc dp = null;
    if (Cache[dck] != null) dp = (xml_doc)Cache[dck];
    else {
      dp = File.Exists(xml) ? new xml_doc(xml) : (File.Exists(xml_enc) ?
        new xml_doc(cry.xml_decrypt(xml_enc, _core.app_setting("pwdcr-xml"))) : null);
      if (dp != null) Cache.Insert(dck, dp, new System.Web.Caching.CacheDependency(xml));
    }

    if (dp != null) { _core.reset_page_config(); _core.load_page_config(dp, dck, db_conn, new Dictionary<string, object>() { { "user_id", u_id } }); }

    // conn to db
    db_reconn();
  }

  public bool db_reconn(bool throw_err = false) {
    try {
      db_provider db = db_conn;
      if (db.is_opened()) db.close_conn();
      _db_connected = db.open_conn();
    } catch (Exception ex) { log.log_err(ex); _db_connected = false; if (throw_err) throw ex; }
    return _db_connected;
  }

  public bool close_conn() {
    bool res = false;
    try {
      db_provider db = db_conn;
      if (db.is_opened()) { db.close_conn(); res = true; }
      _db_connected = false;
    } catch (Exception ex) { log.log_err(ex); _db_connected = false; }
    return res;
  }

  public config.conn cfg_conn { get { return _core.config.get_conn(_core.config.get_var("lib-vars.db-connection").value); } }
  public db_provider conn_to(string conn_name) { return db_provider.create_provider(_core.config.get_conn(conn_name)); }
  public db_provider db_conn { get { return _db != null ? _db : _db = db_provider.create_provider(cfg_conn); } }
  public bool db_connected { get { return _db_connected; } }
  public core core { get { return _core; } }
  public config config { get { return _core.config; } }
  public bool is_user { get { return _user != null; } }
  public bool is_admin { get { return _user != null && _user.type == deepanotes.user.type_user.admin; } }
  public user user { get { return _user; } protected set { _user = value; } }
  public void set_user(int id, string user, string email, user.type_user tp) {
    _user = new user(id, user, email, tp);
  }
  public tl_master master { get { return (tl_master)Master; } }

  protected void write_response(json_result res) {
    Response.Clear();
    Response.ContentType = "application/json";
    Response.Write(Newtonsoft.Json.JsonConvert.SerializeObject(res));
    Response.Flush();
    Response.SuppressContent = true;
    HttpContext.Current.ApplicationInstance.CompleteRequest();
  }

  protected bool base_elab_action(json_request jr, json_result res)
  {
    if(jr.action == "client_cmd") {
      set_client_cmd(jr.val_str("cmd"));
      return true;
    }
    return false;
  }

  #region data

  public string val_str(object val, string def = "") { return val != null && val != DBNull.Value ? val.ToString() : def; }
  public int val_int(object val) { return int.Parse(val_str(val, "0")); }
  public string qry_val(string name, string def = "") { return Request.QueryString[name] != null ? Request.QueryString[name] : def; }
  public int qry_int(string name, int def = 0) { return Request.QueryString[name] != null ? int.Parse(Request.QueryString[name]) : def; }

  #endregion

  #region controls

  public HtmlForm main_form(bool throw_err = true) {
    HtmlForm result = ctrl_by_attr<HtmlForm>("mainform", "true");
    if (result == null && throw_err)
      throw new Exception("devi aggiungere al documento il form principale con l'attributo \"mainForm='true'\"");

    return result;
  }

  public T ctrl_by_attr<T>(string attr, string attr_val) where T : HtmlControl { return ctrl_by_attr<T>(this, attr, attr_val); }

  protected T ctrl_by_attr<T>(Control ctl, string attr, string attr_val) where T : HtmlControl {
    foreach (Control c in ctl.Controls) {
      if (c.GetType() == typeof(T) && ((T)c).Attributes[attr] != null && ((T)c).Attributes[attr] == attr_val)
        return (T)c;

      Control cb = ctrl_by_attr<T>(c, attr, attr_val);
      if (cb != null) return (T)cb;
    }
    return null;
  }

  protected void add_class(HtmlControl ctrl, string class_name) {
    if (ctrl.Attributes["class"] == null) ctrl.Attributes.Add("class", class_name);
    else if (!ctrl.Attributes["class"].Contains(" " + class_name) && ctrl.Attributes["class"] != class_name)
      ctrl.Attributes["class"] += " " + class_name;
  }

  protected void remove_class(HtmlControl ctrl, string class_name) {
    if (ctrl.Attributes["class"] != null) {
      if (ctrl.Attributes["class"] == class_name) ctrl.Attributes["class"] = "";
      else if (ctrl.Attributes["class"].Contains(" " + class_name))
        ctrl.Attributes["class"] = ctrl.Attributes["class"].Replace(" " + class_name, "");
    }
  }

  #endregion

  #region functionalities

  protected void send_mail(string to, string obj, string body) {
    SmtpSection cfg = (SmtpSection)ConfigurationManager.GetSection("mailSettings/" + _core.network_key());
    using (MailMessage mm = new MailMessage() {
      From = new MailAddress(cfg.From), Subject = obj, IsBodyHtml = true, Body = body
    }) {
      mm.To.Add(to);
      using (SmtpClient smtp = new SmtpClient(cfg.Network.Host, cfg.Network.Port) {
        EnableSsl = cfg.Network.EnableSsl, UseDefaultCredentials = cfg.Network.DefaultCredentials,
        Credentials = new NetworkCredential(cry.decrypt(cfg.Network.UserName, "lillo32!"), cry.decrypt(cfg.Network.Password, "lillo32!"))
      }) smtp.Send(mm);
    }
  }

  public void log_out(string to_page) {
    FormsAuthentication.SignOut();
    Response.Redirect(to_page);
  }

  protected string get_url_cmd(string ref_url) { return _core.config.var_value_par("lib-vars.router-cmd", System.Web.HttpUtility.UrlEncode(ref_url)); }

  protected List<element_cut> elements_cut {
    get {
      if(Session["elements_cut"] == null) Session["elements_cut"] = new List<element_cut>();
      return (List<element_cut>)Session["elements_cut"];
    }
  }

  protected bool there_element_cut(int id, element_cut.element_cut_type tp) { return elements_cut.FirstOrDefault(x => x.tp == tp && x.id == id) != null; }

  protected element_cut find_element_cut(int id, element_cut.element_cut_type tp) { return elements_cut.FirstOrDefault(x => x.tp == tp && x.id == id); }

  protected void remove_element_cut(int id, element_cut.element_cut_type tp) { elements_cut.Remove(find_element_cut(id, tp)); }

  protected bool? set_element_cut(int id, element_cut.element_cut_type tp, bool copy = false) {
    element_cut ec = find_element_cut(id, tp);
    if(ec == null) { elements_cut.Add(new element_cut(id, tp, copy)); return true; }
    if(ec.copy != copy) { ec.copy = copy; return null; }
    remove_element_cut(id, tp);
    return false;
  }

  protected void set_client_cmd(string cmd)
  {
    if(master.client_key == "") return;
    db_conn.exec(core.parse(config.get_query("lib-base.set-client-cmd").text
      , new Dictionary<string, object>() { { "client_key", master.client_key }, { "cmd", cmd } }));
  }

  #endregion

  #region cache

  public bool set_cache_var(string var_name, string var_value)
  {
    if(!is_user) return false;
    int user_id = _user.id;
    return string.IsNullOrEmpty(var_value) ? reset_cache_var(var_name, user_id)
      : set_cache_var2(var_name, var_value, user_id);
  }

  private bool set_cache_var2(string var_name, string var_value, int user_id)
  {
    db_conn.exec(core.parse(config.get_query("lib-base.set-cache-var").text
      , new Dictionary<string, object>() { { "user_id", user_id }, { "var_name", var_name }, { "var_value", var_value } }));
    return true;
  }

  protected bool reset_cache_var(string var_name, int user_id)
  {
    db_conn.exec(core.parse(config.get_query("lib-base.reset-cache-var").text
      , new Dictionary<string, object>() { { "user_id", user_id }, { "var_name", var_name } }));
    return true;
  }

  public string get_cache_var(string var_name, string def = "")
  {
    if(!is_user) return def;
    int user_id = _user.id;
    DataRow dr = db_conn.first_row(core.parse(config.get_query("lib-base.get-cache-var").text
      , new Dictionary<string, object>() { { "user_id", user_id }, { "list_vars", "'" + var_name + "'" } }));
    return dr != null ? db_provider.str_val(dr["var_value"], def) : def;
  }

  #endregion

}