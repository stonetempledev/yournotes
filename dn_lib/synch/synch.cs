using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using dn_lib;
using dn_lib.db;
using dn_lib.tools;

namespace dn_lib
{
  public class synch : bo
  {
    public List<synch_folder> synch_folders { get; set; }

    public synch(db_provider conn, core c, config cfg, int user_id = 0, string user_name = "")
      : base(conn, c, cfg, user_id, user_name)
    {
    }

    #region data access

    public List<synch_folder> list_synch_folders(string pc_name)
    {
      return db_conn.dt_table(core.parse_query("lib-notes.synch-folders", new string[,] { { "pc_name", pc_name } })).Rows.Cast<DataRow>()
        .Select(x => new synch_folder(db_provider.int_val(x["synch_folder_id"]), db_provider.str_val(x["pc_name"])
          , db_provider.str_val(x["title"]), db_provider.str_val(x["des"])
          , db_provider.str_val(x["local_path"]), db_provider.str_val(x["http_path"])
          , db_provider.str_val(x["user"]), db_provider.str_val(x["password"]))).ToList();
    }

    public void clean_readed() { db_conn.exec(core.parse_query("lib-notes.clean-readed")); }

    public void del_unreaded(out int cc_files, out int cc_folders)
    {
      DataRow r = db_conn.first_row(core.parse_query("lib-notes.del-unreaded"));
      cc_files = Convert.ToInt32(r[0]); cc_folders = Convert.ToInt32(r[1]);
    }

    public void update_folder_name_db(string folder_name, long folder_id)
    {
      db_conn.exec(core.parse_query("lib-notes.update-folder-name"
        , new Dictionary<string, object>() { { "folder_id", folder_id.ToString() }, { "folder_name", folder_name } }));
    }

    public long set_folder_db(int synch_folder_id, long? parent_id, string folder_name, DateTime dt_ins, DateTime dt_lwt, out string tp, out int cc)
    {
      tp = ""; cc = 0;
      string res = db_conn.exec(core.parse_query("lib-notes.ins-folder"
        , new Dictionary<string, object>() { { "synch_folder_id", synch_folder_id.ToString() }
          , { "dt_ins", dt_ins }, { "dt_lwt", dt_lwt }
          , { "parent_id", !parent_id.HasValue ? "null" : parent_id.Value.ToString() }
          , { "cmp_p", !parent_id.HasValue ? "is" : "=" }, { "folder_name", folder_name } }), true, true);
      tp = res.Split(new char[] { ';' })[1];
      cc = int.Parse(res.Split(new char[] { ';' })[2]);
      return res.Split(new char[] { ';' })[0] != "" ? long.Parse(res.Split(new char[] { ';' })[0]) : -1;
    }

    public long set_file_db(int synch_folder_id, long? folder_id, string file_name, string extension, DateTime ct, DateTime lwt, out string tp, out int cc)
    {
      tp = ""; cc = 0;
      string res = db_conn.exec(core.parse_query("lib-notes.ins-file"
        , new Dictionary<string, object>() { { "synch_folder_id", synch_folder_id.ToString() }, { "ct", ct }, { "lwt", lwt }
          , { "folder_id", !folder_id.HasValue ? "null" : folder_id.ToString() }, { "cmp_f", !folder_id.HasValue ? "is" : "=" }
          , { "file_name", file_name }, { "extension", !string.IsNullOrEmpty(extension) ? extension.ToLower() : "" } }), true, true);
      string[] ress = res.Split(new char[] { ';' });
      tp = ress[1]; cc = int.Parse(ress[2]);
      return ress[0] != "" ? long.Parse(ress[0]) : -1;
    }

