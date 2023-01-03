using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace dn_lib {
  public class task_priorita {

    public string priorita { get; set; }
    public int order { get; set; }
    public string cls { get; set; }
    public string title_plurale { get; set; }
    public string title_singolare { get; set; }

    public task_priorita(string priorita, int order, string cls, string title_plurale, string title_singolare) {
      this.priorita = priorita; this.order = order; this.cls = cls;
      this.title_plurale = title_plurale; this.title_singolare = title_singolare;
    }

  }
}
