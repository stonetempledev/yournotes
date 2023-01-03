using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.IO;
using dn_lib;
using dn_lib.db;

namespace deepanotes
{
  public class notes : io
  {

    public List<synch_folder> synch_folders { get; protected set; }
    public List<task> tasks { get; protected set; }

    public notes()
    {
    }

    public static synch create_synch() { bo b = new bo(); return new synch(b.db_conn, b.core, b.config, b.user_id, b.user_name); }

    public synch get_synch() { return new synch(this.db_conn, this.core, this.config, this.user_id, this.user_name); }

    public folder find_folder(long folder_id)
    {
      foreach(synch_folder sf in this.synch_folders) {
        folder res = sf.get_folder(folder_id);
        if(res != null) return res;
      }
      return null;
    }

    public synch_folder find_synch_folder(long synch_folder_id)
    {
      return this.synch_folders.FirstOrDefault(x => x.id == synch_folder_id);
    }

    public void load_objects(int? folder_id = null, int? synch_folder_id = null, task_filter tf = null, int? search_id = null)
    {

      // folders structure
      this.synch_folders = load_folders(folder_id, synch_folder_id, search_id);

      // tasks
      this.tasks = load_tasks(folder_id: folder_id, synch_folder_id: synch_folder_id, tf: tf, search_id: search_id);

      // task -> folder, file
      foreach(task t in this.tasks) {
        if(t.folder_id.HasValue && !t.file_id.HasValue)
          this.synch_folders.First(x => x.id == t.synch_folder_id).get_folder(t.folder_id.Value).task = t;
        else
          this.synch_folders.First(x => x.id == t.synch_folder_id).get_file(t.file_id.Value).task = t;
      }

    }

    public List<free_label> load_free_labels()
    {
      return db_conn.dt_table(core.parse_query("lib-notes.free-labels")).Rows.Cast<DataRow>()
        .Select(r => new free_label(db_provider.str_val(r["free_txt"]), db_provider.str_val(r["stato"])
          , db_provider.str_val(r["priorita"]), db_provider.str_val(r["tipo"]), db_provider.str_val(r["stima"])
          , db_provider.str_val(r["default"]) == "1")).ToList();
    }

    public DataRow get_task_info(int task_id)
    {
      return db_conn.first_row(core.parse_query("lib-notes.task-info", new string[,] { { "task_id", task_id.ToString() } }));
    }

    public task load_task(int task_id) { List<task> l = load_tasks(task_id); return l.Count > 0 ? l[0] : null; }

