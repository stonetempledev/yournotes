using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace dn_lib {
  public class file {
    public int synch_folder_id { get; set; }
    public long folder_id { get; set; }
    public long id { get; set; }
    public string file_name { get; set; }
    public DateTime dt_ins { get; set; }
    public task task { get; set; }
    public bool found { get; set; }

    public file(int synch_folder_id, long folder_id, long id, string file_name, DateTime dt_ins, bool found) {
      this.synch_folder_id = synch_folder_id; this.folder_id = folder_id;
      this.id = id; this.file_name = file_name; this.dt_ins = dt_ins; this.found = found;
    }

  }
}