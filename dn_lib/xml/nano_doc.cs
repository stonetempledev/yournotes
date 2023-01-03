using System;
using System.Linq;
using System.Collections.Generic;

namespace dn_lib.xml {
  public class nano_base {
    protected static bool _is_space (char c) { return c == ' ' || c == '\t' || c == '\n' || c == '\r'; }

    protected static void _skip_spaces (string str, ref int i) {
      while (i < str.Length) {
        if (!_is_space(str[i])) {
          if (str[i] == '<' && i + 4 < str.Length && str[i + 1] == '!' && str[i + 2] == '-' && str[i + 3] == '-') {
            i += 4; // skip <!--
            while (i + 2 < str.Length && !(str[i] == '-' && str[i + 1] == '-')) i++;
            i += 2; // skip --
          } else break;
        }

        i++;
      }
    }

    protected static string _val (string str, ref int i, char end_c, char end_c2, bool stop_on_space) {
      int start = i;
      while ((!stop_on_space || !_is_space(str[i])) && str[i] != end_c && str[i] != end_c2) i++;
      return str.Substring(start, i - start);
    }

    protected static bool _is_quote (char c) { return c == '"' || c == '\''; }

    protected static string _parse_attrs (string str, ref int i, List<nano_attr> attrs, char end_c, char end_c2) {
      _skip_spaces(str, ref i);
      string name = _val(str, ref i, end_c, end_c2, true);

      _skip_spaces(str, ref i);

      while (str[i] != end_c && str[i] != end_c2) {
        string attrName = _val(str, ref i, '=', '\0', true);

        _skip_spaces(str, ref i);
        i++; // skip '='
        _skip_spaces(str, ref i);

        char quote = str[i];
        if (!_is_quote(quote)) throw new XMLParsingException("Unexpected token after " + attrName);

        i++; // skip quote
        string attrValue = _val(str, ref i, quote, '\0', false);
        i++; // skip quote

        attrs.Add(new nano_attr(attrName, attrValue));

        _skip_spaces(str, ref i);
      }

      return name;
    }
  }

  public class nano_doc : nano_base {
    private nano_node _root;
    private List<nano_attr> _decs = new List<nano_attr>();

    public nano_doc (string xml) { load_xml(xml); }

    public void load_xml (string xml) {
      int i = 0;

      _decs.Clear();

      while (true) {
        _skip_spaces(xml, ref i);

        if (xml[i] != '<') throw new XMLParsingException("Unexpected token");

        i++; // skip <

        if (xml[i] == '?') // declaration
        {
          i++; // skip ?
          _parse_attrs(xml, ref i, _decs, '?', '>');
          i++; // skip ending ?
          i++; // skip ending >

          continue;
        }

        if (xml[i] == '!') // doctype
        {
          while (xml[i] != '>') // skip doctype
            i++;

          i++; // skip >

          continue;
        }

        _root = new nano_node(xml, ref i);
        break;
      }
    }

    public nano_node root { get { return _root; } }

    public IEnumerable<nano_attr> decs { get { return _decs; } }

    public static nano_node create_node(string xml) { nano_doc d = new nano_doc(xml); return d.root; }
  }

  public class nano_node : nano_base {
    private string _value, _name;

    private List<nano_node> _nodes = new List<nano_node>();
    private List<nano_attr> _attrs = new List<nano_attr>();

    public nano_node (string name) { _name = name; }

    internal nano_node (string str, ref int i) {
      _name = _parse_attrs(str, ref i, _attrs, '>', '/');

      if (str[i] == '/') // if this node has nothing inside
      {
        i++; // skip /
        i++; // skip >
        return;
      }

      i++; // skip >

      // temporary. to include all whitespaces into value, if any
      int tempI = i;

      _skip_spaces(str, ref tempI);

      if (str[tempI] == '<') {
        i = tempI;

        while (str[i + 1] != '/') // parse subnodes
        {
          i++; // skip <
          _nodes.Add(new nano_node(str, ref i));

          _skip_spaces(str, ref i);

          if (i >= str.Length) return; // EOF
          if (str[i] != '<') throw new XMLParsingException("Unexpected token");
        }

        i++; // skip <
      } else // parse value
      {
        _value = _val(str, ref i, '<', '\0', false);
        i++; // skip <

        if (str[i] != '/') throw new XMLParsingException("Invalid ending on tag " + _name);
      }

      i++; // skip /
      _skip_spaces(str, ref i);

      string endName = _val(str, ref i, '>', '\0', true);
      if (endName != _name) throw new XMLParsingException("Start/end tag name mismatch: " + _name + " and " + endName);
      _skip_spaces(str, ref i);

      if (str[i] != '>') throw new XMLParsingException("Invalid ending on tag " + _name);

      i++; // skip >
    }

    public string value { get { return _value; } }
    public string name { get { return _name; } }

    public IEnumerable<nano_node> nodes { get { return _nodes; } }
    public IEnumerable<nano_attr> attrs { get { return _attrs; } }

    protected nano_attr find_attr (string name) { return _attrs.FirstOrDefault(a => a.name == name); }
    public nano_attr attr (string name) { return find_attr(name); }
    public string get_attr (string name, bool also_val = false) { nano_attr a = find_attr(name); return a != null ? a.value : (also_val ? _value : ""); }
    public nano_node add (string name) { nano_node nd = new nano_node(name); _nodes.Add(nd); return nd; }
    public nano_node add (nano_node nd) { _nodes.Add(nd); return nd; }
    public void add_attr (string name, string val) {
      nano_attr attr = find_attr(name);
      if (attr == null) _attrs.Add(new nano_attr(name, value));
      else attr.value = val;
    }
    public nano_node add_xml (string xml) {
      nano_doc doc = new nano_doc(string.Format("<root>{0}</root>", xml));
      nano_node fn = null;
      foreach (nano_node nd in doc.root.nodes) { add(nd); if (fn == null) fn = nd; }
      return fn;
    }

  }

  public class nano_attr {
    private string _name, _value;

    public string name { get { return _name; } }
    public string value { get { return _value; } set { _value = value; } }

    internal nano_attr (string name, string value) { _name = name; _value = value; }
  }

  public class XMLParsingException : Exception { public XMLParsingException (string message) : base(message) { } }
}