    protected List<task> load_tasks(int? task_id = null, int? folder_id = null, int? synch_folder_id = null, task_filter tf = null, int? search_id = null)
    {

      // filtro
      string filters = tf != null ? tf.def : "";
      if(filters != "") {
        int i_filter = filters.IndexOf("{");
        while(i_filter >= 0) {
          int i_end = filters.IndexOf("}", i_filter);
          if(i_end <= 0) throw new Exception("il filtro '" + tf.def + "' non è definito correttamente!");
          string sub = filters.Substring(i_filter, (i_end - i_filter) + 1);

          // vedo se c'è un campo
          string field = "", name_field = "";
          int f_from = sub.IndexOf("[");
          if(f_from >= 0) {
            int f_end = sub.IndexOf("]", f_from);
            if(f_end <= 0) throw new Exception("il filtro '" + tf.def + "' non è definito correttamente!");
            field = sub.Substring(f_from, (f_end - f_from) + 1);
            name_field = field.Substring(1, field.Length - 2).Trim();
          }

          string new_sub = sub.Substring(1, sub.Length - 2).Replace(field, "t3." + name_field);
          filters = filters.Replace(sub, new_sub);

          i_filter = filters.IndexOf("{");
        }
      }

      string[,] pars = new string[,] { { "task_id", task_id.HasValue ? task_id.Value.ToString() : "null" }
        , { "folder_id", folder_id.HasValue ? folder_id.Value.ToString() : "null" }
        , { "search", search_id.HasValue ? "true" : "false" }, { "search_id", search_id.HasValue ? search_id.Value.ToString() : "0" }
        , { "synch_folder_id", synch_folder_id.HasValue ? synch_folder_id.Value.ToString() : "null" }
        , { "filter_sf", !synch_folder_id.HasValue && folder_id.HasValue ? " ft.tp <> 'synch_folder' and " : "" }
        , { "filters", filters != "" ? " and " + filters : "" } };

      return db_conn.dt_table(core.parse_query("lib-notes.tasks", pars)).Rows.Cast<DataRow>()
        .Select(dr => new task(db_provider.int_val(dr["synch_folder_id"]), db_provider.long_val(dr["task_id"])
          , db_provider.long_val_null(dr["file_id"]), db_provider.long_val_null(dr["folder_id"])
          , db_provider.str_val(dr["title"]), db_provider.str_val(dr["user"])
          , db_provider.dt_val(dr["dt_lwt"]), db_provider.dt_val(dr["dt_ref"]), db_provider.dt_val(dr["dt_ins"]), db_provider.dt_val(dr["dt_upd"])
          , new task_stato(db_provider.str_val(dr["stato"]), db_provider.int_val(dr["stato_order"], 99)
            , db_provider.str_val(dr["stato_class"]), db_provider.str_val(dr["stato_title_plurale"]), db_provider.str_val(dr["stato_title_singolare"]))
          , new task_priorita(db_provider.str_val(dr["priorita"]), db_provider.int_val(dr["priorita_order"], 99)
            , db_provider.str_val(dr["priorita_class"]), db_provider.str_val(dr["priorita_title_plurale"]), db_provider.str_val(dr["priorita_title_singolare"]))
          , new task_stima(db_provider.str_val(dr["stima"]), db_provider.float_val(dr["stima_days"])
            , db_provider.str_val(dr["stima_class"]), db_provider.str_val(dr["stima_title_plurale"]), db_provider.str_val(dr["stima_title_singolare"]))
          , new task_tipo(db_provider.str_val(dr["tipo"]), db_provider.int_val(dr["tipo_order"], 99)
            , db_provider.str_val(dr["tipo_class"]), db_provider.str_val(dr["tipo_title_plurale"]), db_provider.str_val(dr["tipo_title_singolare"]))
          , db_provider.int_val(dr["task_notes"]) == 1, db_provider.int_val(dr["task_files"]) == 1)).ToList();
    }

