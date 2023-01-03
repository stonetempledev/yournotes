using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dn_lib
{
  public class synch_results
  {
    protected string _err;

    public int folders { get; set; }
    public int files { get; set; }
    public int deleted { get; set; }
    public int seconds { get; set; }
    public string err { get { return _err; } set { _err = !string.IsNullOrEmpty(_err) ? _err : value; } }
    public DateTime lwt { get; set; }
    public bool scan { get; set; }

    public synch_results() { }
  }
}

