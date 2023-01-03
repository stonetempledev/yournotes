using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using dn_lib.xml;

namespace dn_lib.tools {

  public class blocks {
    protected nano_doc _doc = new nano_doc("<root/>");

    public blocks() { }

    public nano_node add(string name) { return _doc.root.add((new nano_doc(string.Format("<{0}/>", name))).root); }

    public nano_node add(string name, string text) {
      return _doc.root.add((new nano_doc(string.Format("<{0}>{1}</{0}>", name, text))).root);
    }

    public nano_node add_xml(string xml) {
      nano_doc doc = new nano_doc(string.Format("<root>{0}</root>", xml));
      nano_node fn = null;
      foreach (nano_node nd in doc.root.nodes) { _doc.root.add(nd); if (fn == null) fn = nd; }
      return fn;
    }

    public void reset() { _doc.load_xml("<root/>"); }

    public string parse_blocks(core cr) {
      StringBuilder sb = new StringBuilder();
      foreach (nano_node nd in _doc.root.nodes) parse_block(sb, cr, nd, cr.mobile);
      return sb.ToString();
    }

    protected void parse_block(StringBuilder sb, core cr, nano_node nd, bool? mobile) {
      string html = cr.config.get_html_block(nd.name).content;

      // childs
      StringBuilder sbc = new StringBuilder();
      if (html.IndexOf("{@childs@}") >= 0) {
        foreach (nano_node nd2 in nd.nodes) parse_block(sbc, cr, nd2, mobile);
        html = html.Replace("{@childs@}", sbc.Length > 0 ? sbc.ToString() : "");
      }

      // attrs
      int n = html.IndexOf("{@");
      while (n >= 0) {
        int n2 = html.IndexOf("@}", n + 2); if (n2 >= 0) {
          string attr = html.Substring(n + 2, n2 - n - 2), name = "", content = "";
          int nif = attr.IndexOf("::");
          if (nif > 0) { content = attr.Substring(nif + 2); name = attr.Substring(0, nif); } else name = attr;

          string val = nd.get_attr(name, name == "text");
          html = html.Replace("{@" + attr + "@}", content != "" ? (val != "" ? content.Replace("#value#", val) : "") : val);
        }
        n = html.IndexOf("{@", n + 2);
      }

      sb.Append(html);
    }
  }
}