    protected List<synch_folder> load_folders(int? folder_id = null, int? synch_folder_id = null, int? search_id = null)
    {

      string[,] pars = new string[,] { { "folder_id", folder_id.HasValue ? folder_id.Value.ToString() : "null" }
        , { "search", search_id.HasValue ? "true" : "false" }, { "search_id", search_id.HasValue ? search_id.Value.ToString() : "0" }
        , { "synch_folder_id", synch_folder_id.HasValue ? synch_folder_id.Value.ToString() : "null" } };

      // folders
      List<synch_folder> res = new List<synch_folder>();
      foreach(DataRow dr in db_conn.dt_table(core.parse_query("lib-notes.folders", pars)).Rows) {
        string tp = db_provider.str_val(dr["tp"]);
        int lf = db_provider.int_val(dr["lvl"]);
        if(tp == "synch_folder") {
          res.Add(new synch_folder(db_provider.int_val(dr["synch_folder_id"])
            , db_provider.str_val(dr["title"]), db_provider.str_val(dr["des"]), db_provider.str_val(dr["http_path"])));
        } else if(tp == "folder") {
          folder f = new folder(db_provider.int_val(dr["synch_folder_id"]), db_provider.int_val(dr["folder_id"])
            , db_provider.int_val_null(dr["parent_id"]), db_provider.str_val(dr["title"]), db_provider.str_val(dr["folder_path"])
            , db_provider.int_val(dr["is_task"]) > 0);
          if(lf == 1) {
            synch_folder p = res.FirstOrDefault(x => x.id == f.synch_folder_id);
            if(p == null) throw new Exception("il synch_folder con id " + f.synch_folder_id.ToString() + " non è stato trovato!");
            p.add_folder(f);
          } else {
            folder p = null;
            foreach(synch_folder s in res) {
              p = s.get_folder(f.parent_id.Value);
              if(p != null) break;
            }
            if(p == null) throw new Exception("il folder con id " + f.parent_id.Value.ToString() + " non è stato trovato!");
            p.add_folder(f);
          }
        } else throw new Exception("tp row '" + tp + "' non supportato!");
      }

      // files
      foreach(DataRow dr in db_conn.dt_table(core.parse_query("lib-notes.files", pars)).Rows) {
        int sfi = db_provider.int_val(dr["synch_folder_id"]);
        long fi = db_provider.long_val(dr["folder_id"]);
        file f = new file(db_provider.int_val(dr["synch_folder_id"]), db_provider.long_val(dr["folder_id"])
            , db_provider.long_val(dr["file_id"]), db_provider.str_val(dr["file_name"])
            , db_provider.dt_val(dr["dt_ins"]).Value, db_provider.int_val(dr["found_file"]) > 0);
        if(fi > 0) res.First(x => x.id == sfi).get_folder(fi).add_file(f);
        else res.First(x => x.id == sfi).add_file(f);
      }

      return res;
    }

    public void remove_task(int task_id)
    {
      // aggiorno il file/folder
      DataRow r = db_conn.first_row(core.parse_query("lib-notes.task-paths", new string[,] { { "task_id", task_id.ToString() } }));
      if(r == null) throw new Exception("il task " + task_id.ToString() + " non c'è in tabella!");
      string folder_path = db_provider.str_val(r["folder_path"]), f_name = db_provider.str_val(r["folder_name"])
        , src = Path.Combine(folder_path, f_name);
      if(db_provider.int_val(r["file_id"]) > 0) {
        if(File.Exists(src)) File.Delete(src);
      } else { if(Directory.Exists(src)) Directory.Delete(src, true); }

      // aggiorno il db
      if(db_provider.int_val(r["file_id"]) > 0)
        db_conn.exec(core.parse_query("lib-notes.del-file", new string[,] { { "file_id", r["file_id"].ToString() } }));
      else
        db_conn.exec(core.parse_query("lib-notes.del-folder", new string[,] { { "folder_id", r["folder_id"].ToString() } }));
    }

    public void remove_att(int file_id)
    {
      // cancello il file
      DataRow r = db_conn.first_row(core.parse_query("lib-notes.file-path", new string[,] { { "file_id", file_id.ToString() } }));
      if(r == null) throw new Exception("il file " + file_id.ToString() + " non esiste!");
      File.Delete(db_provider.str_val(r["file_path"]));

      // aggiorno il db
      db_conn.exec(core.parse_query("lib-notes.del-file", new string[,] { { "file_id", file_id.ToString() } }));
    }

