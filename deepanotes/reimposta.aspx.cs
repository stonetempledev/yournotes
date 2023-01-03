using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;
using System.Data;
using dn_lib.tools;

public partial class reimposta : tl_page {
  protected string _nome, _mail;
  protected void Page_Load (object sender, EventArgs e) {
    try {
      lbl_alert.Visible = lbl_ok.Visible = false;
      DataRow dr = db_conn.first_row(@"select nome, email
          from users where activated = 1 and activate_key = '" + qry_val("akey") + "';");
      if (dr == null) { FormsAuthentication.SignOut(); Response.Redirect("login.aspx"); return; }

      _nome = dr["nome"].ToString(); _mail = dr["email"].ToString();

      txt_title.InnerText = string.Format("Reimposta la password {0}!", dr["nome"]);
    } catch (Exception ex) {
      log.log_err(ex);
    }
  }

  protected void Go_Click (object sender, EventArgs e) {
    try {
      if (user_pass.Value == "") err_msg("devi scrivere la password!");
      else if (user_pass.Value.Length < 3) err_msg("la password dev'essere almeno di 3 caratteri!");
      else if (user_pass2.Value == "") err_msg("devi confermare la password!");
      else if (user_pass.Value != user_pass2.Value) err_msg("la conferma della password è andata male!");
      else {

        // ri-registrazione
        string tkey = dlib.tools.cry.rnd_str(32);
        db_conn.exec(string.Format(@"update users set pwd = '{0}', dt_upd = getdate()
          where activated = 1 and nome = '{1}';"
          , dlib.tools.cry.encode_tobase64(user_pass.Value), _nome));

        Response.Redirect(string.Format("reiscritto.aspx?akey={0}", qry_val("akey")));
      }
    } catch (Exception ex) { dlib.tools.log.log_err(ex); err_msg(ex.Message); }
  }

  protected void ok_msg (string txt) {
    lbl_ok.Visible = true; lbl_ok.InnerHtml = string.Format("<strong>{0}</strong>", txt);
  }

  protected void err_msg (string txt) {
    lbl_alert.Visible = true; lbl_alert.InnerHtml = string.Format("<strong>{0}</strong>", txt);
  }
}