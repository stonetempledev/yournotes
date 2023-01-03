using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;
using System.Data;
using dn_lib.tools;

public partial class toreimposta : tl_page {

  protected void Page_Load (object sender, EventArgs e) {
    try {
      DataRow dr = db_conn.first_row(@"select nome, email
          from users where isnull(activated, 0) = 1 and tmp_key = '" + qry_val("tkey") + "';");
      if (dr == null) { FormsAuthentication.SignOut(); Response.Redirect("login.aspx"); return; }

      db_conn.exec(string.Format(@"update users set tmp_key = null where tmp_key = '{0}';", qry_val("tkey")));

      txt_title.InnerText = string.Format("Ok {0}!", dr["nome"]);
      txt_body.InnerText = "Adesso apri la tua mail e reimposta la password!";
    } catch (Exception ex) {
      log.log_err(ex);
      txt_title.InnerText = "Peccato...";
      txt_body.InnerText = "Si è verificato un errore...<br><br>" + ex.Message;
    }

  }
}