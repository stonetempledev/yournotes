using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dn_client
{
  public class client_cmd
  {
    public string function { get; set; }
    public Dictionary<string, string> pars { get; protected set; }

    public string par(string name) { return pars[name]; }
    public int par_int(string name) { return int.Parse(par(name)); }

    public client_cmd(string cmd)
    {
      string[] pars = cmd.Split(new char[] { '#' });
      this.function = pars[0];
      this.pars = new Dictionary<string, string>();
      if(pars.Length > 1) {
        for(int p = 1; p < pars.Length; p++) {
          string[] par = pars[p].Split(new char[] { ':' });
          this.pars.Add(par[0], par[1]);
        }
      }
    }
  }
}
