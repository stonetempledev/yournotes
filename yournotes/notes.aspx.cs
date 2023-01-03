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

public partial class _notes : tl_page
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

      notes ob = new notes();

      // upload file
      if(json_request.there_file(this)) {
        json_result res = new json_result(json_result.type_result.ok);

        try {

          if(Request.Files.Count > 0) {

            HttpFileCollection files = Request.Files;
            foreach(string key in files) {
              HttpPostedFile file = files[key];
              string tp_file = key.Split(new char[] { '_' })[0];
              if(tp_file == "task-file") {
                int task_id = int.Parse(key.Split(new char[] { '_' })[2]);
                byte[] content;
                using(var streamReader = new MemoryStream()) {
                  file.InputStream.CopyTo(streamReader);
                  content = streamReader.ToArray();
                }
                add_att(ob, task_id, Path.GetFileName(file.FileName), content);
              } else throw new Exception("tipo file upload '" + tp_file + "' non supportato!");
            }
          } else throw new Exception("nessun file da caricare!");

        } catch(Exception ex) { log.log_err(ex); res = new json_result(json_result.type_result.error, ex.Message); }

        write_response(res);

        return;
      }

      // js request
      if(json_request.there_request(this)) {
        json_result res = new json_result(json_result.type_result.ok);

        try {

          json_request jr = new json_request(this);

          // base
          if(base_elab_action(jr, res)) {
            // fatta
          }
          // add_att
          else if(jr.action == "add_att") {
            add_att(ob, jr.val_int("task_id"), jr.val_str("name"));
          }
          // task_state
          else if(jr.action == "task_state") {
            List<free_label> fl = ob.load_free_labels();

            string folder_path;
            ob.update_task(jr.val_int("id"), out folder_path, fl, stato: jr.val_str("stato"));
            List<task_stato> stati = db_conn.dt_table(core.parse_query("menu-states")).Rows.Cast<DataRow>()
              .Select(r => new task_stato(db_provider.str_val(r["stato"]), 0, "", ""
                , db_provider.str_val(r["title_singolare"]))).ToList();
            res.html_element = parse_task(ob.load_task(jr.val_int("id")), folder_path, stati);
          }
          // set_filter_id
          else if(jr.action == "set_filter_id") {
            set_cache_var("active-task-filter", jr.val_str("filter_id"));
          }
          // remove_task
          else if(jr.action == "remove_task") {
            ob.remove_task(jr.val_int("id"));
          }
          // update_task
          else if(jr.action == "update_task") {
            List<free_label> fl = ob.load_free_labels();

            if(!nome_valido(jr.val_str("title")))
              throw new Exception("nome '" + jr.val_str("title") + "' non valido!");

            string folder_path;
            ob.update_task(jr.val_int("id"), out folder_path, fl, title: jr.val_str("title"), assegna: jr.val_str("assegna")
              , priorita: jr.val_str("priorita"), stima: jr.val_str("stima"), tipo: jr.val_str("tipo"));
            List<task_stato> stati = db_conn.dt_table(core.parse_query("menu-states")).Rows.Cast<DataRow>()
              .Select(r => new task_stato(db_provider.str_val(r["stato"]), 0, "", ""
                , db_provider.str_val(r["title_singolare"]))).ToList();
            res.html_element = parse_task(ob.load_task(jr.val_int("id")), folder_path, stati);
          }
          // ren_task
          else if(jr.action == "ren_task") {
            if(!nome_valido(jr.val_str("title")))
              throw new Exception("nome '" + jr.val_str("title") + "' non valido!");
            ob.ren_task(jr.val_int("id"), jr.val_str("title"));
          }
          // add_task
          else if(jr.action == "add_task") {
            if(!nome_valido(jr.val_str("title")))
              throw new Exception("nome '" + jr.val_str("title") + "' non valido!");

            ob.add_task(jr.val_int("synch_folder_id"), jr.val_int("folder_id"), jr.val_int_null("search_id"), jr.val_str("stato")
              , jr.val_str("title"), jr.val_str("assegna"), jr.val_str("priorita"), jr.val_str("tipo"), jr.val_str("stima"));
          }
          // remove_att
          else if(jr.action == "remove_att") {
            ob.remove_att(jr.val_int("id"));
          }          
          // ren_att
          else if(jr.action == "ren_att") {
            ob.ren_file(jr.val_int("file_id"), jr.val_str("name"));
          }
          // add_folder
          else if(jr.action == "add_folder") {
            if(!nome_valido(jr.val_str("title")))
              throw new Exception("nome '" + jr.val_str("title") + "' non valido!");

            ob.add_folder(jr.val_int("synch_folder_id"), jr.val_int("folder_id"), jr.val_str("title"));
          }
          // ren_folder
          else if(jr.action == "ren_folder") {
            if(!nome_valido(jr.val_str("title")))
              throw new Exception("nome '" + jr.val_str("title") + "' non valido!");
            ob.ren_folder(jr.val_int("synch_folder_id"), jr.val_int("folder_id"), jr.val_str("title"));
          }
          // del_folder
          else if(jr.action == "del_folder") {
            ob.del_folder(jr.val_int("synch_folder_id"), jr.val_int("folder_id"));
            res.contents = master.url_cmd("tasks");
          }
          // cut_element
          else if(jr.action == "cut_element") {
            int f_id = jr.val_int("element_id"), sf_id = jr.val_int("synch_folder_id");
            string tp = jr.val_str("tp_element"); bool? added = null;
            if(tp == "folder" || tp == "synch-folder") {
              if(sf_id > 0 && tp == "synch-folder") {
                foreach(int id in ob.ids_childs_folders(sf_id)) {
                  added = set_element_cut(id, element_cut.element_cut_type.folder, jr.val_bool("copy")); res.list.Add(id.ToString());
                }
              } else { added = set_element_cut(f_id, element_cut.element_cut_type.folder, jr.val_bool("copy")); res.list.Add(f_id.ToString()); }
            } else if(tp == "task") added = set_element_cut(f_id, element_cut.element_cut_type.task, jr.val_bool("copy"));
            else if(tp == "att") added = set_element_cut(f_id, element_cut.element_cut_type.attachment, jr.val_bool("copy"));

            res.set_var("added", added.HasValue ? (added.Value ? "true" : "false") : "none");
          }
          // paste_elements
          else if(jr.action == "paste_elements") {
            string tp_paste = jr.val_str("tp");
            int f_id = jr.val_int("folder_id"), sf_id = jr.val_int("synch_folder_id");
            bool paste = false;
            if(elements_cut.Count > 0) {
              string err = ""; List<element_cut> ecs = new List<element_cut>();
              if(tp_paste == "task") {
                DataRow dr = ob.get_task_info(f_id);
                if(db_provider.int_val(dr["file_id"]) > 0) throw new Exception("questo task non può contenere un allegato!");
                string path_task = Path.Combine(db_provider.str_val(dr["synch_local_path"])
                  , db_provider.str_val(dr["folder_path"]).Length > 0 ? db_provider.str_val(dr["folder_path"]).Substring(1) : "");
                foreach(element_cut ec in elements_cut) {
                  try {
                    if(ec.tp == element_cut.element_cut_type.attachment) {
                      paste = true;
                      if(!ec.copy) ob.move_file(ec.id, db_provider.int_val(dr["folder_id"]), db_provider.int_val(dr["synch_folder_id"]), path_task);
                      else ob.copy_file(ec.id, db_provider.int_val(dr["folder_id"]), db_provider.int_val(dr["synch_folder_id"]), path_task);
                    }
                  } catch(Exception ex) {
                    ecs.Add(ec);
                    if(err == "") err = ex.Message;
                  }
                }
              } else {
                if(sf_id <= 0) sf_id = ob.get_synch_folder_id(f_id);
                foreach(element_cut ec in elements_cut) {
                  try {
                    if(ec.tp == element_cut.element_cut_type.folder) {
                      ob.move_folder(ec.id, sf_id, f_id > 0 ? f_id : (int?)null); paste = true;
                    } else if(ec.tp == element_cut.element_cut_type.task) {
                      ob.move_task(ec.id, sf_id, f_id > 0 ? f_id : (int?)null); paste = true;
                    }
                  } catch(Exception ex) {
                    ecs.Add(ec);
                    if(err == "") err = ex.Message;
                  }
                }
              }
              res.message = err;
              elements_cut.Clear();
              foreach(element_cut ec2 in ecs) elements_cut.Add(ec2);
            }

            if(!paste) throw new Exception("non è stato incollato nessun elemento!");
          }
          // get_notes
          else if(jr.action == "get_details") {
            res.contents = ob.get_task_notes(jr.val_int("task_id"));
            string html_allegati = "";
            foreach(DataRow dr in ob.get_task_allegati(jr.val_int("task_id"), jr.val_int_null("search_id")).Rows) {
              bool cut = there_element_cut(db_provider.int_val(dr["file_id"]), element_cut.element_cut_type.attachment)
                , found = db_provider.int_val(dr["found_file"]) > 0, file_task = db_provider.int_val(dr["file_task"]) > 0;
              string style = cut ? "badge-warning" : (found ? "badge-danger" : (file_task ? "badge-primary" : "badge-light"));
              html_allegati += master.client_key == "" ? core.parse_html_block("task-allegato", new string[,] { { "file-id", db_provider.str_val(dr["file_id"]) }
                  , { "http-path", db_provider.str_val(dr["http_path"]) }, { "file-name", db_provider.str_val(dr["file_name"]) }, { "style", style }, { "tp_att", style } })
                : core.parse_html_block("task-allegato-client", new string[,] {
                  { "file-id", db_provider.str_val(dr["file_id"]) }, { "file-name", db_provider.str_val(dr["file_name"]) }, { "style", style }, { "tp_att", style }
                  , { "user-id", ob.user_id.ToString() }, { "user-name", ob.user_name } });
            }
            res.html_element = html_allegati != "" ? core.parse_html_block("task-allegati", new string[,] { { "html-allegati", html_allegati } }) : "";
          }
          // save_task_notes
          else if(jr.action == "save_task_notes") {
            synch s = ob.get_synch(jr.val_int("user_id"), jr.val_str("user_name"));
            s.save_task_notes(jr.val_int("task_id"), jr.val_str("text"));
          } else if(jr.action == "synch_folders") {
            synch s = ob.get_synch(jr.val_int("user_id"), jr.val_str("user_name"));
            s.synch_event += s_synch_event;
            synch_results rf = s.reload_folders(force: true);
            res.data = rf;
            res.contents = _synch_events;
          }

        } catch(Exception ex) { log.log_err(ex); res = new json_result(json_result.type_result.error, ex.Message); }

        write_response(res);

        return;
      }

      // synch
      if(_cmd != null && _cmd.action == "synch") {
        content.InnerHtml = "<p>sincronizzazione cartelle...</p>";
        ClientScript.RegisterStartupScript(GetType(), "__action"
          , "var __action_page = '" + _cmd.action + "';\r\n"
            + "var __user_id = " + this.user.id + ";\r\n"
            + "var __user_name = '" + this.user.name + "';\r\n", true);
      }

      // tasks
      else if(_cmd != null && ((_cmd.action == "view" && _cmd.obj == "tasks")
        || (_cmd.action == "search" && _cmd.obj == "task"))) {

        int? fi = qry_val("idt") != "" ? qry_int("idt") : (qry_val("id") != "" ? qry_int("id") : (int?)null)
          , sfi = qry_val("sft") != "" ? qry_int("sft") : (qry_val("sf") != "" ? qry_int("sf") : (int?)null);
        string search = _cmd.action == "search" ? _cmd.sub_obj() : "";

        // ricerca testo
        int search_cc = 0;
        int? search_id = search != "" ? ob.search_task(search, this.Session.SessionID, out search_cc) : (int?)null;
        search_id_active.Value = search_id.HasValue ? search_id.Value.ToString() : "";

        // filtro attivo
        List<task_filter> tfs = db_conn.dt_table(core.parse_query("filters-tasks")).Rows
          .Cast<DataRow>().Select(r => new task_filter(db_provider.int_val(r["task_filter_id"])
            , db_provider.str_val(r["filter_title"]), db_provider.str_val(r["filter_notes"])
            , db_provider.str_val(r["filter_def"]), db_provider.str_val(r["filter_class"]))).ToList();
        task_filter tf = tfs.FirstOrDefault(x => x.id == int.Parse(get_cache_var("active-task-filter", "1")));
        if(tf == null || search_id.HasValue) tf = new task_filter(0, "elenco completo delle attività", "", "", "");

        ob.load_objects(fi, sfi, tf, search_id);

        menu.InnerHtml = parse_menu(ob.synch_folders, sfi.HasValue || fi.HasValue, qry_val("cmd"), search_id);
        folder_id.Value = qry_val("id");

        folder f = fi.HasValue ? ob.find_folder(fi.Value) : null;
        synch_folder sf = sfi.HasValue ? ob.find_synch_folder(sfi.Value) : null;
        string title = f != null ? f.folder_name : (sf != null ? sf.title : "");

        List<task_stato> stati = db_conn.dt_table(core.parse_query("menu-states")).Rows.Cast<DataRow>()
          .Select(r => new task_stato(db_provider.str_val(r["stato"]), 0, "", "", db_provider.str_val(r["title_singolare"]))).ToList();

        content.InnerHtml = "<div style='display:none'>virtuale</div>"
          + parse_tasks(ob, stati, tf, title, f != null ? ob.find_synch_folder(f.synch_folder_id).title + f.path : "", tfs, search: search_id.HasValue ? search : "");

        ClientScript.RegisterStartupScript(GetType(), "__task_states"
          , "var __action_page = '';\r\n"
            + "var __task_priorita = " + JsonConvert.SerializeObject(db_conn.dt_table(core.parse_query("task-priorita"))) + ";\r\n"
            + "var __task_stime = " + JsonConvert.SerializeObject(db_conn.dt_table(core.parse_query("task-stime"))) + ";\r\n"
            + "var __task_tipi = " + JsonConvert.SerializeObject(db_conn.dt_table(core.parse_query("task-tipi"))) + ";\r\n"
            + "var __task_assegna = " + JsonConvert.SerializeObject(db_conn.dt_table(core.parse_query("task-assegna"))) + ";\r\n"
            + "var __user_id = " + this.user.id + ";\r\n"
            + "var __user_name = '" + this.user.name + "';\r\n", true);

      } else throw new Exception("COMANDO NON RICONOSCIUTO!");

    } catch(Exception ex) { log.log_err(ex); if(!json_request.there_request(this)) master.err_txt(ex.Message); }
  }

  protected string _synch_events = "";
  private void s_synch_event(object sender, synch_event_args e)
  {
    _synch_events += e.message + "<br/>";
  }

  protected void add_att(notes ob, int task_id, string file_name, byte[] content = null)
  {
    synch s = ob.get_synch();

    // salvo il file
    DataRow dr = ob.get_task_info(task_id);
    if(db_provider.int_val(dr["file_id"]) > 0) throw new Exception("il task non può contenere allegati!");

    string folder = Path.Combine(db_provider.str_val(dr["synch_local_path"]), db_provider.str_val(dr["folder_path"]).Substring(1))
      , file_path = Path.Combine(folder, file_name);
    if(content != null) File.WriteAllBytes(file_path, content); else File.WriteAllText(file_path, "");

    // aggiorno il db
    string tp; int cc; 
    long nid = s.set_file_db(db_provider.int_val(dr["synch_folder_id"]), db_provider.int_val(dr["folder_id"]), file_name, Path.GetExtension(file_name)
      , DateTime.Now, DateTime.Now, out tp, out cc);
    if(s.is_type_file(Path.GetExtension(file_name)) != null)
      s.set_file_content_db((int)nid, Path.GetExtension(file_name).ToLower()
        , content != null ? System.Text.Encoding.UTF8.GetString(content) : "", DateTime.Now, DateTime.Now);
    if(s.is_info_file(file_name) != null && db_provider.int_val(dr["file_notes_id"]) <= 0)
      s.init_task_notes_db(task_id, (int)nid, content != null ? System.Text.Encoding.UTF8.GetString(content) : "");
  }

  protected bool nome_valido(string nome) { return (new Regex("^[a-zA-Z0-9 ,ìèéùàò_]*$")).IsMatch(nome); }

  protected string parse_tasks(notes n, List<task_stato> stati, task_filter tf = null, string title_folder = "", string path_folder = ""
    , List<task_filter> tfs = null, string search = "")
  {

    StringBuilder sb = new StringBuilder();

    sb.Append(core.parse_html_block(search != "" ? (title_folder == "" ? "title-attivita-search" : "title-attivita-folder-search")
        : (title_folder == "" ? "title-attivita" : "title-attivita-folder")
      , new string[,] { { "title-folder", title_folder }, { "path-folder", path_folder }
      , { "filter-title", search != "" ? "ricerca attività" : (tf != null ? tf.title : "") }
      , { "filter-des", (search != "" ? "ricerca attività che contengono '" + search + "'" : "") + (tf != null ? tf.notes + " ordinate per data decrescente" : "") }
      , { "conteggio", n.tasks != null && n.tasks.Count > 0 ? "per un totale di: " + n.tasks.Count.ToString() + " attività"
          : "non è stata trovata nessuna attività" }
      , { "html-filters", tfs != null ? string.Join("", tfs.Select(x =>
        string.Format("<a class='dropdown-item {2}' href='javascript:change_filter({1})'>{0}</a>", x.title, x.id, x.class_css != "" ? "text-" + x.class_css : ""))) : "" }}));

    List<int> orders = n.tasks.Select(x => x.stato.order).Distinct().ToList();
    orders.Sort();
    foreach(int order in orders) {
      bool first = true;
      List<task> sub_tasks = n.tasks.Where(x => x.stato.order == order).OrderByDescending(xx => xx.dt_ref).ToList();
      foreach(task t in sub_tasks) {

        // title
        if(first) {
          string sub_title = sub_tasks.Count == 1 ? t.stato.title_singolare : t.stato.title_plurale;
          sb.Append(core.parse_html_block("open-title-sub-attivita", new string[,] {
            { "count", sub_tasks.Count.ToString() }, { "cls", t.stato.cls }
            , { "title", sub_title != "" ? sub_title : (sub_tasks.Count == 1 ? "GENERICA" : "GENERICHE") } }));
          first = false;
        }

        // task
        string folder_path = "";
        if(t.file_id.HasValue) {
          folder f = t.folder_id.HasValue ? n.synch_folders.FirstOrDefault(x => x.id == t.synch_folder_id).get_folder(t.folder_id.Value) : null;
          folder_path = f != null ? f.path : "";
        } else {
          synch_folder sf = n.synch_folders.FirstOrDefault(x => x.id == t.synch_folder_id);
          folder f = sf.get_folder(t.folder_id.Value)
            , fp = f.parent_id.HasValue ? sf.get_folder(f.parent_id.Value) : null;
          folder_path = fp != null ? fp.path : "";
        }
        sb.Append(parse_task(t, n.find_synch_folder(t.synch_folder_id).title + folder_path, stati));
      }
      if(!first) sb.Append(core.parse_html_block("close-title-sub-attivita"));
    }

    return sb.ToString();
  }

  protected string des_date(DateTime dt)
  {
    int gg = (int)(DateTime.Now - dt).TotalDays;
    if(gg == 0) return "oggi";
    else if(gg == 1) return "ieri";
    else if(gg == 2) return "ieri l'altro";
    else if(gg >= 3 && gg <= 7) return gg.ToString() + " giorni fa";
    else if(gg > 7 && gg <= 14) return "una settimana fa";
    else if(gg > 14 && gg <= 21) return "due settimane fa";
    else if(gg > 21 && gg <= 28) return "tre settimane fa";
    else if(gg > 28 && gg <= 35) return "un mese fa";
    else if(gg > 35 && gg <= 60) return "due mesi fa";
    else if(gg > 60 && gg <= 90) return "tre mesi fa";
    else if(dt.Year == DateTime.Now.Year) return dt.ToString("dddd dd MMMM");
    else return dt.ToString("dddd dd MMMM yyyy");
  }

  protected string parse_task(task t, string folder_path, List<task_stato> stati)
  {
    if(t == null) return "";

    try {

      // dates
      string dins = t.dt_ins.HasValue ? des_date(t.dt_ins.Value) : ""
        , dupd = t.dt_upd.HasValue ? des_date(t.dt_upd.Value) : "";
      if(dupd != "") {
        if(dins == dupd ||
          (t.dt_ins.HasValue && t.dt_upd.HasValue && t.dt_ins.Value > t.dt_upd.Value)) dupd = "";
      }

      // classes
      bool cut = there_element_cut((int)t.id, element_cut.element_cut_type.task);

      // stati menu
      string ms = string.Join("", stati.Where(x => x.stato != t.stato.stato).Select(s =>
        core.parse_html_block("menu-state", new string[,] { { "task_id", t.id.ToString() }, { "assign_stato", s.stato }, { "title", s.title_singolare } })
        ));

      return core.parse_html_block("task", new string[,] { { "task_id", t.id.ToString() }, { "path", folder_path }
        , { "cls", t.stato.cls }, { "title", t.title }, { "val-assegna", t.user != null ? t.user : "" }
        , { "classes", cut ? "task-cut" : "" }, { "can-paste", t.file_id.HasValue ? "false" : "true" }
        , { "val-priorita", t.priorita != null ? t.priorita.priorita : "" }, { "val-stima", t.stima != null ? t.stima.stima : "" }
        , { "val-tipo", t.tipo != null ? t.tipo.tipo : "" }, { "menu-states", ms }
        , { "stato", !string.IsNullOrEmpty(t.stato.title_singolare) ? t.stato.title_singolare : "generica" }
        , { "assegnata", !string.IsNullOrEmpty(t.user) ? "assegnata a " + t.user : "&nbsp;" }
        , { "title_notes", (t.has_notes ? "vedi note" + (t.has_files ? " e allegati" : ""): "aggiungi note" + (t.has_files ? " o vedi allegati" : "")) + "..." }
        , { "data", (dins != "" ? "creata " + dins : "") + (dupd != "" ? (dins != "" ? " e " : "") + "aggiornata " + dupd : "") }
        , { "priorita", t.priorita != null && !string.IsNullOrEmpty(t.priorita.priorita) ?
            parse_task_state(t.priorita.title_singolare, t.priorita.cls, "priorità di realizzazione") : "" }
        , { "stima", t.stima != null && !string.IsNullOrEmpty(t.stima.stima) ?
            parse_task_state(t.stima.title_singolare, t.stima.cls, "stima approssimativa dei tempi di realizzazione") : "" }
        , { "tipo", t.tipo != null && !string.IsNullOrEmpty(t.tipo.tipo) ?
            parse_task_state(t.tipo.title_singolare, t.tipo.cls, "tipo di attività") : "" } });
    } catch(Exception ex) {
      return core.parse_html_block("task-error", new string[,] { { "task_id", t.id.ToString() }, { "error", ex.Message }
        , { "source", ex.Source }, { "stack", ex.StackTrace } });
    }
  }

  protected string parse_task_state(string txt, string cls, string tooltip)
  {
    return core.parse_html_block("task-state", new string[,] { { "txt", txt }, { "cls", cls }, { "tooltip", tooltip } });
  }

  protected string parse_menu(List<synch_folder> sfs, bool open_home, string cmd, int? search_id)
  {
    StringBuilder sb = new StringBuilder();
    foreach(synch_folder sf in sfs) {

      // tasks
      List<task> l = sf.tasks; int order = l.Count > 0 ? l.Min(x => x.stato.order) : -1
        , cc = order >= 0 ? l.Count(x => x.stato.order == order) : 0;
      string t_plurale = cc > 0 ? l.First(x => x.stato.order == order).stato.title_plurale : ""
        , t_singolare = cc > 0 ? l.First(x => x.stato.order == order).stato.title_singolare : ""
        , color = cc > 0 ? l.First(x => x.stato.order == order).stato.cls : "";

      t_plurale = t_plurale == "" ? "generiche" : t_plurale;
      t_singolare = t_singolare == "" ? "generica" : t_singolare;

      sb.Append(core.parse_html_block(block_level(0)
        , new string[,] { { "id", sf.id.ToString() }, { "tp", "synch-folder" }
          , { "url_open_home", open_home ? master.url_cmd(cmd) : "" }
          , { "search", search_id.HasValue ? "true" : "false" }
          , { "url_synch_folder", master.url_cmd(cmd, pars: new string[,] { {"sf", sf.id.ToString() } }) }
          , { "title", sf.title }, { "childs", parse_folders_menu(sf.folders, 1, cmd, search_id) }
          , { "block-attivita", cc > 0 ? core.parse_html_block("spin-attivita-synch"
            , new string[,] { { "class_spin", color }, { "synch_folder_id", sf.id.ToString() }
              , { "url_open_tasks", master.url_cmd(cmd, pars: new string[,] { { "sft", sf.id.ToString() } }) }
              , { "title", cc == 1 ? "una attività " + t_singolare : cc.ToString() + " attività " + t_plurale }
              , { "c_attivita", cc.ToString() }} ) : "" }}));
    }
    return string.Format("<ul class='nav flex-column'>{0}</ul>", sb.ToString());
  }

  protected string parse_folders_menu(List<folder> fs, int lvl, string cmd, int? search_id)
  {
    StringBuilder sbb = new StringBuilder();

    foreach(folder f in fs.Where(x => !x.is_task)) {
      string url_open_folder = lvl >= 3 && f.folders.Where(x => x.task == null).Count() > 0
        ? master.url_cmd(cmd, pars: new string[,] { { "id", f.parent_id.ToString() } }) : "";

      bool cut = there_element_cut(f.folder_id, element_cut.element_cut_type.folder);

      // tasks
      List<task> l = url_open_folder == "" ? f.tasks : f.sub_tasks;
      int order = l.Count > 0 ? l.Min(x => x.stato.order) : -1
        , cc = order >= 0 ? l.Count(x => x.stato.order == order) : 0;
      string t_plurale = cc > 0 ? l.First(x => x.stato.order == order).stato.title_plurale : ""
        , t_singolare = cc > 0 ? l.First(x => x.stato.order == order).stato.title_singolare : ""
        , color = cc > 0 ? l.First(x => x.stato.order == order).stato.cls : "";

      t_plurale = t_plurale == "" ? "generiche" : t_plurale;
      t_singolare = t_singolare == "" ? "generica" : t_singolare;

      sbb.Append(core.parse_html_block(block_level(lvl), new string[,] { { "id", f.folder_id.ToString() }, { "tp", "folder" }
        , { "class_cut", cut ? "voce-cut" : "" }, { "title", f.folder_name }, { "childs", parse_folders_menu(f.folders, lvl + 1, cmd, search_id) }, { "url_open_folder", url_open_folder }
        , { "url_folder", master.url_cmd(cmd, pars: new string[,] { { "id", f.folder_id.ToString() } }) }
        , { "search", search_id.HasValue ? "true" : "false" }
        , { "block-attivita", cc > 0 ? core.parse_html_block("spin-attivita"
          , new string[,] { { "class_spin", color }, { "folder_id", f.folder_id.ToString() }
            , { "url_open_tasks", master.url_cmd(cmd, pars: new string[,] { { "idt", f.folder_id.ToString() } }) }
            , { "title", cc == 1 ? "una attività " + t_singolare :  cc.ToString() + " attività " + t_plurale }
            , { "c_attivita", cc.ToString() }} ) : "" }}));
    }
    return sbb.Length > 0 ? string.Format("<ul>{0}</ul>", sbb.ToString()) : "";
  }

  protected string block_level(int lvl)
  {
    return "item-" + (lvl == 0 ? "synch-folder" : (lvl == 1 ? "secondo" : (lvl == 2 ? "terzo" : "quarto")));
  }

  protected override void OnLoad(EventArgs e)
  {
    base.OnLoad(e);

    //if (_cmd.action == "xml") this.master.set_status_txt("caricamento dati...");

  }

  protected override void OnLoadComplete(EventArgs e)
  {
    base.OnLoadComplete(e);
  }

}
