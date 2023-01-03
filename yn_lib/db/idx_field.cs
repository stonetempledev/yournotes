using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace dn_lib.db
{
  public class idx_field
  {
    protected string _name = "";
    protected bool _ascending = false;
    protected int _index = 0;

    public idx_field(string name, bool ascending, int index) {
      _name = name;
      _ascending = ascending;
      _index = index;
    }

    public idx_field(string name, bool ascending)
      : this(name, ascending, 0) { }

    public string name { get { return _name; } }
    public bool ascending { get { return _ascending; } }
    public int index { get { return _index; } }
  }
}