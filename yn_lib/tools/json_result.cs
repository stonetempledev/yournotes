using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace dn_lib.tools
{
  /// <summary>
  /// Summary description for json_result
  /// </summary>
  public class json_result
  {
    public enum type_result { none = 0, ok = 1, error = -1 };
    public type_result result { get; set; }
    public string des_result { get { return this.result.ToString(); } }
    public string message { get; set; }
    public string contents { get; set; }
    public string doc_xml { get; set; }
    public string menu_html { get; set; }
    public string html_element { get; set; }
    public string url_file { get; set; }
    public string url_name { get; set; }
    public Dictionary<string, string> vars { get; set; }
    public List<string> list { get; set; }
    public void set_var(string name, string val) { if(this.vars.ContainsKey(name)) this.vars[name] = val; else this.vars.Add(name, val); }
    public object data { get; set; }

    public json_result(type_result tr, string message = "", string contents = "")
    {
      this.result = tr; this.message = message; this.contents = contents;
      this.vars = new Dictionary<string, string>();
      this.list = new List<string>();
    }

    public static json_result from_json(string json)
    {
      return Newtonsoft.Json.JsonConvert.DeserializeObject<json_result>(json);
    }
  }
}