    public void update_task(int task_id, out string rel_path, List<free_label> fl, string title = null, string assegna = null
      , string stato = null, string priorita = null, string stima = null, string tipo = null)
    {
      rel_path = "";

      // aggiorno il file/folder
      DataRow r = db_conn.first_row(core.parse_query("lib-notes.task-paths", new string[,] { { "task_id", task_id.ToString() } }));
      if(r == null) throw new Exception("il task " + task_id.ToString() + " non c'è in tabella!");
      if(db_provider.int_val(r["file_id"]) > 0) throw new Exception("questo tipo di task non è supportato, lanciare il comando synch!");
      rel_path = db_provider.str_val(r["rel_path"]);
      string folder_path = db_provider.str_val(r["folder_path"]), name = !string.IsNullOrEmpty(title) ? title : db_provider.str_val(r["task_name"])
        , f_name = db_provider.str_val(r["folder_name"]), src = Path.Combine(folder_path, f_name);

      // leggo il doc. indice
      doc_task it = null;
      if(doc_task.exists_index(core, src))
        it = doc_task.open_index(core, src);
      else it = doc_task.create_index(core, src);

      // rinomino il folder
      string folder_name = Path.GetFileName(src);
      if(folder_name.ToLower() != (name + ".task").ToLower()) {
        string new_name = name + ".task", new_path = Path.Combine(Path.GetDirectoryName(src), new_name);
        if (Directory.Exists(new_path)) throw new Exception("cè già la cartella " + new_path);
        Directory.Move(src, new_path);
        src = new_path;
        db_conn.exec(core.parse_query("lib-notes.update-folder-name"
          , new Dictionary<string, object>() { { "folder_id", r["folder_id"].ToString() }, { "folder_name", new_name } }));

        // title
        if(!string.IsNullOrEmpty(title))
          db_conn.exec(core.parse_query("lib-notes.set-task-title"
            , new string[,] { { "title", title }, { "task_id", task_id.ToString() } }));
      }

      // stato 
      if(stato != null) {
        it.stato = stato;
        db_conn.exec(core.parse_query("lib-notes.set-task-state"
          , new string[,] { { "stato", stato }, { "task_id", task_id.ToString() } }));
      }

      // assegna
      if(assegna != null) {
        it.user = assegna;
        db_conn.exec(core.parse_query("lib-notes.set-task-user"
          , new string[,] { { "user", assegna }, { "task_id", task_id.ToString() } }));
      }

      // priorita 
      if(priorita != null) {
        it.priorita = priorita;
        db_conn.exec(core.parse_query("lib-notes.set-task-priorita"
          , new string[,] { { "priorita", priorita }, { "task_id", task_id.ToString() } }));
      }

      // stima 
      if(stima != null) {
        it.stima = stima;
        db_conn.exec(core.parse_query("lib-notes.set-task-stima"
          , new string[,] { { "stima", stima }, { "task_id", task_id.ToString() } }));
      }

      // tipo
      if(tipo != null) {
        it.tipo = tipo;
        db_conn.exec(core.parse_query("lib-notes.set-task-tipo"
          , new string[,] { { "tipo", tipo }, { "task_id", task_id.ToString() } }));
      }

      // aggiorno l'indice      
      if(it.changed) {
        it.dt_upd = dn_lib.tools.sys.without_ms(DateTime.Now);
        it.save_into_folder(core, src);
        db_conn.exec(core.parse_query("lib-notes.set-task-upd"
          , new string[,] { { "i_lwt", it.lwt.ToString("yyyy-MM-dd HH:mm:ss") }, { "dt_upd", it.dt_upd.Value.ToString("yyyy-MM-dd HH:mm:ss") }, { "task_id", task_id.ToString() } }));
      }
    }

