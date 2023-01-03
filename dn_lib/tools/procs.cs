using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace dn_lib.tools {
  public class procs {
    public static string cmd_proc(string cmd, string argv) {

      Process p = new Process();
      p.StartInfo.UseShellExecute = false;
      p.StartInfo.RedirectStandardOutput = true;
      p.StartInfo.FileName = cmd;
      p.StartInfo.Arguments = argv;
      p.StartInfo.CreateNoWindow = true;
      p.Start();
      p.WaitForExit();
      string output = p.StandardOutput.ReadToEnd();
      p.Dispose();

      return output;

    }
  }
}