    public long set_task_db(task t, out string tp, out int cc, DateTime? lwt_i = null)
    {
      tp = ""; cc = 0;
      string res = db_conn.exec(core.parse_query("lib-notes.ins-task", new Dictionary<string, object>() { { "task", t }
        , { "folder_id", t.folder_id.HasValue ? t.folder_id.Value : 0 }, { "file_id", t.file_id.HasValue ? t.file_id.Value : 0 }
        , { "stato", t.stato }, { "priorita", t.priorita }, { "tipo", t.tipo }, { "stima", t.stima }
        , { "dt_lwt_index", lwt_i.HasValue ? lwt_i.Value.ToString("yyyy/MM/dd HH:mm:ss") : "" } }), true, true);

      string[] ress = res.Split(new char[] { ';' });
      tp = ress[1]; cc = int.Parse(ress[2]);
      return ress[0] != "" ? long.Parse(ress[0]) : -1;
    }

    public synch_machine get_synch_machine(string pc_name)
    {
      DataRow r = db_conn.first_row(core.parse_query("lib-synch.start-synch-machine", new string[,] { { "pc_name", pc_name } }));
      return r != null ? new synch_machine(db_provider.int_val(r["synch_machine_id"]), db_provider.str_val(r["pc_name"])
        , db_provider.str_val(r["pc_des"]), db_provider.int_val(r["seconds"]), db_provider.int_val(r["active"]) == 1
        , db_provider.str_val(r["state"])) : null;
    }

    public void start_machine(int id, string ip_address)
    {
      db_conn.exec(core.parse_query("lib-synch.start-machine", new string[,] { { "id", id.ToString() }
        , { "ip_address", ip_address } }));
    }

    public void stop_machine(int id)
    {
      db_conn.exec(core.parse_query("lib-synch.stop-machine", new string[,] { { "id", id.ToString() } }));
    }

    public void last_synch_machine(int id, int folders, int files, int deleted, int seconds)
    {
      db_conn.exec(core.parse_query("lib-synch.last-synch-machine", new string[,] { { "id", id.ToString() }
        , { "folders", folders.ToString() }, { "files", files.ToString() }
        , { "deleted", deleted.ToString() }, { "seconds", seconds.ToString() } }));
    }

    public List<synch_machine> list_synch_machine()
    {
      return db_conn.dt_table(core.parse_query("lib-synch.synch-machines")).Rows.Cast<DataRow>()
        .Select(r => new synch_machine(db_provider.int_val(r["synch_machine_id"]), db_provider.str_val(r["pc_name"])
         , db_provider.str_val(r["pc_des"]), db_provider.int_val(r["seconds"]), db_provider.int_val(r["active"]) == 1
         , db_provider.str_val(r["state"]), db_provider.str_val(r["ip_address"])
         , db_provider.dt_val(r["dt_start"]), db_provider.dt_val(r["dt_stop"]), db_provider.dt_val(r["dt_lastsynch"])
         , db_provider.int_val(r["c_folders"]), db_provider.int_val(r["c_files"]), db_provider.int_val(r["c_deleted"])
         , db_provider.int_val(r["s_synch"]))).ToList();
    }

    public List<free_label> load_free_labels()
    {
      return db_conn.dt_table(core.parse_query("lib-notes.free-labels")).Rows.Cast<DataRow>()
        .Select(r => new free_label(db_provider.str_val(r["free_txt"]), db_provider.str_val(r["stato"])
          , db_provider.str_val(r["priorita"]), db_provider.str_val(r["tipo"]), db_provider.str_val(r["stima"])
          , db_provider.str_val(r["default"]) == "1")).ToList();
    }

    public List<file_info> load_file_info()
    {
      return db_conn.dt_table(core.parse_query("lib-notes.file-infos")).Rows.Cast<DataRow>()
        .Select(r => new file_info(db_provider.str_val(r["file_name"]), (file_info.fi_type)Enum.Parse(typeof(file_info.fi_type), db_provider.str_val(r["type_info"])))).ToList();
    }