    public void ren_task(int task_id, string title)
    {
      // aggiorno il file/folder
      DataRow r = db_conn.first_row(core.parse_query("lib-notes.task-paths", new string[,] { { "task_id", task_id.ToString() } }));
      if(r == null) throw new Exception("il task " + task_id.ToString() + " non c'è in tabella!");
      string folder_path = db_provider.str_val(r["folder_path"]), name = db_provider.str_val(r["task_name"])
        , f_name = db_provider.str_val(r["folder_name"]), new_name = f_name;

      // title
      if(!string.IsNullOrEmpty(title)) new_name = new_name.Replace(db_provider.str_val(r["title"]) + ".", title + ".");

      string src = Path.Combine(folder_path, f_name);
      if(db_provider.int_val(r["file_id"]) > 0) {
        if(!File.Exists(src)) throw new Exception("il file non c'è, non è possibile aggiornare l'attività!");
        File.Move(src, Path.Combine(folder_path, new_name));
      } else {
        if(!Directory.Exists(src)) throw new Exception("la cartella non c'è, non è possibile aggiornare l'attività!");
        Directory.Move(src, Path.Combine(folder_path, new_name));
      }

      // title
      if(!string.IsNullOrEmpty(title))
        db_conn.exec(core.parse_query("lib-notes.set-task-title"
          , new string[,] { { "title", title }, { "task_id", task_id.ToString() } }));

      // aggiorno il file nel db
      if(db_provider.int_val(r["file_id"]) > 0)
        db_conn.exec(core.parse_query("lib-notes.set-file-name", new string[,] { { "file_id", r["file_id"].ToString() }, { "file_name", new_name } }));
      else
        db_conn.exec(core.parse_query("lib-notes.set-folder-name", new string[,] { { "folder_id", r["folder_id"].ToString() }, { "folder_name", new_name } }));
    }

    public string get_task_notes(int task_id)
    {
      DataTable dt = db_conn.dt_table(core.parse_query("lib-notes.get-task-notes", new string[,] { { "task_id", task_id.ToString() } }), true);
      return dt.Rows.Count > 0 ? db_provider.str_val(dt.Rows[0]["content"]) : "";
    }

    public DataTable get_task_allegati(int task_id, int? search_id)
    {
      return db_conn.dt_table(core.parse_query("lib-notes.get-task-allegati", new string[,] { { "task_id", task_id.ToString() }
        , { "search", search_id.HasValue ? "true" : "false" }, { "search_id", search_id.HasValue ? search_id.Value.ToString() : "0" } }));
    }

    public void add_folder(int synch_folder_id, int folder_id, string title)
    {
      DataRow r = folder_id > 0 ? db_conn.first_row(core.parse_query("folder-local-path"
        , new string[,] { { "folder_id", folder_id.ToString() } })) :
        db_conn.first_row(core.parse_query("synch-folder-local-path"
        , new string[,] { { "synch_folder_id", synch_folder_id.ToString() } }));

      string folder_path = folder_id > 0 ? r["local_path"].ToString() : r["local_path"].ToString();
      int sf_id = (int)r["synch_folder_id"];

      string path = Path.Combine(folder_path, title);
      if(Directory.Exists(path)) throw new Exception("esiste già una cartella '" + title + "'!");
      Directory.CreateDirectory(path);

      db_conn.exec(core.parse_query("add-folder", new string[,] { { "synch_folder_id", sf_id.ToString() }
        , {"parent_id", folder_id > 0 ? folder_id.ToString(): "null"}, { "folder_name", title } }), true);
    }

    public int get_synch_folder_id(int folder_id)
    {
      return Convert.ToInt32(db_conn.dt_table(core.parse_query("get-synch-folder-id"
        , new string[,] { { "folder_id", folder_id.ToString() } })).Rows[0]["synch_folder_id"]);
    }

    public int get_file_task(int task_id, out string tp, out int folder_id, out string file_name)
    {
      folder_id = -1; file_name = "";
      DataRow r = db_conn.dt_table(core.parse_query("get-file-task"
        , new string[,] { { "task_id", task_id.ToString() } })).Rows[0];
      // file
      if(db_provider.int_val(r["file_id"]) > 0) {
        tp = "file"; folder_id = db_provider.int_val(r["folder_id"]);
        file_name = db_provider.str_val(r["file_name"]);
        return db_provider.int_val(r["file_id"]);
      }
      // folder
      tp = "folder"; return db_provider.int_val(r["folder_id"]);
    }

