using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;
using System.Data;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using deepanotes;
using dn_lib.tools;
using dn_lib.xml;

public partial class login : tl_page {

  protected users _u = null;

  protected void Page_Load(object sender, EventArgs e) {
    lbl_alert.Visible = lbl_ok.Visible = false;
    _u = new users();
  }

  protected void Go_Click(object sender, EventArgs e) {
    try {
      string tkey, akey;
      int uid = _u.add_utente(user_name.Value, user_mail.Value, user_pass.Value, user_pass2.Value, out tkey, out akey);
      this.user = new user(uid, user_name.Value, user_mail.Value, user.type_user.normal);
      set_cache_var("tmp_password", user_pass.Value);
      Response.Redirect(string.Format("iscritto.aspx?tkey={0}", tkey));
    } catch (Exception ex) { err_msg(ex.Message); }

  }

  protected void ok_msg(string txt) {
    lbl_ok.Visible = true; lbl_ok.InnerHtml = string.Format("<strong>{0}</strong>", txt);
  }

  protected void err_msg(string txt) {
    lbl_alert.Visible = true; lbl_alert.InnerHtml = string.Format("<strong>{0}</strong>", txt);
  }
}