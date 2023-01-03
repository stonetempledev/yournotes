using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;
using System.Data;
using dn_lib.tools;

public partial class reiscritto : tl_page {

  protected void Page_Load (object sender, EventArgs e) {
    try {
      DataRow dr = db_conn.first_row(@"select nome, email, activate_key 
        from users where activated = 1 and activate_key = '" + qry_val("akey") + "';");
      if (dr == null) { FormsAuthentication.SignOut(); Response.Redirect("login.aspx"); return; }

      db_conn.exec(string.Format(@"update users set activate_key = null 
        where activated = 1 and activate_key = '{0}';", qry_val("akey")));

      txt_title.InnerText = string.Format("{0} hai aggiornato la password della deepa-notes!", dr["nome"]);
      txt_body.InnerHtml = "<a href='login.aspx?nm=" + dr["nome"].ToString() + "'>Puoi entrare con la tua nuova password...</a>";
    } catch (Exception ex) {
      log.log_err(ex);
      txt_title.InnerText = "Peccato...";
      txt_body.InnerText = "Ma si è verificato un errore...<br><br>" + ex.Message;
    }

  }
}