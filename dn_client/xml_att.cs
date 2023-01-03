using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dn_lib.xml;
using System.IO;

namespace dn_client
{
  public class xml_att : xml_doc
  {
    public xml_att(string path) : base(path) { }

    public static xml_att open()
    {
      string folder = Program._c.config.get_var("client.client-tmp-path").value
        , iname = Program._c.config.get_var("client.index-att").value, i = Path.Combine(folder, iname);
      if(!File.Exists(i)) File.WriteAllText(i, "<root/>");
      return new xml_att(i);
    }

    public bool exists_file(int id_file)
    {
      return this.node($"/root/file[@id='{id_file}']").node != null;
    }

    public bool exists_file(int id_file, out xml_node n)
    {
      n = this.node($"/root/file[@id='{id_file}']"); return n.node != null;
    }

    public List<fi> files()
    {
      return nodes("/root/file").Cast<xml_node>().Select(x => new fi() {
        id_file = x.get_int("id"), file_name = x.get_attr("name"), file_name_local = x.get_attr("name_local")
      , http_path = x.get_attr("http_path"), lwt = DateTime.Parse(x.get_attr("lwt"))
      }).ToList();
    }

    public void del_file(int id) { node($"/root/file[@id={id}]").remove(); }

    public xml_node set_file(fi i, DateTime lwt, int user_id, string user_name) { del_file(i.id_file); return add_file(i, lwt, user_id, user_name); }

    public xml_node add_file(fi i, DateTime lwt, int user_id, string user_name)
    {
      return root_node.add_node("file", new Dictionary<string, string>() { { "id", i.id_file.ToString() }
        , { "user_id", user_id.ToString() }, { "user_name", user_name }
        , { "name", i.file_name }, { "name_local", i.file_name_local }, { "http_path", i.http_path }
        , { "lwt", lwt.ToString("yyyy-MM-dd HH:mm:ss") } });
    }

  }
}
