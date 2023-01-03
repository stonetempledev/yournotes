using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Net;
using Newtonsoft.Json.Linq;

namespace dn_lib.tools
{
  /// <summary>
  /// Summary description for json_request
  /// </summary>
  public class json_request
  {
    JObject _jo = null;

    // base function
    public bool has_action { get { return _jo != null; } }
    public byte[] val_bytes(string code, System.Text.Encoding e)
    {
      if(!_jo.ContainsKey(code)) throw new Exception("il parametro '" + code + "' per la request non è specificato!");
      return e.GetBytes(_jo[code].ToString());
    }
    public string val_str(string code)
    {
      if(!_jo.ContainsKey(code)) throw new Exception("il parametro '" + code + "' per la request non è specificato!");
      return _jo[code].ToString();
    }
    public int val_int(string code, int def = 0) { return !string.IsNullOrEmpty(val_str(code)) ? int.Parse(val_str(code)) : def; }
    public int? val_int_null(string code) { return !string.IsNullOrEmpty(val_str(code)) ? int.Parse(val_str(code)) : (int?)null; }
    public long val_long(string code) { return !string.IsNullOrEmpty(val_str(code)) ? long.Parse(val_str(code)) : 0; }
    public bool val_bool(string code)
    {
      string str = val_str(code);
      return !string.IsNullOrEmpty(str) && (str == "1" || str.ToUpper() == "TRUE" || str.ToUpper() == "VERO" || str.ToUpper() == "SI");
    }
    public JArray val_array(string code) { return _jo[code] as JArray; }

    // shortcut
    public string action { get { return val_str("action"); } }
    public long id { get { return val_long("id"); } }

    public json_request(System.Web.UI.Page page)
    {
      string json = String.Empty;
      page.Request.InputStream.Position = 0;
      using(var inputStream = new StreamReader(page.Request.InputStream)) {
        json = inputStream.ReadToEnd();
      }
      if(string.IsNullOrEmpty(json)) throw new Exception("nessuna richiesta da elaborare!");

      _jo = JObject.Parse(json);
    }

    public static bool there_request(System.Web.UI.Page page) { return page.Request.Headers["dn-post"] != null; }

    public static bool there_file(System.Web.UI.Page page) { return page.Request.Headers["dn-post-file"] != null; }

    public static json_result post(string url, object obj)
    {
      HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
      req.ContentType = "application/json";
      req.Method = WebRequestMethods.Http.Post;
      req.AllowWriteStreamBuffering = true;
      req.Timeout = 100000;
      req.Headers.Add("dn-post", "true");
      string json_str = json.serialize(obj);
      req.ContentLength = json_str.Length;
      req.AllowWriteStreamBuffering = true;
      using(var dataStream = new StreamWriter(req.GetRequestStream()))
        dataStream.Write(json_str);

      json_result res = null;
      using(WebResponse response = req.GetResponse()) {
        Stream res_str = response.GetResponseStream();
        StreamReader read_str = new StreamReader(res_str);
        res = json_result.from_json(read_str.ReadToEnd());
      }

      if(res.result == json_result.type_result.error)
        throw new Exception(res.message);

      return res;
    }
  }
}