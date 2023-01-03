using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dn_lib
{
  public class file_info
  {
    public enum fi_type { notes }

    public string file_name { get; set; }
    public fi_type type_info { get; set; }

    public file_info(string file_name, fi_type tp) { this.file_name = file_name; this.type_info = tp; }
  }
}