    public void move_file(int file_id, int folder_id, int synch_folder_id, string folder_path)
    {
      // sposto il file
      DataRow r = db_conn.first_row(core.parse_query("lib-notes.file-path", new string[,] { { "file_id", file_id.ToString() } }));
      if(r == null) throw new Exception("il file " + file_id.ToString() + " non esiste!");
      string fp = db_provider.str_val(r["file_path"]);
      File.Move(fp, Path.Combine(folder_path, Path.GetFileName(fp)));

      // aggiorno il db
      db_conn.exec(core.parse_query("move-file", new string[,] { { "file_id", file_id.ToString() }
        , { "folder_id", folder_id.ToString() }, { "synch_folder_id", synch_folder_id.ToString() } }));
    }

    public void copy_file(int file_id, int folder_id, int synch_folder_id, string folder_path)
    {
      // sposto il file
      DataRow r = db_conn.first_row(core.parse_query("lib-notes.file-path", new string[,] { { "file_id", file_id.ToString() } }));
      if(r == null) throw new Exception("il file " + file_id.ToString() + " non esiste!");

      string file_name = "";
      int i = 1;
      while(true) {
        string fp = db_provider.str_val(r["file_path"]), fn = Path.GetFileNameWithoutExtension(fp), suff = i > 1 ? i.ToString() : ""
          , fe = Path.GetExtension(fp), fp2 = Path.Combine(folder_path, fn + suff + fe);
        if(!File.Exists(fp2)) { file_name = fn + suff + fe; File.Copy(fp, fp2); break; }
        i++;
      }

      // aggiorno il db
      db_conn.exec(core.parse_query("copy-file", new string[,] { { "file_id", file_id.ToString() }, { "file_name", file_name }
        , { "folder_id", folder_id.ToString() }, { "synch_folder_id", synch_folder_id.ToString() } }));
    }

    public void move_folder(int folder_id, int synch_folder_id, int? parent_id)
    {
      // sposto la cartella
      string from_path = db_conn.first_row(core.parse_query("folder-local-path"
        , new string[,] { { "folder_id", folder_id.ToString() } }))["local_path"].ToString()
        , to_path = parent_id.HasValue ? db_conn.first_row(core.parse_query("folder-local-path"
          , new string[,] { { "folder_id", parent_id.Value.ToString() } }))["local_path"].ToString()
          : db_conn.first_row(core.parse_query("synch-folder-local-path"
            , new string[,] { { "synch_folder_id", synch_folder_id.ToString() } }))["local_path"].ToString();
      string to = Path.Combine(to_path, (new DirectoryInfo(from_path)).Name);
      if(Directory.Exists(to)) throw new Exception("esiste già una cartella '" + (new DirectoryInfo(from_path)).Name + "'!");
      Directory.Move(from_path, to);

      // aggiorno la tabella
      db_conn.exec(core.parse_query("set-folder-new-parent", new string[,] { { "folder_id", folder_id.ToString() }
        , {"synch_folder_id", synch_folder_id.ToString() }, { "parent_id", parent_id.HasValue ? parent_id.Value.ToString() : "null" } }));
    }

