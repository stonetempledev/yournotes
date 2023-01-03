using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.IO;
using dn_lib.db;

namespace dn_client
{
  public class fi
  {
    public int id_file { get; set; }
    public string http_path { get; set; }
    public string server_path { get; set; }
    public string file_name { get; set; }
    public string file_name_local { get; set; }
    public string extension { get; set; }
    public DateTime? lwt { get; set; }

    public fi() { }

    public static fi load_fi(int id_file)
    {
      DataRow r = Program.first_row("client.file-infos", new string[,] { { "file_id", id_file.ToString() } });
      return r != null ? new fi() { id_file = id_file,
        http_path = db_provider.str_val(r["http_path"]), server_path = db_provider.str_val(r["server_path"])
        , file_name = db_provider.str_val(r["file_name"]), extension = db_provider.str_val(r["extension"])
        , file_name_local = id_file.ToString() + "_" + db_provider.str_val(r["file_name"]) + db_provider.str_val(r["extension"])
      } : null;
    }
  }
}
