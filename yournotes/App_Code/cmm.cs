using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Net;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using dn_lib.tools;

/// <summary>
/// Summary description for cmm
/// </summary>
public class cmm {
  public cmm() {
  }
  public static void dump_object(object obj, string path) {
    File.WriteAllText(path, JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented
      , new JsonSerializerSettings() {
        ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
      }));
  }
}