    public List<file_type> load_file_types()
    {
      return db_conn.dt_table(core.parse_query("lib-notes.file-types")).Rows.Cast<DataRow>()
        .Select(r => new file_type(db_provider.str_val(r["extension"]), db_provider.str_val(r["des_extension"]), db_provider.str_val(r["open_comment"]), db_provider.str_val(r["type_content"])
          , (file_type.ft_type_content)Enum.Parse(typeof(file_type.ft_type_content), db_provider.str_val(r["type_content"])))).ToList();
    }

    #endregion

    #region synch

    public event EventHandler<synch_event_args> synch_event;
    protected void fire_synch_event(string txt, bool init = false)
    {
      log.log_debug(txt);
      EventHandler<synch_event_args> handler = synch_event;
      if(handler != null) {
        handler(this, new synch_event_args() { message = txt, init = init });
      }
    }

    protected bool _loaded_settings = false;
    protected List<string> _users = null;
    protected List<free_label> _labels = null;
    protected List<file_info> _f_infos = null;
    protected List<file_type> _f_types = null;
    protected void reload_settings()
    {
      if(_loaded_settings) return;
      _loaded_settings = true;
      _users = db_conn.dt_table(core.parse_query("lib-notes.task-users")).Rows.Cast<DataRow>()
        .Select(r => db_provider.str_val(r["nome"])).ToList();
      _labels = load_free_labels();
      _f_infos = load_file_info();
      _f_types = load_file_types();
    }

    public file_info is_info_file(string file_name) { reload_settings(); return _f_infos.FirstOrDefault(x => x.file_name.ToLower() == file_name.ToLower()); }

    public file_type is_type_file(string extension) { reload_settings(); return _f_types.FirstOrDefault(x => x.extension.ToLower() == extension.ToLower()); }

    public synch_results reload_folders(bool check = false, bool force = false)
    {
      reload_settings();

      synch_results res = new synch_results();
      try {

        // check cache
        synch_results rc = null;
        if(!check) {
          res.scan = true;

          // seconds
          if(!force) {
            string prev_last = get_cache_var("synch-last", -1);
            DateTime last = prev_last != "" ? DateTime.Parse(prev_last) : DateTime.MinValue;
            setting s = settings.get_setting(core, db_conn, "synch-seconds");
            if(s != null && last != DateTime.MinValue && (DateTime.Now - last).TotalSeconds < int.Parse(s.value)) {
              res.scan = false;
              return res;
            }
          }

          // files, folders, lwt          
          int prev_files = int.Parse(get_cache_var("synch-files", -1, "-1"))
          , prev_folders = int.Parse(get_cache_var("synch-folders", -1, "-1"));
          string prev_lwt = get_cache_var("synch-lwt", -1);
          DateTime lwt = prev_lwt != "" ? DateTime.Parse(prev_lwt) : DateTime.MinValue;
          rc = reload_folders(true);
          if(!force && ((prev_files > 0 && prev_files == rc.files)
            && (prev_folders > 0 && prev_folders == rc.folders)
            && (lwt != DateTime.MinValue && lwt == rc.lwt))) {
            set_cache_var("synch-last", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), -1);
            res.scan = false;
            return res;
          }
        }

        DateTime start = DateTime.Now;
        if(!check) fire_synch_event("elenco cartelle da sincronizzare", true);

        // folders to synch
        this.synch_folders = list_synch_folders(Environment.MachineName);
        if(!check) {
          foreach(synch_folder f in this.synch_folders)
            fire_synch_event(string.Format("   - cartella di sincronizzazione '{0}' - {1}, path: {2}"
              , f.title, f.des, f.local_path), true);
        }

        // leggo le cartelle
        if(!check) clean_readed();
        foreach(synch_folder f in this.synch_folders) {
          if(!check) fire_synch_event($"elaboro la cartella {f.local_path}...", true);
          res = init_synch_folder(f.id, f.local_path, res: res, check: check);
        }

        if(!check) {
          del_unreaded(out int cc_files, out int cc_folders);
          res.deleted = cc_files + cc_folders;
          if(res.deleted > 0) {
            if(cc_files > 0) fire_synch_event($"cancellati dal database: {cc_files} files");
            if(cc_folders > 0) fire_synch_event($"cancellati dal database: {cc_folders} folders");
          }
        }

        res.seconds = (int)(DateTime.Now - start).TotalSeconds;

        // check cache
        if(!check) {
          set_cache_var("synch-files", rc.files.ToString(), -1);
          set_cache_var("synch-folders", rc.folders.ToString(), -1);
          set_cache_var("synch-lwt", rc.lwt.ToString("yyyy/MM/dd HH:mm:ss"), -1);
          set_cache_var("synch-last", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), -1);
        }

      } catch(Exception ex) { res.err = ex.Message; log.log_err(ex.Message); } finally { }

