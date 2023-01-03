using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace deepanotes {
  public class cmd {
    string _cmd, _group, _code, _sub_code, _type, _page; List<string> _keys; List<string> _subcmds;
    public cmd(string txt) { _cmd = txt; _type = ""; _page = ""; _keys = parse_cmds(_cmd); _subcmds = new List<string>(); }

    public string txt { get { return _cmd; } }
    public string group { get { return _group; } set { _group = value; } }
    public string code { get { return _code; } set { _code = value; } }
    public string sub_code { get { return _sub_code; } set { _sub_code = value; } }
    public string type { get { return _type; } set { _type = value; } }
    public string page { get { return _page; } set { _page = value; } }
    public string action { get { return _keys.Count > 0 ? _keys[0] : null; } set { if(_keys.Count > 0) _keys[0] = value; } }
    public string obj { get { return _keys.Count > 1 ? _keys[1] : null; } set { if (_keys.Count > 1) _keys[1] = value; } }
    public string sub_obj (int i = 0) {
      string res = _keys.Count > 2 && 2 + i < _keys.Count ? _keys[2 + i] : "";
      return is_subcmd(res) ? "" : res;
    }
    protected bool is_subcmd (string txt) {
      return _subcmds.FirstOrDefault(x => txt.Length >= x.Length && txt.Substring(0, x.Length) == x) != null;
    }
    public string sub_cmd (string sub_cmd, string def = "") {
      sub_cmd += ":";
      string res = _keys.FirstOrDefault(x => x.Length >= sub_cmd.Length && x.Substring(0, sub_cmd.Length) == sub_cmd);
      return res != null ? res.Substring(sub_cmd.Length) : def;
    }
    public void set_sub_cmds (string cmds) {
      foreach (string s in cmds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
        _subcmds.Add(s + ":");
    }

    protected List<string> parse_cmds (string line_cmd) {
      List<string> cmds = new List<string>();
      int start = 0;
      while (true) {
        if (line_cmd.Length <= start) break;
        string first_c = line_cmd.Substring(start, 1);
        start = first_c == "'" || first_c == "\"" ? start + 1 : start;
        int to = first_c == "'" || first_c == "\"" ? line_cmd.IndexOf(first_c, start) : line_cmd.IndexOf(" ", start);
        string val = first_c == "'" || first_c == "\"" ? (to < 0 ? "" : line_cmd.Substring(start, to - start))
          : (to >= 0 ? line_cmd.Substring(start, to - start) : line_cmd.Substring(start));
        if (val.Length > 0) cmds.Add(val);
        if (to < 0) break;
        start = first_c == "'" || first_c == "\"" ? to + 2 : to + 1;
      }
      return cmds;
    }

  }
}
