using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace dn_lib.tools
{
  public class json
  {
    public static string serialize(object obj)
    {
      return JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented
        , new JsonSerializerSettings() {
          ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
        });
    }
  }
}
