using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.Security;
using dn_lib.db;
using dn_lib.tools;
using deepanotes;

public partial class _default : tl_master {

  protected Dictionary<string, string> _qvals = null;

  protected void Page_Init(object sender, EventArgs e) {

    try {
      // base
      bool mb = is_mobile();
      this.head.Controls.Add(new Literal() { Text = @"<link href=""" + (mb ? "base-mobile.css" : "base.css") + @""" type=""text/css"" rel=""stylesheet"" />" });

      // check user
      string u_name = ""; int u_id = -1;
      FormsIdentity id = (FormsIdentity)tlp.User.Identity;
      FormsAuthenticationTicket ticket = id.Ticket;
      u_name = ticket.Name.Split(new char[] { '|' })[0];
      u_id = int.Parse(ticket.Name.Split(new char[] { '|' })[1]);

      // check utente
      string u_email = ""; user.type_user u_tp = user.type_user.none;
      if (cry.encode_tobase64(u_name) == tlp.core.app_setting("ad-usr")) {
        u_email = cry.decrypt(tlp.core.app_setting("ad-mail"), "kokko32!");
        u_id = 0; u_tp = user.type_user.admin;
      } else {
        DataRow dr = tlp.db_conn.first_row(@"select nome, email, isnull(activated, 0) as activated 
          from users where user_id = " + u_id.ToString());
        if (dr == null || Convert.ToInt16(dr["activated"]) != 1) { tlp.log_out("login.aspx"); return; }
        u_email = dr["email"].ToString(); u_tp = user.type_user.normal;
      }

      tlp.set_user(u_id, u_name, u_email, u_tp);
      __utype.Value = u_tp.ToString();

      __sid.Value = this.Session.SessionID;

      // command
      txt_cmd.Value = this.cmd = tlp.qry_val("cmd");

      // client key    
      if(tlp.qry_val("ck") != "") {
        this.client_key = tlp.qry_val("ck");
        Session["ck"] = tlp.qry_val("ck");
        DataRow r = tlp.db_conn.first_row(tlp.core.parse_query("lib-base.client-session-id"
          , new string[,] { { "session_id", Session.SessionID }, { "client_key", this.client_key } }), true);
        if(!(r != null && (int)r[0] == 1)) this.client_key = "";
      } else {
        this.client_key = (Session["ck"] != null ? Session["ck"].ToString() : "");
        if(this.client_key != "") {
          DataRow r = tlp.db_conn.first_row(tlp.core.parse_query("lib-base.client-session-id"
            , new string[,] { { "session_id", Session.SessionID }, { "client_key", this.client_key } }), true);
          if(!(r != null && (int)r[0] == 1)) this.client_key = "";
        }
      }

      // parameters
      _qvals = new Dictionary<string, string>();
      foreach (string qs in Request.QueryString.AllKeys.Where(x => x != "cmd"))
        _qvals.Add(qs, tlp.qry_val(qs));

    } catch { }
  }

  protected void Page_Load(object sender, EventArgs e)
  {
    // navbar admin
    if(tlp.user != null && tlp.user.type == user.type_user.admin) {
      string cl = navbar.Attributes["class"];
      navbar.Attributes["class"] = cl.Replace("bg-primary", "bg-warning");
    }
    
    icona_normal.Visible = this.client_key == "" ? true : false;
    if(this.client_key != "") {
      icona_client.Style["display"] = "";
    }    
  }

  public override void set_status_txt(string text) {
    string cl = navbar.Attributes["class"];
    navbar.Attributes["class"] = cl.Replace("bg-primary", "bg-success");
    grp_vai.Style.Add(HtmlTextWriterStyle.Display, "none");
    lbl_status.Style[HtmlTextWriterStyle.Display] = "";
    lbl_status.InnerText = text;
  }

  public override string get_val(string id) {
    Control ctrl = FindControl(id); return ctrl != null ? (ctrl is HtmlInputText ? ((HtmlInputText)ctrl).Value
      : (ctrl is HtmlInputHidden ? ((HtmlInputHidden)ctrl).Value : "")) : "";
  }

  public override void set_val(string id, string val) {
    Control ctrl = FindControl(id);
    if (ctrl != null && ctrl is HtmlInputText) ((HtmlInputText)ctrl).Value = val;
    else if (ctrl != null && ctrl is HtmlInputHidden) ((HtmlInputHidden)ctrl).Value = val;
  }

  #region cmds

  public override cmd check_cmd(string cmd) {
    cmd c = new cmd(cmd), c2 = new cmd("[action] " + cmd);
    config.table_row tr = null;
    foreach (config.table_row r in config.get_table("cmds.base-cmds").rows) {
      string action = r.field("action"), obj = r.field("object")
        , synobj = r.field("syn-object"), subobj = r.field("subobj");
      bool action_opt = r.fld_bool("action-opt");
      if (is_like_cmd(action, c.action) && is_like_cmd(obj, c.obj, synobj) && is_like_cmd(subobj, c.sub_obj())) {
        tr = r; c.obj = obj.Contains("{") ? c.obj : obj; break;
      }
      if (action_opt) {
        if (is_like_cmd(obj, c2.obj, synobj) && is_like_cmd(subobj, c2.sub_obj())) {
          tr = r; c2.action = action; c2.obj = obj.Contains("{") ? c.obj : obj; c = c2; break;
        }
      }
    }
    if (tr == null) return null;
    c.group = tr.field("group");
    c.code = tr.field("code");
    c.sub_code = tr.field("sub-code");
    c.type = tr.field("type");
    c.page = tr.field("page");

    cmd_action.Value = c.action;
    cmd_obj.Value = c.obj;

    return c;
  }

  // SINTASSI DICHIARAZIONE object, subobj
  //  keyword - parola chiave che rappresenta un oggetto o un'azione particolare
  //  {par_value} - valore variabile dell'object o del subobj
  //  name_parameter:{par_value} - valore variabile accompagnato al nome del parametro associato all'object o al subobj
  protected bool is_like_cmd(string syntax, string txt, string sinonimi) {
    bool res = false;
    List<string> stxs = new List<string>() { syntax };
    stxs.AddRange(sinonimi.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
    foreach (string s in stxs) {
      if (is_like_cmd(s, txt)) { res = true; break; }
    }
    return res;
  }
  protected bool is_like_cmd(string syntax, string txt) {
    bool res = false;
    if (string.IsNullOrEmpty(syntax) && string.IsNullOrEmpty(txt)) res = true;
    else if (!string.IsNullOrEmpty(syntax) && !string.IsNullOrEmpty(txt)) {
      if (syntax.Contains('{')) {
        int pos = syntax.IndexOf('{');
        if (pos == 0) res = true;
        else {
          string name_par = syntax.Substring(0, pos);
          if (txt.Length >= name_par.Length && txt.Substring(0, pos) == name_par) res = true;
        }
      } else {
        if (txt == syntax) res = true;
      }
    }
    return res;
  }

  protected void Cmd_Click(object sender, EventArgs e) { elab_cmd(config.get_var("lib-vars.router-page").value); }

  public void elab_cmd(string page, string cmd = "") { Response.Redirect(url_cmd(cmd != "" ? cmd : txt_cmd.Value, page)); }

  public override void redirect_to(string page) {
    string url = page + "?cmd=" + HttpUtility.UrlEncode(txt_cmd.Value);
    foreach (string qs in _qvals.Keys)
      url += "&" + qs + "=" + HttpUtility.UrlEncode(_qvals[qs]);
    Response.Redirect(url);
  }

  #endregion
}
