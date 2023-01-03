using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

public class task_filter
{
  public int id { get; set; }
  public string title { get; set; }
  public string notes { get; set; }
  public string def { get; set; }
  public string class_css { get; set; }

	public task_filter(int id, string title, string notes, string def, string class_css) {
    this.id = id; this.title = title; this.notes = notes; this.def = def; this.class_css = class_css;
  }
}