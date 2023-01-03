using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using dn_lib.xml;
using dn_lib.tools;

namespace dn_lib
{
  public class doc_task : xml_doc
  {
    public core c { get; set; }

    protected DateTime _lwt;
    public DateTime lwt { get { return _lwt; } set { _lwt = sys.without_ms(value); } }
    public bool changed { get; set; }
    public bool created { get; set; }

    public doc_task(string path) : base(path) { this.lwt = sys.without_ms(new FileInfo(path).LastWriteTime); }
    public doc_task() : base() { }

    public override void save(string path = "") { base.save(path); this.lwt = sys.without_ms(new FileInfo(_path).LastWriteTime); this.changed = false; }
    public void save_into_folder(core c, string folder_path)
    {
      string i_name = c.config.get_var("lib-vars.index-folder").value
        , fp = Path.Combine(folder_path, i_name);
      this.save(fp);

      FileInfo fi = new FileInfo(fp); _lwt = sys.without_ms(fi.LastWriteTime);
    }

    public static bool exists_index(core c, string folder_path)
    {
      return File.Exists(Path.Combine(folder_path, c.config.get_var("lib-vars.index-folder").value));
    }

    public static doc_task open_index(core c, string folder_path)
    {
      doc_task res = null;
      string i_name = c.config.get_var("lib-vars.index-folder").value
        , fp = Path.Combine(folder_path, i_name);
      return File.Exists(fp) ? new doc_task(fp) : null;
    }

    public static doc_task create_index(core c, string folder_path)
    {
      doc_task res = new doc_task() { xml = "<root/>", created = true };
      string i_name = c.config.get_var("lib-vars.index-folder").value
        , fp = Path.Combine(folder_path, i_name);      
      res.save(fp);
      return res;
    }

    #region task attributes

    public string stato { get { return this.root_value("stato"); } set { if(this.stato != value) this.changed = true; this.set_root_attr("stato", value); } }
    public string priorita { get { return this.root_value("priorita"); } set { if(this.priorita != value) this.changed = true; this.set_root_attr("priorita", value); } }
    public string tipo { get { return this.root_value("tipo"); } set { if(this.tipo != value) this.changed = true; this.set_root_attr("tipo", value); } }
    public string stima { get { return this.root_value("stima"); } set { if(this.stima != value) this.changed = true; this.set_root_attr("stima", value); } }
    public string user { get { return this.root_value("user"); } set { if(this.user != value) this.changed = true; this.set_root_attr("user", value); } }

    public DateTime? dt_create {
      get { string val = this.root_value("dt_create"); return val != "" ? DateTime.Parse(val) : (DateTime?)null; }
      set {
        if(this.dt_create.HasValue != value.HasValue || (this.dt_create.HasValue && value.HasValue && this.dt_create != value)) this.changed = true;
        this.set_root_attr("dt_create", value.HasValue ? value.Value.ToString("yyyy/MM/dd HH:mm:ss") : "");
      }
    }

    public DateTime? dt_upd {
      get { string val = this.root_value("dt_upd"); return val != "" ? DateTime.Parse(val) : (DateTime?)null; }
      set {
        if(this.dt_upd.HasValue != value.HasValue || (this.dt_upd.HasValue && value.HasValue && this.dt_upd != value)) this.changed = true;
        this.set_root_attr("dt_upd", value.HasValue ? value.Value.ToString("yyyy/MM/dd HH:mm:ss") : "");
      }
    }

    #endregion
  }
}
