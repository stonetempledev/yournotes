using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using dn_lib.tools;

namespace dn_lib {
  public class task {
    public long id { get; set; }
    public int synch_folder_id { get; set; }
    public long? file_id { get; set; }
    public long? folder_id { get; set; }
    public string title { get; set; }
    public string user { get; set; }
    public DateTime? dt_lwt { get; set; }
    public DateTime? dt_ref { get; set; }
    public DateTime? dt_upd { get; set; }
    public DateTime? dt_ins { get; set; }
    public task_stato stato { get; set; }
    public task_priorita priorita { get; set; }
    public task_tipo tipo { get; set; }
    public task_stima stima { get; set; }
    public bool has_notes { get; set; }
    public bool has_files { get; set; }
    public int level_folder { get; set; }
    public doc_task doc { get; set; }
    public string path { get; set; }

    public task(int synch_folder_id, long id, long? file_id, long? folder_id, string title, string user
      , DateTime? dt_lwt, DateTime? dt_ref, DateTime? dt_ins, DateTime? dt_upd, task_stato stato
      , task_priorita priorita, task_stima stima, task_tipo tipo, bool has_notes, bool has_files) {
      this.synch_folder_id = synch_folder_id; this.id = id; this.file_id = file_id; this.folder_id = folder_id;
      this.title = title; this.user = user; this.dt_lwt = dt_lwt; this.dt_ref = dt_ref; this.dt_upd = dt_upd; this.dt_ins = dt_ins;
      this.stato = stato; this.priorita = priorita; this.tipo = tipo; this.stima = stima; this.has_notes = has_notes; 
      this.has_files = has_files;
    }

    public task(int synch_folder_id, long? file_id, long? folder_id, string title, string user
      , string stato, string priorita, string tipo, string stima, DateTime? dt_ins, DateTime? dt_upd, DateTime? dt_lwt, doc_task doc) {
      this.synch_folder_id = synch_folder_id; this.file_id = file_id; this.folder_id = folder_id;
      this.title = title; this.user = user; this.dt_upd = dt_upd; this.dt_ins = dt_ins; this.dt_lwt = dt_lwt;
      this.stato = new task_stato(stato, 0, "", "", "");
      this.priorita = new task_priorita(priorita, 0, "", "", "");
      this.tipo = new task_tipo(tipo, 0, "", "", "");
      this.stima = new task_stima(stima, 0, "", "", "");
      this.doc = doc;
    }

    static public task parse_task(core c, int synch_folder_id, string path, DateTime ct, DateTime lwt
      , List<string> users, List<free_label> labels, long? folder_id = null, long? file_id = null, bool set_index = false) {
      try {
        if (!folder_id.HasValue && !file_id.HasValue) throw new Exception("il task dev'essere un file o un folder");

        string[] parts = Path.GetFileName(path).Split(new char[] { '.' });
        if (parts.Length <= 1) return null;

        // task
        if (folder_id.HasValue && parts[parts.Length - 1] != "task") return null;
        else if (file_id.HasValue && parts[parts.Length - 1] != "task" && parts[parts.Length - 2] != "task") return null;

        // leggo il doc. indice
        doc_task it = null;
        if(folder_id.HasValue && set_index) 
           it = doc_task.exists_index(c, path) ? doc_task.open_index(c, path) : doc_task.create_index(c, path);

        // parse nome cartella
        string title = "", user = it != null ? it.user : "", stato = it != null ? it.stato : "", priorita = it != null ? it.priorita : ""
          , tipo = it != null ? it.tipo : "", stima = it != null ? it.stima : "";
        DateTime? dt = null, dt2 = null;
        for (int i = 0; i < parts.Length; i++) {
          string p = parts[i]; if (p == "task") break;

          // date
          if (dn_lib.tools.strings.is_int(p) && (p.Length == 6 || p.Length == 8)) {
            if (!dt.HasValue) parse_date(p, out dt); 
            else if (!dt2.HasValue) parse_date(p, out dt2);
            if (dt.HasValue && dt2.HasValue && dt2 < dt) { DateTime tmp = dt2.Value; dt = dt2; dt2 = tmp; }
            if(it != null && it.created && dt.HasValue) it.dt_create = dt;
            if(it != null && it.created && dt2.HasValue) it.dt_upd = dt2;
            continue;
          }

          // state
          free_label lbl = labels.FirstOrDefault(x => !string.IsNullOrEmpty(x.stato) && x.free_txt.ToLower() == p.ToLower());
          if (lbl != null) { stato = lbl.stato; if(it != null) it.stato = stato; continue; }

          // priorita
          lbl = labels.FirstOrDefault(x => !string.IsNullOrEmpty(x.priorita) && x.free_txt.ToLower() == p.ToLower());
          if (lbl != null) { priorita = lbl.priorita; if(it != null) it.priorita = priorita; continue; }

          // tipo
          lbl = labels.FirstOrDefault(x => !string.IsNullOrEmpty(x.tipo) && x.free_txt.ToLower() == p.ToLower());
          if (lbl != null) { tipo = lbl.tipo; if(it != null) it.tipo = tipo; continue; }

          // stima
          lbl = labels.FirstOrDefault(x => !string.IsNullOrEmpty(x.stima) && x.free_txt.ToLower() == p.ToLower());
          if (lbl != null) { stima = lbl.stima; if(it != null) it.stima = stima; continue; }

          // user
          if (user == "" && users.FirstOrDefault(x => x.ToLower() == p.ToLower()) != null) { user = p; if(it != null) it.user = user;  continue; }

          // title
          if (title == "") title = p;

        }

        return new task(synch_folder_id, file_id, folder_id, title, user, stato, priorita, tipo, stima
          , it != null ? it.dt_create : (dt.HasValue ? dt : ct), it != null ? it.dt_upd : (DateTime?)null, lwt, it) { path = path };
      } catch(Exception ex) { log.log_err(ex); return null; }
    }

    static protected bool parse_date(string txt, out DateTime? dt) {
      dt = null;
      try {
        if (string.IsNullOrEmpty(txt) || !txt.All(char.IsDigit)) return false;
        if (txt.Length == 6) {
          dt = new DateTime(2000 + int.Parse(txt.Substring(0, 2)), int.Parse(txt.Substring(2, 2))
            , int.Parse(txt.Substring(4, 2))); return true;
        } else if (txt.Length == 8) {
          dt = new DateTime(int.Parse(txt.Substring(0, 4)), int.Parse(txt.Substring(4, 2)), int.Parse(txt.Substring(6, 2))); return true;
        }
      } catch { }
      return false;
    }


  }
}
