using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace dn_lib {
  public class free_label {

    public string free_txt { get; set; }
    public string stato { get; set; }
    public string priorita { get; set; }
    public string tipo { get; set; }
    public string stima { get; set; }
    public bool def { get; set; }

    public free_label(string free_txt, string stato, string priorita, string tipo, string stima, bool def) {
      this.free_txt = free_txt; this.stato = stato;
      this.priorita = priorita; this.tipo = tipo; this.stima = stima; this.def = def;
    }

  }
}
