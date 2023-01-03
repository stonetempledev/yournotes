using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;

namespace dn_lib.db
{
  public class meta_link
  {
    public enum types_link { normal, list };

    string _table_link, _table, _title, _field; 
    types_link _type;
    bool _basic;
    XmlNode _node;

    public meta_link(string table_link, string table, string title, string field, string type, bool basic, XmlNode node)
    {
      _table_link = table_link; _table = table; _title = title;
      _field = field;
      _type = (types_link)Enum.Parse(typeof(types_link), string.IsNullOrEmpty(type) ? "normal" : type); 
      _basic = basic;
      _node = node;
    }

    public string table_link { get { return _table_link; } }
    public string table { get { return _table; } }
    public string title { get { return _title; } }
    public string field { get { return _field; } }
    public types_link type { get { return _type; } }
    public string type_str { get { return _type.ToString(); } }
    public bool basic { get { return _basic; } }

    public string attr(string name) { return _node.Attributes[name] != null ? _node.Attributes[name].Value : ""; }
  }
}