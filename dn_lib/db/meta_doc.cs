using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using dn_lib.tools;
using dn_lib.xml;

namespace dn_lib.db
{
  public class meta_doc {
    public enum table_type { none, data, temporanea, sistema }
    public enum col_type { primary, diretta, linked, service, info }

    protected xml_doc _doc = null;
    public xml_doc doc { get { return _doc; } }
    public bool loaded { get { return _doc.loaded; } }
    public string path { get { return _doc.path; } }

    protected schema_doc _schema = null;

    public meta_doc(string path, schema_doc sch) { _doc = new xml_doc(path); _schema = sch; }
    public meta_doc(XmlDocument doc, schema_doc sch) { _doc = new xml_doc(doc); _schema = sch; }

    public void save(string path) { _doc.doc.Save(path); }

    public string ver { get { return _doc.root_value("ver"); } }

    // valori opzionali - per gestire tutti i tipi di db
    public string prefix_sys () { return tables_attr("prefix-sys"); }
    public string prefix_tmp () { return tables_attr("prefix-tmp"); }
    public string field_ins () { return tables_attr("field-ins"); }
    public string field_upd () { return tables_attr("field-upd"); }
    
    protected string tables_attr(string attr_name) { return _doc.exists("/root/tables", attr_name) ? _doc.get_value("/root/tables", attr_name) : null; }

    public bool sys_table (string table) { return _doc.exists("/root/tables", "prefix-sys") && strings.like(table, _doc.get_value("/root/tables", "prefix_sys") + "*"); }

    public string title_col(string col) { return _doc.get_value("/root/fields/field[@name-upper='" + col.ToUpper() + "']", "title", col); }

    public string function_ids(string table, string field) {
      return _doc.get_value("/root/tables/table[@name-upper='" + table.ToUpper() + "']", "fnc");
    }

    public Dictionary<string, Dictionary<string, string>> table_cols(string table, List<col_type> types, bool or = true) {
      // fields
      List<string> ifields = _doc.get_value("/root/tables/table[@name-upper='" + table.ToUpper() + "']"
          , "fields-info").ToLower().Split(',').ToList<string>();

      // ciclo colonne
      Dictionary<string, Dictionary<string, string>> result = new Dictionary<string, Dictionary<string, string>>();
      foreach (schema_field fld in _schema.table_fields(table)) {
        Dictionary<string, string> attrs = new Dictionary<string, string>() { { "indice", (result.Count + 1).ToString() }
                    , { "type", fld.original_type }, { "primary", fld.primary.ToString().ToLower() }, { "linked", "" }
                    , { "linkedtable", "" }, { "linkedtype", "" }, { "service", "" }
                    , { "info", ifields.Contains(fld.name.ToLower()).ToString().ToLower() }};
        attrs["service"] = (_doc.get_value("/root/tables", "field-ins").ToUpper() == fld.name.ToUpper()
            || _doc.get_value("/root/tables", "field-upd").ToUpper() == fld.name.ToUpper()).ToString().ToLower();
        attrs["linked"] = _doc.exists("/root/tables/table[@name-upper='" + table.ToUpper() + "']/links/link[@field-upper='" + fld.name.ToUpper() + "']").ToString().ToLower();
        attrs["linkedtable"] = _doc.get_value("/root/tables/table[@name-upper='" + table.ToUpper() + "']/links/link[@field-upper='" + fld.name.ToUpper() + "']", "table-upper");
        attrs["linkedtype"] = _doc.get_value("/root/tables/table[@name-upper='" + table.ToUpper() + "']/links/link[@field-upper='" + fld.name.ToUpper() + "']", "type");
        attrs["diretta"] = (!bool.Parse(attrs["primary"]) && !bool.Parse(attrs["linked"]) && !bool.Parse(attrs["service"])).ToString();

        bool add = or && types != null && types.Count > 0 ? false : true;
        foreach (col_type tp in types)
          if (or && attrs[tp.ToString()] != null && bool.Parse(attrs[tp.ToString()])) { add = true; break; } 
          else if (!or && attrs[tp.ToString()] != null && !bool.Parse(attrs[tp.ToString()])) { add = false; break; }

        if (add) result.Add(fld.name, attrs);
      }

      return result;
    }