    public void move_task(int task_id, int synch_folder_id, int? parent_id)
    {
      string tp_file; int folder_id; string file_name;
      int id_file = get_file_task(task_id, out tp_file, out folder_id, out file_name);

      // sposto il task
      if(tp_file == "folder") {
        string from_path = db_conn.first_row(core.parse_query("folder-local-path"
          , new string[,] { { "folder_id", id_file.ToString() } }))["local_path"].ToString()
          , to_path = parent_id.HasValue ? db_conn.first_row(core.parse_query("folder-local-path"
            , new string[,] { { "folder_id", parent_id.Value.ToString() } }))["local_path"].ToString()
            : db_conn.first_row(core.parse_query("synch-folder-local-path"
              , new string[,] { { "synch_folder_id", synch_folder_id.ToString() } }))["local_path"].ToString();
        string to = Path.Combine(to_path, (new DirectoryInfo(from_path)).Name);
        if(Directory.Exists(to)) throw new Exception("esiste già una cartella '" + (new DirectoryInfo(from_path)).Name + "'!");
        Directory.Move(from_path, to);

        // aggiorno il task
        db_conn.exec(core.parse_query("set-folder-new-parent", new string[,] { { "folder_id", id_file.ToString() }
        , {"synch_folder_id", synch_folder_id.ToString() }, { "parent_id", parent_id.HasValue ? parent_id.Value.ToString() : "null" } }));
      } else {
        string from_path = db_conn.first_row(core.parse_query("folder-local-path"
          , new string[,] { { "folder_id", folder_id.ToString() } }))["local_path"].ToString()
          , to_path = parent_id.HasValue ? db_conn.first_row(core.parse_query("folder-local-path"
            , new string[,] { { "folder_id", parent_id.Value.ToString() } }))["local_path"].ToString()
            : db_conn.first_row(core.parse_query("synch-folder-local-path"
              , new string[,] { { "synch_folder_id", synch_folder_id.ToString() } }))["local_path"].ToString();
        from_path = Path.Combine(from_path, file_name);
        string to = Path.Combine(to_path, (new DirectoryInfo(from_path)).Name);
        if(File.Exists(to)) throw new Exception("esiste già il file '" + file_name + "'!");
        File.Move(from_path, to);

        // aggiorno il task
        db_conn.exec(core.parse_query("set-file-new-parent", new string[,] { { "file_id", id_file.ToString() }
        , {"synch_folder_id", synch_folder_id.ToString() }, { "parent_id", parent_id.HasValue ? parent_id.Value.ToString() : "null" } }));
      }
    }

    public List<int> ids_childs_folders(int sf_id)
    {
      return db_conn.dt_table(core.parse_query("ids-childs-folders", new string[,] { { "synch_folder_id", sf_id.ToString() } }))
        .Rows.Cast<DataRow>().Select(x => Convert.ToInt32(x["folder_id"])).ToList();
    }

    public void ren_folder(int synch_folder_id, int folder_id, string title)
    {
      DataRow r = folder_id > 0 ? db_conn.first_row(core.parse_query("folder-local-path"
        , new string[,] { { "folder_id", folder_id.ToString() } })) :
        db_conn.first_row(core.parse_query("synch-folder-local-path"
        , new string[,] { { "synch_folder_id", synch_folder_id.ToString() } }));

      string folder_path = (string)r["local_path"], name = (string)r["name"];
      int sf_id = (int)r["synch_folder_id"];

      if(folder_id > 0) {
        string path = folder_path, path2 = Path.Combine(Directory.GetParent(path).Parent.FullName, title);
        if(Directory.Exists(path2)) throw new Exception("esiste già una cartella '" + title + "'!");
        Directory.Move(path, path2);
      }

      if(folder_id > 0)
        db_conn.exec(core.parse_query("ren-folder", new string[,] { { "folder_id", folder_id.ToString() }
          , { "folder_name", title } }), true);
      else
        db_conn.exec(core.parse_query("ren-synch-folder", new string[,] { { "synch_folder_id", sf_id.ToString() }
          , { "title", title } }), true);
    }

    public void ren_file(int file_id, string name)
    {
      // rinomino il file
      DataRow r = db_conn.first_row(core.parse_query("lib-notes.file-path", new string[,] { { "file_id", file_id.ToString() } }));
      if(r == null) throw new Exception("il file " + file_id.ToString() + " non esiste!");
      string fp = db_provider.str_val(r["file_path"]);
      File.Move(fp, Path.Combine(Path.GetDirectoryName(fp), name));

      // aggiorno il db
      db_conn.exec(core.parse_query("ren-file", new string[,] { { "file_id", file_id.ToString() }, { "file_name", name } }));
    }

