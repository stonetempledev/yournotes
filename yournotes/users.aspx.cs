using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml;
using System.Text;
using System.IO;
using System.Collections.Specialized;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using dn_lib.db;
using dn_lib.tools;
using dn_lib.xml;
using deepanotes;

public partial class _users : tl_page {

  protected cmd _cmd = null;

  protected override void OnInit(EventArgs e) {
    base.OnInit(e);

    // inizializzazione
    _cmd = master.check_cmd(qry_val("cmd"));

    // elab requests & cmds
    if (this.IsPostBack) return;

    try {

      users us = new users();

      if (json_request.there_request(this)) {
        json_result res = new json_result(json_result.type_result.ok);

        try {

          json_request jr = new json_request(this);

          // add_user
          if (jr.action == "add_user") {
            string user_name = jr.val_str("user_name"), email = jr.val_str("email")
              , password = jr.val_str("password"), c_password = jr.val_str("c_password");

            string tkey, akey;
            us.add_utente(user_name, email, password, c_password, out tkey, out akey, 3);

            send_mail(email, "conferma iscrizione al dipa notes",
              string.Format(@"<h2>{0}.</h2><p>Sei stato iscritto al <a href='{1}'>Deepa-Notes</a>!</p>
                <p><i>Ecco la password: {3}</i></p>
                <h3><a href='{1}confirm.aspx?akey={2}'>entra per confermare la tua iscrizione!</a></h3>"
              , user_name, core.base_url, akey, password));
          } // del_user
          else if (jr.action == "del_user") {
            us.del_utente(jr.val_int("id"));
          } // disable_user
          else if (jr.action == "disable_user") {
            us.disable_utente(jr.val_int("id"));
          } // active_user
          else if (jr.action == "active_user") {
            us.riactive_utente(jr.val_int("id"));
            user u = us.get_user(jr.val_int("id"));

            send_mail(u.email, "attivazione al deepa-notes",
              string.Format(@"<h2>{0}.</h2><pSei stato iscritto al <a href='{1}'>Deepa-Notes</a>!</p>
                <h3><a href='{1}login.aspx?nm={0}'>clicca qui per entrare, ma devi ricordare la password!</a></h3>"
              , u.name, core.base_url));
          }

        } catch (Exception ex) { log.log_err(ex); res = new json_result(json_result.type_result.error, ex.Message); }

        write_response(res);

        return;
      }

      // check cmd
      if (_cmd == null) return;

      // user
      StringBuilder sb = new StringBuilder();
      if (_cmd.action == "view" && _cmd.obj == "user") {
        cmd_add.Value = master.url_cmd("add user");
        page_title.InnerText = "Utente loggato";
        sb.Append(core.parse_html_block("logged-user", new Dictionary<string, object>() { { "us", user } }));
        view.InnerHtml = sb.ToString();

      }  // users
      else if (_cmd.action == "view" && _cmd.obj == "users") {
        cmd_add.Value = master.url_cmd("add user");
        page_title.InnerText = "Elenco utenti";
        foreach (user u in us.list_users())
          sb.Append(core.parse_html_block("user", new Dictionary<string, object>() { { "us", u } }));
        view.InnerHtml = sb.ToString();
      } // add user
      else if (_cmd.action == "add" && _cmd.obj == "user") {
        cmd_users.Value = master.url_cmd("view utenti");
        page_title.InnerText = "Nuovo utente";
        view.Visible = false;
        add.Visible = true;
      } else throw new Exception("COMANDO NON RICONOSCIUTO!");


    } catch (Exception ex) { log.log_err(ex); if (!json_request.there_request(this)) master.err_txt(ex.Message); }
  }

  protected override void OnLoad(EventArgs e) {
    base.OnLoad(e);

    //if (_cmd.action == "xml") this.master.set_status_txt("caricamento dati...");

  }

  protected override void OnLoadComplete(EventArgs e) {
    base.OnLoadComplete(e);
  }

}
