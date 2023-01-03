using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml;
using System.Text;
using System.IO;
using dn_lib.db;
using dn_lib.tools;
using dn_lib.xml;

public partial class _default : tl_page {

  protected override void OnInit (EventArgs e) {
    base.OnInit(e);
  }

  protected override void OnLoad (EventArgs e) {
    base.OnLoad(e);
    this.master.elab_cmd("attivita");
  }

  protected override void OnUnload (EventArgs e) {
    base.OnUnload(e);
  }

}