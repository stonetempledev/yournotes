using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using log4net.Appender;
using log4net.Repository;

//Here is the once-per-application setup information
[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace dn_lib.tools
{
  public class log
  {
    static readonly log4net.ILog _log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    public static string log_info(string text) { _log.Info(text); return text; }
    public static string log_debug(string text) { _log.Debug(text); return text; }
    public static string log_err(string text) { _log.Error(text); return text; }
    public static string log_err(Exception ex) { _log.Error("error", ex); return ex.Message; }
    public static string log_err_sql(Exception ex, string sql) { _log.Error("sql: " + sql, ex); return ex.Message; }
    public static string log_sql(string text) { _log.Debug("SQL: " + text); return text; }
    public static string log_warning(string text) { _log.Warn(text); return text; }
    public static string dir_path() { string fn = file_name(); return string.IsNullOrEmpty(fn) ? "" : Path.GetDirectoryName(fn); }
    public static string file_name()
    {
      foreach(IAppender appender in _log.Logger.Repository.GetAppenders()) {
        Type t = appender.GetType();
        if(t.Equals(typeof(FileAppender)) || t.Equals(typeof(RollingFileAppender)))
          return ((FileAppender)appender).File;
      }
      return "";
    }
  }
}
