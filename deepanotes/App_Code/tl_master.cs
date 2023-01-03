using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using dn_lib.tools;
using deepanotes;

public class tl_master : System.Web.UI.MasterPage
{

  public tl_master()
  {
  }

  protected override void OnInit(EventArgs e)
  {
    base.OnInit(e);
  }

  public string cmd { get; set; }

  protected tl_page tlp { get { return ((tl_page)Parent); } }

  protected config config { get { return tlp.core.config; } }

  public virtual void redirect_to(string page) { Response.Redirect(page); }

  public virtual string get_val(string id) { return ""; }
  public virtual void set_val(string id, string val) { }

  public string client_key { get; set; }

  public string url_cmd(string cmd, string page = "", string[,] pars = null)
  {
    string upars = "";
    if(pars != null) {
      for(int i = 0; i < pars.GetLength(0); i++)
        upars += "&" + pars[i, 0] + "=" + HttpUtility.UrlEncode(pars[i, 1]);
    }
    return (page == "" ? config.get_var("lib-vars.router-page").value : page) + "?cmd=" + HttpUtility.UrlEncode(cmd) + upars;
  }

  public virtual void set_status_txt(string text) { }

  public void elab_cmd(string cmd) { Response.Redirect(url_cmd(cmd, config.get_var("lib-vars.router-page").value)); }
  public virtual cmd check_cmd(string cmd) { return null; }

  public bool is_mobile()
  {
    return strings.contains_any(Request.ServerVariables["HTTP_USER_AGENT"], new[] { "iPhone", "iPod", "iPad", "Android", "BlackBerry" });
  }

  public void status_txt(string txt)
  {
    tlp.ClientScript.RegisterStartupScript(tlp.GetType(), "__status_txt"
      , "status_txt_ms(\"" + txt.Replace("\"", "'") + "\");", true);
  }

  public void err_txt(string txt)
  {
    tlp.ClientScript.RegisterStartupScript(tlp.GetType(), "__err_txt"
      , "err_txt(\"" + txt.Replace("\"", "'").Replace("\r", " ").Replace("\n", " ").Replace("\\", "/") + "\");", true);
  }
}