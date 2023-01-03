using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using dn_lib.db;

namespace dn_lib {
  public class synch_folder {

    public int id { get; set; }
    public string pc_name { get; set; }
    public string title { get; set; }
    public string des { get; set; }
    public string local_path { get; set; }
    public string http_path { get; set; }
    public string user { get; set; }
    public string password { get; set; }

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

    public synch_folder(int id, string pc_name, string title, string des, string local_path, string http_path, string user, string password) {
      this.id = id; this.pc_name = pc_name; this.title = title; this.des = des;
      this.local_path = local_path; this.http_path = http_path;  
      this.user = user; this.password = password;
    }

    public synch_folder(int id, string title, string des, string http_path) {
      this.id = id; this.title = title; this.des = des; this.http_path = http_path;
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

    //public string get_folder_path(long? folder_id) {
    //  if (!folder_id.HasValue) return this.title + "/";
    //  string folders = "";
    //  while (true) {
    //    folder f = get_folder(folder_id.Value);
    //    if (f == null) break;
    //    folders = f.folder_name + "/" + folders;
    //    if (!f.parent_id.HasValue) break;
    //    folder_id = f.parent_id.Value;
    //  }
    //  return this.title + "/" + folders;
    //}

  }
}
