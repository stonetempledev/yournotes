using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace dn_lib.tools {

  public class strings {

    static public bool is_int(string s) { try { Int32.Parse(s); } catch { return false; } return true; }

    static public bool is_float(string s) { try { float.Parse(s); } catch { return false; } return true; }

    static public int parse_int(string s, int def = 0) { int result = def; try { result = Int32.Parse(s); } catch { result = def; } return result; }

    static public string file_name(string path_file, bool with_ext = true) {
      try {
        path_file = path_file.Replace("\\", "/");
        if (path_file.LastIndexOf("/") > 0)
          path_file = path_file.Substring(path_file.LastIndexOf("/") + 1, path_file.Length - path_file.LastIndexOf("/") - 1);
        return !with_ext && path_file.LastIndexOf(".") > 0 ? path_file.Substring(0, path_file.LastIndexOf(".")) : path_file;
      } catch (Exception e) { string msg = e.Message; return ""; }
    }

    static public Boolean is_absolute_url(string url) {
      string url2 = url.Replace("\\", "/");
      return ((url2.Length >= 2 && url2.Substring(1, 1).ToLower() == ":")
          || (url2.Length >= 2 && url2.Substring(0, 2).ToLower() == "//")
          || (url2.Length >= 7 && url2.Substring(0, 7).ToLower() == "http://")
          || (url2.Length >= 8 && url2.Substring(0, 8).ToLower() == "https://")
          || (url2.Length >= 7 && url2.Substring(0, 7).ToLower() == "file://")
          || (url2.Length >= 6 && url2.Substring(0, 6).ToLower() == "ftp://")
          || (url2.Length >= 7 && url2.Substring(0, 7).ToLower() == "sftp://")) ? true : false;
    }

    static public string combine_url(string url, string parsurl) { return parsurl != "" ? url + (url.IndexOf("?") > 0 ? "&" : "?") + parsurl : url; }

    static public string combine_url(string url, List<string> parsurl) {
      foreach (string pars in parsurl) url = combine_url(url, pars);
      return url;
    }

    static public bool like(string str, string wildcard) {
      return new Regex("^" + Regex.Escape(wildcard).Replace(@"\*", ".*").Replace(@"\?", ".") + "$",
          RegexOptions.IgnoreCase | RegexOptions.Singleline).IsMatch(str);
    }

    static public Regex reg_like(string wildcard) {
      return new Regex("^" + Regex.Escape(wildcard).Replace(@"\*", ".*").Replace(@"\?", ".") + "$",
          RegexOptions.IgnoreCase | RegexOptions.Singleline);
    }

    static public bool find_string(List<string> values, string value) {
      foreach (string tmp in values) { if (tmp.ToUpper() == value.ToUpper()) return true; }
      return false;
    }

    static public bool contain_tag(string tags, string tag) {
      foreach (string item in tags.Split(',')) if (item.Trim().ToLower() == tag.ToLower()) return true;
      return false;
    }

    static public bool is_alpha(string val) { return (new Regex(@"^[a-zA-Z0-9]*$")).IsMatch(val); }

    static public string rel_path(string base_path, string path_file) {
      if (!is_absolute_url(base_path) || !is_absolute_url(path_file)) throw new Exception("entrambi i percorsi devono essere assoluti!");

      return (path_file.Length >= path_file.Length && base_path.ToLower().Replace("/", "\\") == path_file.Substring(0, base_path.Length).ToLower().Replace("/", "\\") ?
        path_file.Substring(base_path.Length, path_file.Length - base_path.Length) : path_file);
    }

    static public Dictionary<string, string> get_args(string args, char sep_args, char sep_val) {
      Dictionary<string, string> res = new Dictionary<string, string>();
      foreach (string arg in args.Split(new char[] { sep_args }, StringSplitOptions.RemoveEmptyEntries)) {
        string name = arg.Split(new char[] { sep_val }, StringSplitOptions.RemoveEmptyEntries)[0]
          , val = arg.Split(new char[] { sep_val }, StringSplitOptions.RemoveEmptyEntries)[1];
        res.Add(name, val);
      }
      return res;
    }

    protected static Random _rnd = new Random();
    public static string random_hex(int digits) {
      byte[] buffer = new byte[digits / 2];
      _rnd.NextBytes(buffer);
      string result = String.Concat(buffer.Select(x => x.ToString("X2")).ToArray());
      if (digits % 2 == 0)
        return result;
      return result + _rnd.Next(16).ToString("X");
    }

    public static bool contains_any(string haystack, params string[] needles) {
      if(string.IsNullOrEmpty(haystack)) return false;
      foreach (string needle in needles) {
        if (haystack.Contains(needle))
          return true;
      }
      return false;
    }

    public static string remove_specials(string str) {
      return Regex.Replace(str, "[^a-zA-Z0-9_.]+", "", RegexOptions.Compiled);
    }
  }

}