    public table_type type_table(string table) { string table_ref = ""; return type_table(table, out table_ref); }

    public table_type type_table(string table, out string table_ref) {
      table_type res = is_table(table, "prefix-tmp") ? table_type.temporanea
          : is_table(table, "prefix-sys") ? table_type.sistema : table_type.data;

      table_ref = res == table_type.temporanea ? table.Substring(_doc.get_value("/root/tables", "prefix-tmp").Length)
        : res == table_type.sistema ? table.Substring(_doc.get_value("/root/tables", "prefix-sys").Length) : "";

      return res;
    }

    protected bool is_table(string table, string attr_prefix) { return _doc.exists("/root/tables", attr_prefix) && strings.like(table, _doc.get_value("/root/tables", attr_prefix) + "*"); }

    protected meta_link link(XmlNode node) {
      return new meta_link(node.ParentNode.ParentNode.Attributes["name-upper"].Value, node.Attributes["table-upper"].Value
        , node.ParentNode.ParentNode.Attributes["title"].Value, node.Attributes["field-upper"].Value, xml_node.node_val(node, "type")
        , xml_node.node_bool(node, "basic"), node);
    }

    public bool idlist_field_link(string table, string field) {
      return links_table(table).FirstOrDefault(y => y.field.ToLower() == field.ToLower() && y.table_link == "list") != null;
    }

    public meta_table.align_codes table_align_code(string table) {
      return meta_table.get_align_code(_doc.get_value("/root/tables/table[@name-upper='" + table.ToUpper() + "']", "align_code"));
    }

    public string table_title(string table) { return _doc.get_value("/root/tables/table[@name-upper='" + table.ToUpper() + "']", "title"); }

    public string table_from_code(meta_table.align_codes rec_code) { return _doc.get_value("/root/tables/table[@align_code='" + rec_code.ToString() + "']", "name-upper"); }

    public IEnumerable<meta_link> table_links(string table) { return _doc.nodes("/root/tables/table/links//link[@table-upper='" + table.ToUpper() + "']").Cast<XmlNode>().Select(x => link(x)); }

    public IEnumerable<meta_link> links_table(string table) { return _doc.nodes("/root/tables/table[@name-upper='" + table.ToUpper() + "']/links/link").Cast<XmlNode>().Select(x => link(x)); }

    public meta_table meta_tbl(string table) { return table_from_node(table, _doc.select_node("/root/tables/table[@name-upper='" + table.ToUpper() + "']")); }

    public bool enum_tbl(string table) { return _doc.get_bool("/root/tables/table[@name-upper='" + table.ToUpper() + "']", "enum"); }

    public bool export_tbl(string table) { return !_doc.get_bool("/root/tables/table[@name-upper='" + table.ToUpper() + "']", "exclude_export"); }

    protected meta_table table_from_node(string table, XmlNode node) {
      return node != null ? new meta_table(xml_node.node_val(node, "name-upper"), xml_node.node_val(node, "title")
          , xml_node.node_val(node, "single"), xml_node.node_val(node, "fields-info")
          , type_table(xml_node.node_val(node, "name-upper")), node
          , node != null ? node.SelectNodes("rules/nochar").Cast<XmlNode>().Select(x => new meta_rule(x.Name, x.Attributes["field-upper"].Value, x.Attributes["value"].Value)) : null
          , node != null ? node.SelectNodes("links/link").Cast<XmlNode>().Select(x => link(x)) : null, xml_node.node_val(node, "align_code"), xml_node.node_bool(node, "enum")) :
          new meta_table(table, table, table, "", type_table(table), node);
    }

    public idx_table idx_unique(string table) {
      return _schema.table_indexes(table, true).FirstOrDefault(
        x => x.fields.Count > 1 || (x.fields.Count == 1 && field_ins() != null && x.fields[0].name.ToLower() != field_ins().ToLower())
        || (x.fields.Count == 1 && field_ins() == null));
    }

    public idx_table idx_unique_onins(string table) {
      return _schema.table_indexes(table, true).FirstOrDefault(
        x => x.fields.Count == 1 && field_ins() != null && x.fields[0].name.ToLower() == field_ins().ToLower());
    }
  }
}