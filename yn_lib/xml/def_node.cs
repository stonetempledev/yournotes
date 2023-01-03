using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dn_lib.xml {
  public class def_node {
    protected string _name, _text;
    protected Dictionary<string, string> _attrs = null;

    public string name { get { return _name; } set { _name = value; } }
    public string text { get { return _text; } set { _text = value; } }
    public Dictionary<string, string> attrs { get { return _attrs; } }

    public def_node (string name, string text) { _name = name; _text = text; _attrs = new Dictionary<string, string>(); }
    public def_node (string name, Dictionary<string, string> attrs) { _name = name; _attrs = attrs; }
    public def_node (string name, string text, Dictionary<string, string> attrs) { _name = name; _text = text; _attrs = attrs; }
    public def_node (string name, string text, string[] attrs) {
      _name = name; _text = text; _attrs = new Dictionary<string, string>(); set_attrs(attrs);
    }
    public def_node (string name, string[] attrs) {
      _name = name; _text = ""; _attrs = new Dictionary<string, string>(); set_attrs(attrs);
    }

    public void set_attrs (string[] attrs) {
      foreach (string attr in attrs) {
        int n = attr.IndexOf(':'); _attrs.Add(n >= 0 ? attr.Substring(0, n) : attr, n >= 0 ? attr.Substring(n + 1) : "");
      }
    }
  }
}
