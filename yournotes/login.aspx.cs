using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;
using System.Data;
using dn_lib.tools;
using dn_lib.db;

public partial class login : tl_page {
  protected void Page_Load (object sender, EventArgs e) {
    if (!IsPostBack && qry_val("nm") != "") { user_mail.Value = qry_val("nm"); }
    if (!db_connected) { btn_new.Visible = false; btn_lost.Visible = false; }
  }

  protected void Logon_Click (object sender, EventArgs e) {
    try {
      if (user_mail.Value == "") err_login("DEVI SCRIVERE CHI SEI!");
      else if (user_pass.Value == "") err_login("DEVI SCRIVERE LA PASSWORD!");
      else {
        // admin
        if (cry.encode_tobase64(user_mail.Value) == core.app_setting("ad-usr")) {
          string uname = user_mail.Value, upass = user_pass.Value, uid = "0";
          if (cry.encode_tobase64(upass) != core.app_setting("ad-pwd")) {
            err_login("PASSWORD ERRATA!"); return;
          }
          FormsAuthentication.RedirectFromLoginPage(uname + "|" + uid, true);
        }
          // utente
        else {
          string uname = user_mail.Value, upass = user_pass.Value;
          DataRow dr = db_conn.first_row(@"select [user_id], nome, pwd, email, isnull(activated, 0) as activated 
          from users where nome = '" + uname + "' order by isnull(activated, 0) desc;");
          if (dr == null) {
            err_login("NON SEI REGISTRATO!"); return;
          } else if (Convert.ToInt16(dr["activated"]) == 0) {
            err_login("NON SEI ATTIVO!"); return;
          } else if (Convert.ToInt16(dr["activated"]) == 2 || Convert.ToInt16(dr["activated"]) == 3) {
            err_login("DEVI CONFERMARE LA TUA ISCRIZIONE!"); return;
          } else if (dr["pwd"].ToString() == "") {
            err_login("NON HAI IMPOSTATO LA PASSWORD!"); return;
          } else if (dlib.tools.cry.encode_tobase64(upass) != dr["pwd"].ToString()) {
            err_login("PASSWORD ERRATA!"); return;
          }

          // ok
          string uid = db_provider.str_val(dr["user_id"]);
          FormsAuthentication.RedirectFromLoginPage(uname + "|" + uid, true);
        }
      }
    } catch (Exception ex) { dlib.tools.log.log_err(ex); err_login(ex.Message); }
  }

  protected void Lost_Click (object sender, EventArgs e) {
    try {
      if (user_mail.Value == "") err_login("DEVI SCRIVERE CHI SEI PER REIMPOSTARE LA PASSWORD!");
      else {
        // check nomignolo
        DataRow dr = db_conn.first_row(@"select nome, email
        from users where activated = 1 and nome = '" + user_mail.Value + "';");
        if (dr == null) { err_login("NON C'È NESSUN UTENTE " + user_mail.Value + " ATTIVO!"); return; }

        // registrazione
        string tkey = dlib.tools.cry.rnd_str(32), akey = dlib.tools.cry.rnd_str(32);
        db_conn.exec(string.Format(@"update users set tmp_key = '{0}', activate_key = '{1}', dt_upd = getdate() 
          where nome = '{2}' and activated = 1", tkey, akey, user_mail.Value));

        send_mail(dr["email"].ToString(), "reimposta la tua password della deepa-notes",
          string.Format("<h3>Ciao {0}!</h3><p><a href='{1}reimposta.aspx?akey={2}'>Clicca qui per poter reimpostare la tua password!</a></p>"
          , dr["nome"], core.base_url, akey));

        Response.Redirect(string.Format("~/toreimposta.aspx?tkey={0}", tkey));
      }
    } catch (Exception ex) { dlib.tools.log.log_err(ex); err_login(ex.Message); }
  }

  protected void New_Click (object sender, EventArgs e) { Response.Redirect("~/new.aspx"); }

  protected void err_login (string txt) {
    lbl_alert.Visible = true; lbl_alert.InnerHtml = string.Format("<strong>Ops...c'è qualcosa che non va!</strong><br/><br/>{0}", txt);
  }
}