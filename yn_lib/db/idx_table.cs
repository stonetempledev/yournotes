using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace dn_lib.db
{
  public class idx_table
  {
    protected string _table_name = "", _name = "";
    protected bool _clustered = false, _unique = false, _primary = false;
    protected List<idx_field> _fields = null;

    public idx_table(string table_name, string name, bool clustered, bool unique, bool primary) {
      _table_name = table_name; _name = name;
      _clustered = clustered; _unique = unique; _primary = primary;
      _fields = new List<idx_field>();
    }

    public idx_table(string table_name, string name, bool clustered, bool unique, bool primary, IEnumerable<idx_field> lst)
      : this(table_name, name, clustered, unique, primary) {
      _fields.AddRange(lst);
    }

    public string table_name { get { return _table_name; } set { _table_name = value; } }
    public string name { get { return _name; } set { _name = value; } }
    public bool clustered { get { return _clustered; } }
    public bool unique { get { return _unique; } }
    public bool primary { get { return _primary; } }
    public List<idx_field> fields { get { return _fields; } }
    public idx_field exist_field(string field_name) { return _fields.FirstOrDefault(x => string.Compare(x.name, field_name, true) == 0); }
    public string list_fields() { return string.Join(",", _fields.Select(x => x.name)); }
    public void add_field(idx_field idx_fld) { _fields.Add(idx_fld); }

    static public idx_table find(List<idx_table> list, string idx_name, string tbl_name) {
      return list.FirstOrDefault(x => string.Compare(x.name, idx_name, false) == 0
        && string.Compare(x.table_name, tbl_name, false) == 0);
    }

    static public idx_table find_index(List<idx_table> list, idx_table idx) { return list.FirstOrDefault(x => same_index(x, idx)); }

    static public bool same_index(idx_table idx, idx_table idx2) {
      if (idx.clustered != idx2.clustered || idx.unique != idx2.unique || idx.primary != idx2.primary)
        return false;

      foreach (idx_field field in idx.fields)
        if (idx2.exist_field(field.name) == null)
          return false;

      return true;
    }

    public string des_index() {
      return name + " - " + (unique ? "unique" : "no unique")
        + (clustered ? ", clustered" : ", no clustered")
        + (primary ? ", primary" : "") + "(" + string.Join(", ", fields.Select(x => x.name)) + ")";
    }

    static public bool filter_unique(bool? uniques, bool unique, bool primary) {
      return !uniques.HasValue || (uniques.HasValue && (uniques.Value && unique && !primary) || (!uniques.Value && !unique));
    }
  }
}

