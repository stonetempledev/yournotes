using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;
using System.Data;
using dn_lib.tools;

public partial class confirm : tl_page {
  protected void Page_Load (object sender, EventArgs e) {
    try {
      DataRow dr = db_conn.first_row(@"select nome, email
          from users where isnull(activated, 0) = 3 and activate_key = '" + qry_val("akey") + "';");
      if (dr == null) { FormsAuthentication.SignOut(); Response.Redirect("login.aspx"); return; }

      db_conn.exec(string.Format(@"update users set activated = 1, dt_activate = getdate(), tmp_key = null, activate_key = null
      where activate_key = '{0}';", qry_val("akey")));

      lbl_user.Value = dr["nome"].ToString();
      txt_title.InnerText = string.Format("{0}, ora sei iscritto al deepa-notes!", dr["nome"]);
      txt_body.InnerText = string.Format("Per entrare digita l'utente '{0}' e la password giusta!", dr["nome"]);
      c_down.Visible = true;
    } catch (Exception ex) {
      log.log_err(ex);
      txt_title.InnerText = "Peccato...";
      txt_body.InnerHtml = "Ma non è stato possibile confermare la tua registrazione!<br><br>" + ex.Message;
    }
  }
}