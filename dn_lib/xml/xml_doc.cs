using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Xml;
using System.Linq;

namespace dn_lib.xml {

  // xml_doc
  public class xml_doc : IDisposable {
    bool disposed = false;
    protected XmlDocument _doc = null;
    public XmlDocument doc { get { load(); return _doc; } }

    protected string _path = "";
    public string path { get { return _path; } }

    public bool loaded { get { return _doc != null; } }

    public xml_doc () { }
    public xml_doc (string path, bool force_load = true) { _path = path; if (force_load) load(); }
    public xml_doc (XmlDocument doc) { _doc = doc; }
    public xml_doc (XmlDocument doc, string path) { _doc = doc; _path = path; }
    public xml_doc (System.IO.Stream str) { _doc = new XmlDocument(); _doc.Load(str); }

    ~xml_doc () { Dispose(false); }

    public void Dispose () { Dispose(true); GC.SuppressFinalize(this); }

    protected virtual void Dispose (bool disposing) { if (disposed) return; if (disposing) { } disposed = true; }

    public void load_xml (string xml) { if (_doc == null) _doc = new XmlDocument(); _doc.LoadXml(xml); }

    public string xml { get { load(); return _doc.OuterXml; } set { load_xml(value); } }

    public virtual void save (string path = "") { if (_doc == null) return; if (!string.IsNullOrEmpty(path)) _path = path; _doc.Save(_path); }

    protected void load () { if (_doc != null) return; _doc = new XmlDocument(); _doc.Load(_path); }

    public static xml_doc doc_from_xml (string xml) { xml_doc doc = new xml_doc(); doc.load_xml(xml); return doc; }

    public static string xml_from_path (string path) { XmlDocument doc = new XmlDocument(); doc.Load(path); return doc.OuterXml; }

    #region nodes

    public xml_node root_node { get { load(); return new xml_node(_doc.DocumentElement); } }

    public bool exists (string xpath, string attr = "") {
      load();
      return _doc.SelectSingleNode(xpath) == null
        || (attr != "" && _doc.SelectSingleNode(xpath).Attributes[attr] == null) ? false : true;
    }

    public void set_attr (string xpath, string attr, string value, bool optional = true) { load(); xml_node.set_attr(_doc, xpath, attr, value, optional); }

    public void set_root_attr (string attr, string value, bool optional = true) { load(); xml_node.set_attr(_doc, "/*", attr, value, optional); }

    public string root_value (string attr, string defValue = "") { load(); return xml_node.node_val(root_node.node, attr, defValue); }

    public bool root_bool (string attr, bool defValue = false) { load(); return xml_node.node_bool(root_node.node, attr, defValue); }

    public int root_int (string attr, int defValue = -1) { load(); return xml_node.node_int(root_node.node, attr, defValue); }

    public string get_value (string xpath, string attr = "", string defValue = "") { return get_value2(xpath, attr, defValue); }

    public List<string> get_values (string xpath, string separator, string attr = "", string defValue = "") {
      string val = get_value2(xpath, attr, defValue);
      return string.IsNullOrEmpty(val) ? new List<string>()
        : val.Split(new string[] { separator }, StringSplitOptions.RemoveEmptyEntries).ToList();
    }

    protected string get_value2 (string xpath, string attr = "", string def = "") {
      load(); return xml_node.node_val(_doc.SelectSingleNode(attr == "" ? xpath : xpath + "[@" + attr + "]"), attr, def);
    }

    public int get_int (string xpath, string attr = "", int def = 0) {
      load(); return xml_node.node_int(select_node(attr == "" ? xpath : xpath + "[@" + attr + "]"), attr, def);
    }

    public bool get_bool (string xpath, string attr = "", bool def = false) {
      load(); return xml_node.node_bool(select_node(attr == "" ? xpath : xpath + "[@" + attr + "]"), attr, def);
    }

    public bool remove (string xpath) { XmlNode nd = select_node(xpath); if (nd == null) return false; nd.ParentNode.RemoveChild(nd); return true; }

    public xml_node add_node (string xpath, string name, string text = "") {
      xml_node nd = node(xpath); if (nd != null && !string.IsNullOrEmpty(text)) nd.text = text;
      return nd == null ? null : nd.add_node(name);
    }

    public xml_node add_after (string xpath, xml_node node_add, string xpath_after) {
      xml_node nd = node(xpath); return nd == null ? null : nd.add_node(node_add, node(xpath_after));
    }

    public xml_node add_before(string xpath, xml_node node_add, string xpath_before) {
      xml_node nd = node(xpath); return nd == null ? null : nd.add_node(node_add, null, node(xpath_before));
    }

    public xml_node add_node (string xpath, xml_node node_add) { xml_node nd = node(xpath); return nd == null ? null : nd.add_node(node_add); }

    public xml_node add_node (string name) { xml_node nd = root_node; return nd == null ? null : nd.add_node(name); }

    public xml_node add_xml (string xpath, string xml) { xml_node nd = node(xpath); return nd == null ? null : nd.add_xml(xml); }

    public void add_xml_nodes (string xpath, string xml) { xml_node.add_xml_nodes(node(xpath).node, xml); }

    public xml_node root_add (string name) {
      XmlNode nd = root_node.node;
      return nd == null ? null : new xml_node(nd.AppendChild(nd.OwnerDocument.CreateElement(name)));
    }

    public xml_node node (string xpath, bool create = false) { return new xml_node(select_node(xpath, create)); }

    public XmlNode select_node (string xpath, bool create = false) {
      load();

      XmlNode result = _doc.SelectSingleNode(xpath);
      if (result == null && create) {
        string[] xparts = xml_doc.xparts(xpath);
        XmlNode parent = null; string tmpPath = "";
        foreach (string xpart in xparts) {
          string namePart = xml_doc.name_element(xpart);
          tmpPath += "/" + namePart;
          result = _doc.SelectSingleNode(tmpPath);
          if (result == null) {
            if (parent == null) {
              _doc.LoadXml("<" + namePart + "/>"); result = _doc.DocumentElement;
            } else result = parent.AppendChild(_doc.CreateElement(namePart));
          }
          parent = result;
        }
      }

      return result;
    }

    public int count_nodes (string xpath) { return select_nodes(xpath).Count; }

    public List<xml_node> nodes (string xpath) { load(); return _doc.SelectNodes(xpath).Cast<XmlNode>().Select(n => new xml_node(n)).ToList(); }

    public XmlNodeList select_nodes (string xpath) { load(); return _doc.SelectNodes(xpath); }

    #endregion

    #region xpath

    static public string[] xparts (string xpath) {
      if (xpath.Substring(0, 1) != "/") throw new Exception("il percorso '" + xpath + "' non può essere valutato");
      return xpath.Substring(1).Split('/');
    }

    static public string name_element (string xpathPart) {
      string name = xpathPart;
      if (name.IndexOf('[') > 0) name = name.Substring(0, name.IndexOf('['));
      return name;
    }

    #endregion
  }
}
