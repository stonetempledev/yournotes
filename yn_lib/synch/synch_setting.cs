using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using dn_lib.db;

namespace dn_lib {
  public class synch_setting {

    public string name { get; set; }
    public string des { get; set; }
    public string value { get; set; }

    public synch_setting(string name, string des, string value) {
      this.name = name; this.des = des; this.value = value; 
    }

  }
}