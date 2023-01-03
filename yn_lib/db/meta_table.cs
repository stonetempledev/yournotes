using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;

namespace dn_lib.db
{
  public class meta_table
  {
    public enum align_codes { none, folders, files };
    public enum align_types { none, level };

    string _name, _title, _single, _fields_info;
    List<meta_rule> _rules = null;
    List<meta_link> _links = null;
    meta_doc.table_type _type = meta_doc.table_type.none;
    align_codes _align_code;
    XmlNode _node = null;
    bool _enum = false;

    public meta_table(string name, string title, string single, string fields_info, meta_doc.table_type type
      , XmlNode nd, IEnumerable<meta_rule> rules = null, IEnumerable<meta_link> links = null, string rec_code = "", bool is_enum = false) {
      _name = name; _title = title; _single = single; _type = type;      
      _fields_info = fields_info; _node = nd;
      _rules = rules != null ? new List<meta_rule>(rules) : new List<meta_rule>();
      _links = links != null ? new List<meta_link>(links) : new List<meta_link>();
      _align_code = rec_code != "" ? (align_codes)Enum.Parse(typeof(align_codes), rec_code) : align_codes.none;
      _enum = is_enum;
    }

    static public align_codes get_align_code(System.Xml.XmlAttribute xml_attr) {
      return get_align_code(xml_attr != null ? xml_attr.Value : "");
    }

    static public align_codes get_align_code(string code) {
      return !string.IsNullOrEmpty(code) ? (meta_table.align_codes)Enum.Parse(typeof(meta_table.align_codes), code)
        : db.meta_table.align_codes.none;
    }

    static public bool recursive_link(align_codes parent_tbl, align_codes tbl) { return parent_tbl == align_codes.folders && tbl == align_codes.folders; }
    static public bool recursive_link(meta_table parent_tbl, meta_table tbl) {
      return recursive_link(parent_tbl != null ? parent_tbl.align_code : align_codes.none
        , tbl != null ? tbl.align_code : align_codes.none);
    }

    public string name { get { return _name; } }
    public meta_doc.table_type type { get { return _type; } }
    public string title { get { return _title; } }
    public string single { get { return _single; } }
    public string fields_info { get { return _fields_info; } }    
    public List<meta_rule> rules { get { return _rules; } }
    public List<meta_link> links { get { return _links; } }
    public meta_link find_link(string field) { return _links.FirstOrDefault(x => x.field.ToLower() == field.ToLower()); }
    public align_codes align_code { get { return _align_code; } }
    public XmlNode node { get { return _node; } }
    public List<string> flds_info() { return _fields_info.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList(); }
    public string attr(string name_attr) { return dn_lib.xml.xml_node.node_val(_node, name_attr); }
    public string first_fld_info() { return string.IsNullOrEmpty(_fields_info) ? "" : _fields_info.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)[0]; }
  }
}