    public void del_folder(int synch_folder_id, int folder_id)
    {
      DataRow r = folder_id > 0 ? db_conn.first_row(core.parse_query("folder-local-path"
        , new string[,] { { "folder_id", folder_id.ToString() } })) :
        db_conn.first_row(core.parse_query("synch-folder-local-path"
        , new string[,] { { "synch_folder_id", synch_folder_id.ToString() } }));

      string folder_path = (string)r["local_path"];

      if(folder_id > 0)
        Directory.Delete(folder_path, true);
      else {
        foreach(string f in Directory.EnumerateFiles(folder_path)) {
          if(Path.GetFileName(f).ToLower() == "web.config")
            continue;

          File.Delete(f);
        }
        foreach(string d in Directory.EnumerateDirectories(folder_path))
          Directory.Delete(d, true);
      }

      if(folder_id > 0)
        db_conn.exec(core.parse_query("del-folder", new string[,] { { "synch_folder_id", r["synch_folder_id"].ToString() }
          , { "folder_id", folder_id.ToString() } }), true);
      else
        db_conn.exec(core.parse_query("clean-synch", new string[,] { { "synch_folder_id", synch_folder_id.ToString() } }), true);
    }

    public void add_task(int synch_folder_id, int folder_id, int? search_id, string stato
      , string title, string assegna, string priorita, string tipo, string stima)
    {
      if(string.IsNullOrEmpty(title)) throw new Exception("il titolo non è stato specificato!");

      string folder_path = folder_id > 0 ? db_conn.first_row(core.parse_query("folder-local-path"
        , new string[,] { { "folder_id", folder_id.ToString() } }))["local_path"].ToString() :
        db_conn.first_row(core.parse_query("synch-folder-local-path"
        , new string[,] { { "synch_folder_id", synch_folder_id.ToString() } }))["local_path"].ToString();

      List<free_label> fl = load_free_labels();

      // creo la cartella fisica
      string name = title + ".task", fp = Path.Combine(folder_path, name);
      Directory.CreateDirectory(fp);

      // creo l'indice      
      doc_task it = doc_task.exists_index(this.core, fp) ? doc_task.open_index(this.core, fp) : doc_task.create_index(this.core, fp);
      it.dt_create = DateTime.Now;
      if(!string.IsNullOrEmpty(assegna)) it.user = assegna;
      if(!string.IsNullOrEmpty(stato)) it.stato = stato;
      if(!string.IsNullOrEmpty(priorita)) it.priorita = priorita;
      if(!string.IsNullOrEmpty(tipo)) it.tipo = tipo;
      if(!string.IsNullOrEmpty(stima)) it.stima = stima;
      it.save();

      // aggiorno il db
      long new_folder_id = long.Parse(synch_folder_id > 0 ?
        db_conn.exec(core.parse_query("lib-notes.task-ins-into-synch", new string[,] { { "synch_folder_id", synch_folder_id.ToString() }, { "name", name } }), true)
        : db_conn.exec(core.parse_query("lib-notes.task-ins-into-folder", new string[,] { { "folder_id", folder_id.ToString() }, { "name", name } }), true));

      long new_task_id = long.Parse(db_conn.exec(core.parse_query("lib-notes.ins-task-from-folder", new string[,] { { "folder_id", new_folder_id.ToString() }
        , { "title", title }, { "user", assegna }, { "stato", !string.IsNullOrEmpty(stato) ? stato : "da_fare" }
        , { "priorita", priorita }, { "tipo", tipo }, { "stima", stima }, { "dt_lwt_index", it.lwt.ToString("yyyy/MM/dd HH:mm:ss") } }), true));

      if(search_id.HasValue)
        db_conn.exec(core.parse_query("lib-notes.ins-task-into-search", new string[,] { { "search_id", search_id.ToString() }, { "task_id", new_task_id.ToString() } }));
    }

    public int search_task(string text, string session_id, out int cc)
    {
      DataRow r = db_conn.first_row(core.parse_query("search-notes", new string[,] { { "text", text }, { "session_id", session_id } }));
      cc = db_provider.int_val(r["cc"]);
      return db_provider.int_val(r["search_id"]);
    }
  }
}