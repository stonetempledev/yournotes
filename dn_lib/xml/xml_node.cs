using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace dn_lib.xml {

  public class xml_node {
    XmlNode _node = null;
    public XmlNode node { get { return _node; } }
    public string name { get { return _node != null ? _node.Name : ""; } }

    public xml_node(XmlNode node) { _node = node; }

    public bool is_element { get { return _node != null && _node.NodeType == XmlNodeType.Element; } }

    public string text {
      get {
        XmlNode tc = txt_child;
        //return tc != null ? tc.Value : (_node != null ? _node.InnerText : "");
        return tc != null ? tc.Value : "";
      }
      set { _node.InnerText = value; }
    }

    public string inner_xml {
      get {
        XmlNode tc = txt_child;
        return tc != null ? tc.InnerXml : (_node != null ? _node.InnerXml : "");
      }
      set { _node.InnerXml = value; }
    }

    public string outer_xml {
      get {
        return _node != null ? _node.OuterXml : "";
      }
    }

    public string data {
      get {
        return this.text;
      }
      set {
        if (_node == null) return;
        XmlNode child = this.txt_child;
        if (child != null && child.InnerText != "") _node.RemoveChild(child);
        _node.AppendChild(_node.OwnerDocument.CreateCDataSection(value));
      }
    }

    protected XmlNode txt_child {
      get {
        if (_node == null) return null;
        foreach (XmlNode child in _node.ChildNodes) {
          if (child.NodeType == XmlNodeType.Text ||
              child.NodeType == XmlNodeType.CDATA) {
            return child;
          }
        }
        return null;
      }
    }

    // attrs
    public string[] get_attrs() {
      return _node == null || _node.Attributes == null ?
        new string[0] : _node.Attributes.Cast<XmlAttribute>().Select(a => a.Name).ToArray();
    }
    public string set_attr(string name, string value, bool optional = true) { set_attr(_node, name, value, optional); return value; }
    public string get_attr(string name, string def = "") { return get_attr(_node, name, def); }
    public xml_node set_attrs(Dictionary<string, string> attrs) { if (attrs != null) { foreach (KeyValuePair<string, string> attr in attrs) set_attr(_node, attr.Key, attr.Value); } return this; }
    public xml_node set_attrs(string[] attrs) {
      if (attrs != null) {
        foreach (string attr in attrs) {
          int n = attr.IndexOf(':'); set_attr(n >= 0 ? attr.Substring(0, n) : attr, n >= 0 ? attr.Substring(n + 1) : "");
        }
      }
      return this;
    }
    public xml_node set_attrs(string[,] attrs) {
      if (attrs != null) {
        bool name = true; string[] nv = new string[2];
        foreach (string attr in attrs) {
          nv[name ? 0 : 1] = attr;
          if (!name) set_attr(nv[0], nv[1]);
          name = !name;
        }
      }
      return this;
    }

    // nodes
    public xml_node add_node(string name) { return _node == null ? null : new xml_node(_node.AppendChild(_node.OwnerDocument.CreateElement(name))); }
    public void add_nodes(def_node[] nodes) {
      if (_node == null) return; foreach (def_node nd in nodes) add_node(nd.name, nd.text, nd.attrs);
    }
    public xml_node add_node(string name, string text) { xml_node nd = add_node(name); nd.text = text; return nd; }
    public xml_node add_node(string name, Dictionary<string, string> attrs) {
      xml_node nd = add_node(name); if (nd != null) nd.set_attrs(attrs); return nd;
    }
    public xml_node add_node(string name, string text, Dictionary<string, string> attrs) {
      xml_node nd = add_node(name); if (nd != null) {
        nd.text = !string.IsNullOrEmpty(text) ? text : ""; nd.set_attrs(attrs);
      }
      return nd;
    }
    public xml_node add_node(string name, string text, string[] attrs) {
      xml_node nd = add_node(name); if (nd != null) { nd.text = text; if (attrs != null) nd.set_attrs(attrs); } return nd;
    }
    public xml_node add_node(xml_node node_add, xml_node node_after = null, xml_node node_before = null) {
      return _node == null ? null : new xml_node(node_after != null ? _node.InsertAfter(node_add.node, node_after.node) :
        node_before != null ? _node.InsertBefore(node_add.node, node_before.node) : _node.AppendChild(node_add.node));
    }
    public xml_node add_after(xml_node node_add) {
      if (_node == null || _node.ParentNode == null) return null;
      return new xml_node(_node.ParentNode.InsertAfter(node_add.node, this.node));
    }

    public bool remove() {
      if (_node == null || _node.ParentNode == null) return false;
      _node.ParentNode.RemoveChild(_node);
      _node = null; return true;
    }

    public xml_node add_clone(xml_node node_to_clone) {
      return this.add_node(node_to_clone.clone(this));
    }
    public List<xml_node> add_clone_childs(xml_node node_to_clone) {
      List<xml_node> res = new List<xml_node>();
      foreach (xml_node nc in node_to_clone.childs)
        res.Add(this.add_node(nc.clone(this)));
      return res;
    }
    public xml_node add_xml(string xml) {
      return _node == null ? null : new xml_node(_node.AppendChild(_node.OwnerDocument.ImportNode(
          xml_doc.doc_from_xml(xml).root_node.node, true)));
    }
    public void add_xml_nodes(string xml) {
      if (_node != null) {
        xml_doc doc = xml_doc.doc_from_xml("<root>" + xml + "</root>");
        foreach (XmlNode nd in doc.select_nodes("/root/*"))
          _node.AppendChild(_node.OwnerDocument.ImportNode(nd, true));
      }
    }
    public bool exists_attr(string name) { return node == null || node.Attributes == null || node.Attributes[name] == null ? false : true; }
    public bool exists_node(string xpath) { return _node.SelectSingleNode(xpath) != null; }
    public xml_node sub_node(string xpath) { return new xml_node(_node.SelectSingleNode(xpath)); }
    public bool has_child() { return _node.ChildNodes.Count > 0; }

    public List<xml_node> nodes(string xpath) { return _node.SelectNodes(xpath).Cast<XmlNode>().Select(n => new xml_node(n)).ToList(); }
    public List<xml_node> childs { get { return _node.ChildNodes.Cast<XmlNode>().Select(n => new xml_node(n)).ToList(); } }

    // values
    public string get_val(string attr = "", string def = "") { return node_val(_node, attr, def); }
    public bool get_bool(string attr = "", bool def = false) { return node_bool(_node, attr, def); }
    public int get_int(string attr = "", int def = -1) { return node_int(_node, attr, def); }

    protected xml_node clone(xml_node ref_node) {
      xml_node res = new xml_node(ref_node._node.OwnerDocument.CreateElement(this.name));

      // attrs
      foreach (XmlAttribute a in this._node.Attributes)
        res.set_attr(a.Name, a.Value);

      // childs
      foreach (xml_node nc in this.childs)
        res._node.AppendChild(nc.clone(ref_node)._node);
      return res;
    }

    public List<xml_node> clone_childs(xml_node ref_node) {
      List<xml_node> res = new List<xml_node>();
      foreach (xml_node nc in this.childs)
        res.Add(nc.clone(this));
      return res;
    }


    #region globals

    // attrs
    public static string get_attr(XmlNode node, string name, string def = "") { return node == null || node.Attributes == null || node.Attributes[name] == null ? def : node.Attributes[name].Value; }
    public static XmlNode set_attrs(XmlNode node, Dictionary<string, string> attrs) { foreach (KeyValuePair<string, string> attr in attrs) set_attr(node, attr.Key, attr.Value); return node; }
    public static XmlNode set_attr(XmlDocument doc, string xpath, string attr, string value, bool optional = true) { if (doc == null) return null; return set_attr(doc.SelectSingleNode(xpath), attr, value, optional); }
    public static XmlNode set_attr(XmlNode node, string attr, string value, bool optional = true) {
      if (node == null || (node != null && string.IsNullOrEmpty(value) && optional && (node.Attributes == null || node.Attributes[attr] == null))) return node;
      else if (string.IsNullOrEmpty(value) && optional && node.Attributes != null && node.Attributes[attr] != null) {
        node.Attributes.RemoveNamedItem(attr);
        return node;
      }

      if (node.Attributes != null) {
        if (node.Attributes[attr] == null)
          node.Attributes.Append(node.OwnerDocument.CreateAttribute(attr));
        node.Attributes[attr].Value = value;
      }

      return node;
    }

    // nodes
    static public XmlNode add_node(XmlNode node, string name) { if (node == null) return null; return node.AppendChild(node.OwnerDocument.CreateElement(name)); }
    static public XmlNode add_node(XmlNode node, string name, Dictionary<string, string> attrs) { if (node == null) return null; return set_attrs(node.AppendChild(node.OwnerDocument.CreateElement(name)), attrs); }

    static public XmlNode add_node(XmlNode node, XmlNode node_add, XmlNode node_after = null, XmlNode node_before = null) {
      return node == null ? null : node_after != null ? node.InsertAfter(node_add, node_after) :
        node_before != null ? node.InsertBefore(node_add, node_before) : node.AppendChild(node_add);
    }

    static public XmlNode add_xml(XmlNode node, string xml) {
      return node == null ? null : node.AppendChild(node.OwnerDocument.ImportNode(
          xml_doc.doc_from_xml(xml).root_node.node, true));
    }

    static public void add_xml_nodes(XmlNode node, string xml) {
      if (node != null) {
        xml_doc doc = xml_doc.doc_from_xml("<root>" + xml + "</root>");
        foreach (XmlNode nd in doc.select_nodes("/root/*"))
          node.AppendChild(node.OwnerDocument.ImportNode(nd, true));
      }
    }

    // values
    static public string node_val(XmlNode node, string attr = "", string def = "") {
      return attr != "" ? ((node != null && node.Attributes != null && node.Attributes[attr] != null) ? node.Attributes[attr].Value : def)
          : (node != null ? node.InnerText : def);
    }

    static public bool node_bool(XmlNode node, string attr = "", bool def = false) {
      string val = node_val(node, attr, def.ToString());
      return !string.IsNullOrEmpty(val) ? bool.Parse(val) : def;
    }

    static public int node_int(XmlNode node, string attr = "", int def = -1) {
      string val = node_val(node, attr, def.ToString());
      return !string.IsNullOrEmpty(val) ? int.Parse(val) : def;
    }

    #endregion
  }
}
