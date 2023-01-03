using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace deepanotes
{
  public class html_blocks : bo
  {
    public html_blocks() {
    }

    #region section

    public string section_title(string title, string des) {
      return core.parse_html_block("blocks.section-title"
          , new string[,] { { "title", title }, { "des", des } });
    }

    #endregion

    #region list

    public string open_list() { return core.parse_html_block("blocks.open-list"); }
    public string list_item(string title, string sub_title = "", string href = "", string[] sub_items = null) {
      return core.parse_html_block("blocks.list-item"
        , new string[,] { { "title", title }, { "sub-title", sub_title }, { "ref", href != "" ? href : "#" }
          , {"sub-items", sub_items != null ? string.Join("", sub_items.Select(x => 
              x != "" ? core.parse_html_block("blocks.list-sub-item", new string[,]   { { "html", x } }) : "")) : "" } });
    }
    public string close_list() { return core.parse_html_block("blocks.close-list"); }

    #endregion
  }

}