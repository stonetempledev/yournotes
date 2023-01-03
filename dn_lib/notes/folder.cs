using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace dn_lib {
  public class folder {
    public int synch_folder_id { get; set; }
    public int folder_id { get; set; }
    public int? parent_id { get; set; }
    public string folder_name { get; set; }
    public task task { get; set; }
    public string path { get; set; }
    public bool is_task { get; set; }

    public List<folder> folders { get; protected set; }
    public folder add_folder(folder f) { this.folders.Add(f); return f; }

    public List<file> files { get; protected set; }
    public file add_file(file f) { this.files.Add(f); return f; }

    public List<task> tasks {
      get {
        return folders.Where(f => f.task != null).Select(x => x.task)
          .Concat(files.Where(f => f.task != null).Select(x => x.task)).ToList();
      }
    }

    public List<task> sub_tasks {
      get {
        List<task> st = new List<task>();
        st.AddRange(folders.Where(f => f.task != null).Select(x => x.task)
          .Concat(files.Where(f => f.task != null).Select(x => x.task)));
        foreach (folder f in this.folders)
          f.add_sub_tasks(st);
        return st;
      }
    }

    protected void add_sub_tasks(List<task> st) {
      st.AddRange(folders.Where(f => f.task != null).Select(x => x.task)
        .Concat(files.Where(f => f.task != null).Select(x => x.task)));
      foreach (folder f in this.folders)
        f.add_sub_tasks(st);
    }

    public folder(int synch_folder_id, int folder_id, int? parent_id, string folder_name, string folder_path, bool is_task) {
      this.synch_folder_id = synch_folder_id; this.folder_id = folder_id; this.is_task = is_task;
      this.parent_id = parent_id; this.folder_name = folder_name; this.path = folder_path; 
      this.folders = new List<folder>();
      this.files = new List<file>();
    }

    public folder get_folder(long folder_id) {
      folder res = this.folders.FirstOrDefault(f => f.folder_id == folder_id);
      if (res != null) return res;
      foreach (folder f in folders) {
        res = f.get_folder(folder_id);
        if (res != null) return res;
      }
      return null;
    }

    public file get_file(long file_id) {
      file res = this.files.FirstOrDefault(f => f.id == file_id);
      if (res != null) return res;
      foreach (folder f in folders) {
        res = f.get_file(file_id);
        if (res != null) return res;
      }
      return null;
    }

  }
}