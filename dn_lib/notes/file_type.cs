using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dn_lib
{
  public class file_type
  {
    public enum ft_type_content { source, info }

    public string extension { get; set; }
    public string des_extension { get; set; }
    public string open_comment { get; set; }
    public string close_comment { get; set; }
    public ft_type_content type_content { get; set; }

    public file_type(string extension, string des_extension, string open_comment, string close_comment, ft_type_content tp)
    {
      this.extension = extension;
      this.des_extension = des_extension;
      this.open_comment = open_comment;
      this.close_comment = close_comment;
      this.type_content = tp;
    }
  }
}
