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
using System.Text.RegularExpressions;
using System.Text;
using System.IO;
using System.Collections.Specialized;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using dn_lib.db;
using dn_lib.tools;
using dn_lib.xml;
using deepanotes;
using dn_lib;

public partial class _io : tl_page
{

  protected cmd _cmd = null;

  protected override void OnInit(EventArgs e)
  {
    base.OnInit(e);

    // inizializzazione
    _cmd = master.check_cmd(qry_val("cmd"));

    // elab requests & cmds
    if(this.IsPostBack) return;

    try {

      io ob = new io();

      // js request
      if(json_request.there_request(this)) {
        json_result res = new json_result(json_result.type_result.ok);

        try {

          json_request jr = new json_request(this);

          // save_file
          if(jr.action == "save_file") {
            synch s = ob.get_synch(jr.val_int("user_id"), jr.val_str("user_name"));
            Encoding enc = System.Text.Encoding.GetEncoding(jr.val_str("enc"));
            byte[] bytes = jr.val_bytes("bin_data", enc);
            string path = ob.file_path(jr.val_int("id"));
            File.WriteAllBytes(path, bytes);
            s.set_file_content_db(jr.val_int("id"), Path.GetExtension(path).ToLower(), enc.GetString(bytes), DateTime.Now, DateTime.Now);
          } // synch folders
          else if(jr.action == "synch_folders") {
            synch s = ob.get_synch(jr.val_int("user_id"), jr.val_str("user_name"));
            s.synch_event += s_synch_event;
            synch_results rf = s.reload_folders();
            res.data = rf;
            res.contents = _synch_events;
          }

        } catch(Exception ex) { log.log_err(ex); res = new json_result(json_result.type_result.error, ex.Message); }

        write_response(res);

        return;
      }

      // tasks
      //if(_cmd != null && _cmd.action == "view" && _cmd.obj == "tasks") {
      //  int? fi = qry_val("idt") != "" ? qry_int("idt") : (qry_val("id") != "" ? qry_int("id") : (int?)null)
      //    , sfi = qry_val("sft") != "" ? qry_int("sft") : (qry_val("sf") != "" ? qry_int("sf") : (int?)null);
      //} else throw new Exception("COMANDO NON RICONOSCIUTO!");

    } catch(Exception ex) { log.log_err(ex); if(!json_request.there_request(this)) master.err_txt(ex.Message); }
  }

  protected string _synch_events = "";
  private void s_synch_event(object sender, synch_event_args e)
  {
    _synch_events += e.message + "\n";
  }

  protected override void OnLoad(EventArgs e)
  {
    base.OnLoad(e);
  }

  protected override void OnLoadComplete(EventArgs e)
  {
    base.OnLoadComplete(e);
  }

}
