using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace dn_lib {
  public class task_stima {

    public string stima { get; set; }
    public float days { get; set; }
    public string cls { get; set; }
    public string title_plurale { get; set; }
    public string title_singolare { get; set; }

    public task_stima(string stima, float days, string cls, string title_plurale, string title_singolare) {
      this.stima = stima; this.days = days; this.cls = cls;
      this.title_plurale = title_plurale; this.title_singolare = title_singolare;
    }

  }
}
