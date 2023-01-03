using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

public class element_cut
{
  public enum element_cut_type { task, folder, attachment }

  public int id { get; set; }
  public element_cut_type tp { get; set; }
  public bool copy { get; set; }

  public element_cut(int id, element_cut_type tp, bool copy = false)
  {
    this.id = id; this.tp = tp; this.copy = copy;
  }
}