      return res;
    }

    public void set_file_content_db(int file_id, string extension, string content, DateTime ct, DateTime lwt)
    {
      db_conn.exec(core.parse_query("lib-notes.set-content", new string[,] { { "file_id", file_id.ToString() }, { "extension", extension.ToLower() }
        , { "ct", ct.ToString("yyyy-MM-dd HH:mm:ss") }, { "lwt", lwt.ToString("yyyy-MM-dd HH:mm:ss") } })
        , pars: new System.Data.Common.DbParameter[] { new SqlParameter("@content", System.Data.SqlDbType.VarChar) { Value = content } });
    }

    protected synch_results init_synch_folder(int synch_folder_id, string path
      , long? parent_id = null, task parent_task = null, synch_results res = null, bool check = false)
    {
      if(res == null) res = new synch_results();
      try {

        // folders
        foreach(string fp in Directory.EnumerateDirectories(path)) {
          DirectoryInfo di = new DirectoryInfo(fp);
          DateTime ct = sys.without_ms(di.CreationTime), lwt = sys.without_ms(di.LastWriteTime);          
          string folder_name = di.Name;

          long folder_id = 0; task t = null;
          if(!check) {
            // folder
            string tp; int cc = 0;
            folder_id = set_folder_db(synch_folder_id, parent_id, folder_name, ct, lwt, out tp, out cc);
            if(tp == "insert") fire_synch_event("aggiunto folder al database: " + fp);
            else if(tp == "update" && cc > 0) fire_synch_event("aggiornato folder nel database: " + fp);

            // task folder 
            if(parent_task == null) {
              t = elab_task_folder(res, synch_folder_id, fp, ct, lwt, folder_id);
              if(t != null) { di = new DirectoryInfo(t.path); folder_name = di.Name; }
            }
          }

          res.folders++;
          if(parent_task != null) parent_task.level_folder++;
          res = init_synch_folder(synch_folder_id, Path.Combine(path, folder_name), folder_id, t != null ? t : parent_task, res, check);
        }

        // files
        string i_task = core.config.get_var("lib-vars.index-folder").value;
        foreach(string fn in Directory.EnumerateFiles(path)) {
          FileInfo fi = new FileInfo(fn);
          DateTime ct = sys.without_ms(fi.CreationTime), lwt = sys.without_ms(fi.LastWriteTime);
          if(lwt > res.lwt) res.lwt = lwt;

          // web.config
          if(fi.Name.ToLower() == "web.config" && !parent_id.HasValue) continue;

          // __i.xml
          if(parent_task != null && fi.Name == i_task) continue;

          if(!check) {

            // file          
            long file_id = set_file_db(synch_folder_id, parent_id, fi.Name, fi.Extension, ct, lwt, out string tp, out int cc);
            if(tp == "insert") fire_synch_event("aggiunto file al database: " + fn);
            else if(tp == "update" && cc > 0) fire_synch_event("aggiornato file nel database: " + fn);

            // file content
            string new_content = "";
            file_info info = is_info_file(fi.Name);
            file_type ftp = is_type_file(fi.Extension);
            if(info != null || ftp != null) {
              if(tp == "insert" || (tp == "update" && cc > 0)) {
                new_content = File.ReadAllText(fn);
                if(new_content.Replace(" ", "").Replace("\r", "").Replace("\n", "") != "") {
                  set_file_content_db((int)file_id, Path.GetExtension(fn).ToLower(), new_content, ct, lwt);
                  fire_synch_event("salvato contenuto file nel database: " + fn);

                  if(parent_task != null && info != null) {
                    if(set_task_notes_db(parent_task.id, file_id, new_content, file_type.ft_type_content.info, ct, lwt))
                      fire_synch_event($"salvate le note del task nel database: {fn}");
                  }
                }
              }
            }

            // task file
            if(parent_task == null) {
              task t = task.parse_task(core, synch_folder_id, fn, ct, lwt, _users, _labels, file_id: file_id);
              if(t != null) {
                long task_id = set_task_db(t, out tp, out cc);
                if(tp == "insert") fire_synch_event("inserito task nel database: " + Path.Combine(path, t.title));
                else if(tp == "update" && cc > 0) fire_synch_event("aggiornato il task nel database: " + Path.Combine(path, t.title));

                // task notes
                string notes = "";
                if(ftp != null && (tp == "insert" || (tp == "update" && cc > 0))) {
                  if(set_task_notes_db(task_id, file_id, new_content, ftp.type_content, ct, lwt, out notes))
                    fire_synch_event($"salvate le note del task nel database: {fn}");
                }

                // file -> folder                
                long folder_id = task_file_to_folder(synch_folder_id, (int)task_id, t.title, fn, (int)file_id, parent_id, ct, out string new_folder_path, notes);

                // reparse folder task
                elab_task_folder(res, synch_folder_id, new_folder_path, ct, lwt, folder_id);
              }
            }
          }

          res.files++;
        }

        // task folder - dt_upd
        if(parent_task != null && !check) {
          string cc = db_conn.exec(core.parse_query("lib-notes.upd-task-date", new string[,] { { "task_id", parent_task.id.ToString() } }));
          if(cc != "0") fire_synch_event("aggiornato task date nel database: " + Path.Combine(Path.GetDirectoryName(path), parent_task.title));
        }

      } catch(Exception ex) { log.log_err(ex.Message); res.err = ex.Message; }
      return res;
    }

    protected task elab_task_folder(synch_results res, int synch_folder_id, string folder_path, DateTime ct, DateTime lwt, long folder_id)
    {
      task t = task.parse_task(core, synch_folder_id, folder_path, ct, lwt, _users, _labels, folder_id: folder_id, set_index: true);
      if(t != null) {

        // rinomino il folder
        string folder_name = Path.GetFileName(folder_path);
        if(folder_name.ToLower() != (t.title + ".task").ToLower()) {
          try {
            string dn = Path.GetDirectoryName(folder_path), new_name = t.title + ".task";
            t.path = Path.Combine(dn, new_name);
            if (Directory.Exists(t.path)) throw new Exception("cè già la cartella " + t.path);
            Directory.Move(folder_path, t.path);
            fire_synch_event("rinominato folder: " + folder_path + ", in: " + Path.Combine(dn, new_name));
            folder_name = new_name; folder_path = Path.Combine(dn, new_name);
            update_folder_name_db(folder_name, folder_id);
          } catch(Exception ex) { log.log_err(ex); res.err = ex.Message; }
        }

        // aggiorno l'indice
        DateTime? lwt_i = t.doc != null ? t.doc.lwt : (DateTime?)null;
        if(t.doc != null && t.doc.changed) {
          t.doc.save_into_folder(core, folder_path);
          fire_synch_event("salvato index doc: " + t.doc.path);
          lwt_i = t.doc.lwt;
        }

        t.id = set_task_db(t, out string tp, out int cc, lwt_i);
        if(tp == "insert") fire_synch_event("aggiunto task al database: " + t.title);
        else if(tp == "update" && cc > 0) fire_synch_event("aggiornato task nel database: " + t.title);
      }
      return t;
    }

    public bool set_task_notes_db(long task_id, long file_id, string content, file_type.ft_type_content type, DateTime ct, DateTime lwt)
    {
      return set_task_notes_db(task_id, file_id, content, type, ct, lwt, out string out_notes);
    }

    public bool set_task_notes_db(long task_id, long file_id, string content, file_type.ft_type_content type, DateTime ct, DateTime lwt, out string out_notes)
    {
      bool res = false; out_notes = "";
      if(type == file_type.ft_type_content.info) {
        db_conn.exec(core.parse_query("lib-notes.set-task-notes", new string[,] { { "task_id", task_id.ToString() }, { "file_id", file_id.ToString() }
          , { "ct", ct.ToString("yyyy-MM-dd HH:mm:ss") }, { "lwt", lwt.ToString("yyyy-MM-dd HH:mm:ss") } })
          , pars: new System.Data.Common.DbParameter[] { new SqlParameter("@content", System.Data.SqlDbType.VarChar) { Value = content } });
        out_notes = content; res = true;
      } else if(type == file_type.ft_type_content.source) {
        string key_from = "###FROM_NOTES###", key_to = "###TO_NOTES###";
        int from_n = content.IndexOf(key_from), to_n = from_n >= 0 ? content.IndexOf(key_to, from_n + 1) : -1;
        if(from_n >= 0 && to_n > 0) {
          string notes = content.Substring(from_n + key_from.Length, to_n - from_n - key_from.Length - 1).Trim(new char[] { ' ', '\n', '\r' });
          db_conn.exec(core.parse_query("lib-notes.set-task-notes", new string[,] { { "task_id", task_id.ToString() }, { "file_id", file_id.ToString() }
            , { "ct", ct.ToString("yyyy-MM-dd HH:mm:ss") }, { "lwt", lwt.ToString("yyyy-MM-dd HH:mm:ss") } })
            , pars: new System.Data.Common.DbParameter[] { new SqlParameter("@content", System.Data.SqlDbType.VarChar) { Value = notes } });
          out_notes = notes; res = true;
        }
      }
      return res;
    }

    public long task_file_to_folder(int synch_id, int task_id, string title, string file_path, int file_id, long? parent_folder_id, DateTime c_time, string notes = "")
    {
      return task_file_to_folder(synch_id, task_id, title, file_path, file_id, parent_folder_id, c_time, out string new_folder_path, notes);
    }

    public long task_file_to_folder(int synch_id, int task_id, string title, string file_path, int file_id, long? parent_folder_id, DateTime c_time, out string new_folder_path, string notes = "")
    {
      fire_synch_event($"trasformazione task file in task folder: {file_path}");

      file_type ftp = is_type_file(Path.GetExtension(file_path));
      bool ifile = ftp.type_content == file_type.ft_type_content.info;

      // creo una cartella con questo nome di task e aggiungo le note
      string parent = Path.GetDirectoryName(file_path)
        , name_folder = Path.GetFileNameWithoutExtension(file_path);
      new_folder_path = Path.Combine(parent, name_folder);
      int sfid = synch_id; long? pfid = parent_folder_id;
      DirectoryInfo di = Directory.CreateDirectory(new_folder_path);
      DateTime ct = sys.without_ms(di.CreationTime), lwt = sys.without_ms(di.LastWriteTime);
      string tp; int cc;
      long folder_id = set_folder_db(sfid, pfid, name_folder, ct, lwt, out tp, out cc);
      fire_synch_event("creato folder: " + new_folder_path);

      // sposto il task file      
      string name_file = (ifile ? "i" : "content"), ext = Path.GetExtension(file_path);
      if (File.Exists(Path.Combine(new_folder_path, name_file + ext)))
        File.Delete(Path.Combine(new_folder_path, name_file + ext));
      File.Move(file_path, Path.Combine(new_folder_path, name_file + ext));
      db_conn.exec(core.parse_query("lib-notes.move-file", new string[,] { { "synch_folder_id", synch_id.ToString() }
        , { "name_file", name_file + ext }, { "folder_id", folder_id.ToString() }, { "file_id", file_id.ToString() } }));
      set_folder_task(task_id, (int)folder_id);
      fire_synch_event("spostato file da: " + file_path + ", a:" + Path.Combine(new_folder_path, name_file + ext));
      if(ifile) {
        init_task_notes_db(task_id, file_id, notes.Trim(new char[] { ' ', '\n', '\r' }), false);
        fire_synch_event("salvate le note del task: " + new_folder_path);
      }

      // setto le note
      if(notes != "" && !ifile) {
        string fp = Path.Combine(new_folder_path, "i.txt");
        File.WriteAllText(fp, notes, System.Text.Encoding.UTF8);

        FileInfo fi = new FileInfo(fp);
        ct = sys.without_ms(fi.CreationTime); lwt = sys.without_ms(fi.LastWriteTime);

        long nid = set_file_db(sfid, folder_id, "i.txt", ".txt", ct, lwt, out tp, out cc);
        set_file_content_db((int)nid, ".txt", notes.Trim(new char[] { ' ', '\n', '\r' }), c_time, c_time);
        init_task_notes_db(task_id, (int)nid, notes.Trim(new char[] { ' ', '\n', '\r' }), false);
        fire_synch_event("salvate le note nel file: " + Path.Combine(new_folder_path, "i.txt"));
      }

      return folder_id;
    }

    protected void set_folder_task(int task_id, int folder_id)
    {
      db_conn.exec(core.parse_query("lib-notes.set-task-folder", new string[,] { { "task_id", task_id.ToString() }, { "folder_id", folder_id.ToString() } }));
    }

    public void save_task_notes(int task_id, string notes)
    {
      DataTable dt = db_conn.dt_table(core.parse_query("lib-notes.info-task-notes", new string[,] { { "task_id", task_id.ToString() } }));
      if(dt.Rows.Count == 0) throw new Exception("l'attività " + task_id.ToString() + " non è registrata correttamente!");

      DataRow dr = dt.Rows[0];
      DateTime ct = sys.without_ms(DateTime.Now);

      // aggiornamento del file
      if(db_provider.str_val(dr["file_id_notes"]) != "") {
        int fid = db_provider.int_val(dr["file_id_notes"]);
        string file_path = db_provider.str_val(dr["file_path_notes"]) != "" ?
          db_provider.str_val(dr["file_path_notes"]) : db_provider.str_val(dr["file_path"]);
        System.Text.Encoding e = encoding_type.GetType(file_path);
        // file sorgente
        if(db_provider.str_val(dr["type_content"]) == "source") {
          string all = File.ReadAllText(file_path, e), oc = db_provider.str_val(dr["open_comment"])
            , cc = db_provider.str_val(dr["close_comment"]), key_from = "###FROM_NOTES###", key_to = "###TO_NOTES###";
          // cerco i commenti 
          int from = all.IndexOf(key_from), to = all.IndexOf(key_to, from + 1);
          string src = "";
          if(from >= 0 && to > 0)
            src = all.Substring(0, from + key_from.Length) + "\r\n" + notes + "\r\n" + all.Substring(to);
          else
            src = oc + " " + key_from + "\r\n" + notes + "\r\n" + key_to + cc + "\n\n" + all;
          File.WriteAllText(file_path, src, e);
          set_file_content_db(fid, Path.GetExtension(file_path).ToLower(), src, ct, ct);
          init_task_notes_db(task_id, fid, notes.Trim(new char[] { ' ', '\n', '\r' }));
        }
        // file info
        else {
          File.WriteAllText(file_path, notes, e);
          set_file_content_db(fid, Path.GetExtension(file_path).ToLower(), notes.Trim(new char[] { ' ', '\n', '\r' }), ct, ct);
          init_task_notes_db(task_id, fid, notes.Trim(new char[] { ' ', '\n', '\r' }));
        }
      } // nuovo commento
      else {
        // file
        if(db_provider.str_val(dr["file_id"]) != "") {
          int fid = db_provider.int_val(dr["file_id"]);
          string file_path = db_provider.str_val(dr["file_path"]);
          System.Text.Encoding e = encoding_type.GetType(file_path);
          // sorgente
          if(db_provider.str_val(dr["type_content"]) == "source") {
            string all = File.ReadAllText(file_path, e), oc = db_provider.str_val(dr["open_comment"])
              , cc = db_provider.str_val(dr["close_comment"]), key_from = "###FROM_NOTES###", key_to = "###TO_NOTES###";
            string src = oc + " " + key_from + "\r\n" + notes + "\r\n" + key_to + cc + "\n\n" + all;
            File.WriteAllText(file_path, src, e);
            set_file_content_db(fid, Path.GetExtension(file_path).ToLower(), src, ct, ct);
            init_task_notes_db(task_id, fid, notes.Trim(new char[] { ' ', '\n', '\r' }));
          } else {
            // task file -> folder
            string title = db_provider.str_val(dr["title"]), parent = Path.GetDirectoryName(file_path)
              , name_folder = Path.GetFileNameWithoutExtension(file_path), new_folder = Path.Combine(parent, name_folder);
            int sfid = db_provider.int_val(dr["synch_folder_id"]); long? pfid = db_provider.long_val_null(dr["parent_folder_id"]);
            task_file_to_folder(sfid, task_id, title, file_path, fid, pfid, ct, notes);
          }
        }
        // cartella
        else {
          string fp = db_provider.str_val(dr["folder_path"]) + "i.txt";
          File.WriteAllText(fp, notes, System.Text.Encoding.UTF8);

          FileInfo fi = new FileInfo(fp);
          ct = sys.without_ms(fi.CreationTime); DateTime lwt = sys.without_ms(fi.LastWriteTime);

          long fid = set_file_db(db_provider.int_val(dr["synch_folder_id"]), db_provider.int_val(dr["folder_id"]), "i.txt", ".txt", ct, lwt, out string tp, out int cc);
          set_file_content_db((int)fid, ".txt", notes.Trim(new char[] { ' ', '\n', '\r' }), ct, ct);
          init_task_notes_db(task_id, (int)fid, notes.Trim(new char[] { ' ', '\n', '\r' }));
        }
      }

      // leggo il doc. indice
      if(doc_task.exists_index(core, db_provider.str_val(dr["folder_path"]))) {
        doc_task it = doc_task.open_index(core, db_provider.str_val(dr["folder_path"]));
        it.dt_upd = ct;
        it.save_into_folder(core, db_provider.str_val(dr["folder_path"]));

        // aggiorno l'indice      
        db_conn.exec(core.parse_query("lib-notes.set-task-upd"
            , new string[,] { { "i_lwt", it.lwt.ToString("yyyy-MM-dd HH:mm:ss") }, { "dt_upd", ct.ToString("yyyy-MM-dd HH:mm:ss") }, { "task_id", task_id.ToString() } }));
      }
    }

    public void init_task_notes_db(int task_id, int file_id, string notes, bool upd_task = true)
    {
      db_conn.exec(core.parse_query("lib-notes.init-notes", new string[,] { { "task_id", task_id.ToString() }
        , { "file_id", file_id.ToString() }, { "upd_task", upd_task ? "1" : "0" }})
        , pars: new System.Data.Common.DbParameter[] { new System.Data.SqlClient.SqlParameter("@content", System.Data.SqlDbType.VarChar) { Value = notes } });
    }

    #endregion

  }

  public class synch_event_args : EventArgs
  {
    public string message { get; set; }
    public bool init { get; set; }
  }
}
