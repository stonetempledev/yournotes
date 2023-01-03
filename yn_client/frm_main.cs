using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Net;
using System.Threading.Tasks;
using dn_lib;
using dn_lib.db;
using dn_lib.tools;
using dn_lib.xml;

namespace dn_client
{
  public partial class frm_main : Form
  {
    public static core _c = null;
    protected Point _dp = Point.Empty;
    protected settings _settings = null;
    protected string _client_key = "";
    protected int _interval_ss = 60;
    protected int _max_lines_log = 10000, _min_lines_log = 3000, _threads = 3;
    protected RichTextBox _cache_log;

    public frm_main(core c)
    {
      _c = c; InitializeComponent();
    }

    private void lbl_title_MouseDown(object sender, MouseEventArgs e)
    {
      if(e.Button != MouseButtons.Left) return; _dp = new Point(e.X, e.Y);
    }

    private void lbl_title_MouseMove(object sender, MouseEventArgs e)
    {
      if(_dp == Point.Empty) return;
      Point location = new Point(this.Left + e.X - _dp.X, this.Top + e.Y - _dp.Y);
      this.Location = location;
    }

    private void lbl_title_MouseUp(object sender, MouseEventArgs e)
    {
      if(e.Button != MouseButtons.Left) return;
      _dp = Point.Empty;
    }

    private void frm_main_Resize(object sender, EventArgs e)
    {
      if(this.WindowState == FormWindowState.Minimized) {
        Hide(); ntf_main.Visible = true;
      }
    }

    private void ntf_main_DoubleClick(object sender, EventArgs e)
    {
      System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("explorer", "\"" + _settings.get_value("url") + "?ck=" + _client_key + "\"") {
        RedirectStandardOutput = true, UseShellExecute = false, CreateNoWindow = true
      });
    }

    private void frm_main_Load(object sender, EventArgs e)
    {
      try {
        tmr_state.Interval = _interval_ss * 1000;
        db_provider conn = Program.open_conn();
        client_opened(conn, true);
        _settings = settings.read_settings(_c, conn);

        this.WindowState = FormWindowState.Minimized;
        this.ShowInTaskbar = false;

        // log
        this._cache_log = new RichTextBox();
        this._cache_log.BackColor = Color.Black;
        this._cache_log.BorderStyle = BorderStyle.None;
        this._cache_log.Font = new Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this._cache_log.ForeColor = Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));

        // tray
        ntf_main.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
        ntf_main.ContextMenuStrip.Items.Add("Apri...", null, this.MenuApri_Click);
        ntf_main.ContextMenuStrip.Items.Add("Pannello...", null, this.MenuPannello_Click);
        ntf_main.ContextMenuStrip.Items.Add("Esci", null, this.MenuExit_Click);
        ntf_main.ContextMenuStrip.BackColor = Color.Gray;
        ntf_main.ContextMenuStrip.ForeColor = Color.White;
        ntf_main.ContextMenuStrip.Font = new Font("segoe ui light", 9, FontStyle.Regular);

      } catch(Exception ex) { log_err(ex.Message); }
    }

    protected void log_err(string txt) { log_txt(txt, Color.Tomato); }

    protected void log_txt(string txt, Color? clr = null)
    {
      if(String.IsNullOrEmpty(txt)) return;

      RichTextBox rtb = chk_passo.Checked ? rt_main : _cache_log;
      lock(rtb) {
        try {
          rtb.SelectionStart = rtb.TextLength;
          rtb.SelectionLength = 0;
          if(clr.HasValue) rtb.SelectionColor = clr.Value;
          rtb.AppendText(DateTime.Now.ToString("HH:mm:ss") + " - " + txt + Environment.NewLine);
          rtb.SelectionColor = rtb.ForeColor;
          if(rtb.Lines.Length > _max_lines_log) {
            int start_index = rtb.GetFirstCharIndexFromLine(0);
            int count = rtb.GetFirstCharIndexFromLine((rtb.Lines.Count() - _min_lines_log) + 1) - start_index;
            rtb.Text = rtb.Text.Remove(start_index, count);
          }
          if(chk_passo.Checked) rtb.ScrollToCaret();
        } catch { }
      }
    }

    void MenuPannello_Click(object sender, EventArgs e)
    {
      Show();
      this.WindowState = FormWindowState.Normal;
      ntf_main.Visible = false;
    }

    void MenuApri_Click(object sender, EventArgs e)
    {
      ntf_main_DoubleClick(null, null);
    }

    void MenuExit_Click(object sender, EventArgs e)
    {
      Application.Exit();
    }

    private void lbl_title_DoubleClick(object sender, EventArgs e)
    {
      if(this.WindowState == FormWindowState.Normal) this.WindowState = FormWindowState.Maximized;
      else if(this.WindowState == FormWindowState.Maximized) this.WindowState = FormWindowState.Normal;
    }

    protected void msg_error(string txt)
    {
      MessageBox.Show(txt, "Attenzione!", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    private void mi_exit_Click(object sender, EventArgs e)
    {
      MenuExit_Click(sender, e);
    }

    private void tmr_state_Tick(object sender, EventArgs e) { client_opened(); }

    protected void client_opened(db_provider conn = null, bool first = false)
    {
      try {
        bool close = false;
        if(conn == null) { conn = Program.open_conn(); close = true; }

        if(_client_key == "") _client_key = strings.random_hex(20);

        conn.exec(_c.parse_query("lib-base.client-refresh", new string[,] { { "client_key", _client_key }, { "ip_machine", sys.machine_ip() }
            , { "first", first ? "1" : "0" }, { "machine_name", sys.machine_name() }, { "interval_ss", _interval_ss.ToString() } }));
        if(first) log_txt($"connected client key '{_client_key}'", Color.Azure);

        if(close) { conn.close_conn(); conn = null; }
      } catch { }
    }

    protected void close_client()
    {
      Program.open_conn().exec(_c.parse_query("lib-base.client-close", new string[,] { { "client_key", _client_key } }));
      log_txt($"close client '{_client_key}'", Color.Azure);
    }

    public void open_att(int file_id, int user_id, string user_name)
    {
      Task.Factory.StartNew(() => {

        // download file...
        string fp = "";
        try {
          string folder = Program._c.config.get_var("client.client-tmp-path").value;
          if(!Directory.Exists(folder)) Directory.CreateDirectory(folder);
          fi i = fi.load_fi(file_id);
          lbl_message(log.log_info($"download file {i.http_path}"));
          fp = Path.Combine(folder, i.file_name_local);
          using(WebClient webClient = new WebClient()) {
            webClient.DownloadFile(i.http_path, fp);
          }
          xml_att idoc = xml_att.open(); idoc.set_file(i, DateTime.Now, user_id, user_name); idoc.save();
          lbl_message(log.log_info($"downloaded file {i.http_path}"));
        } catch(Exception ex) {
          log.log_err(ex);
          lbl_message($"download file error: {ex.Message}");
        }

        // apri il file
        try {
          lbl_message(log.log_info($"open file {fp}"));
          System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("explorer", "\"" + fp + "\"") {
            RedirectStandardOutput = true, UseShellExecute = false, CreateNoWindow = true
          });
          lbl_message();
        } catch(Exception ex) {
          log.log_err(ex);
          lbl_message($"open file error: {ex.Message}");
        }

      });
    }

    protected bool _tmr_att = false;
    private void tmr_att_Tick(object sender, EventArgs e)
    {
      try {
        if(_tmr_att) return;
        _tmr_att = true;

        check_upload_att();
      } catch(Exception ex) { log.log_err(ex); }
      _tmr_att = false;
    }

    protected void check_upload_att()
    {
      string folder = Program._c.config.get_var("client.client-tmp-path").value
        , iname = Program._c.config.get_var("client.index-att").value;
      if(!Directory.Exists(folder)) Directory.CreateDirectory(folder);
      xml_att idoc = xml_att.open();

      // enum files & check to upload
      bool savei = false;
      foreach(FileInfo i in Directory.EnumerateFiles(folder).Select(x => new FileInfo(x))) {
        if(i.Name.ToLower() == iname) continue;
        try {
          string fn = i.Name.ToLower(); int id_file = int.Parse(fn.Split(new char[] { '_' })[0]);

          // add file
          if(!idoc.exists_file(id_file, out xml_node n)) {
            n = idoc.add_file(fi.load_fi(id_file), i.LastWriteTime, n.get_int("user_id"), n.get_val("user_name"));
            savei = true;
            continue;
          }

          // check upload
          DateTime lwt = DateTime.Parse(n.get_attr("lwt"));
          if(lwt.ToString("yyyy-MM-dd HH:mm:ss") != i.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss")) {
            savei = true;
            upload_file(i.FullName, id_file, n);
            n.set_attr("lwt", i.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss"));
            continue;
          }

          // check delete
          if((DateTime.Now - lwt).TotalHours > 12) {
            n.remove();
            i.Delete();
            savei = true;
            continue;
          }

        } catch(Exception ex) { log.log_err(ex); }
      }

      // del file
      foreach(fi f in idoc.files()) {
        if(!File.Exists(Path.Combine(folder, f.file_name_local))) {
          idoc.del_file(f.id_file);
          savei = true;
        }
      }

      if(savei) idoc.save();
    }

    protected void upload_file(string path, int id_file, xml_node n)
    {
      try {
        lbl_message(log.log_info($"upload file {n.get_attr("http_path")}"));
        System.Text.Encoding e = encoding_type.GetType(path);
        var file = new {
          action = "save_file", id = id_file, bin_data = e.GetString(File.ReadAllBytes(path)), enc = e.HeaderName
          , user_id = n.get_int("user_id"), user_name = n.get_val("user_name")
        };
        json_request.post(_c.base_url + _c.config.get_var("client.io-page").value, file);
        lbl_message(log.log_info($"uploaded file {n.get_attr("http_path")}!"), 2);
      } catch(Exception ex) {
        log.log_err(ex);
        lbl_message($"uploaded error {ex.Message}!", 5, true);
        n.set_attr("upload_err", $"error at {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}: {ex.Message}"); throw ex;
      }
    }

    private void frm_main_FormClosing(object sender, FormClosingEventArgs e)
    {
      try {
        check_upload_att();
      } catch(Exception ex) { }
    }

    protected void lbl_message(string txt = "", int? seconds = null, bool error = false)
    {
      if(ss_label.Tag != null && ss_label.Tag.ToString() == "delayed") return;

      if(!error) log_txt(txt); else log_err(txt);

      if(error) {
        ss_label.BackColor = Color.Tomato;
        ss_main.BackColor = Color.Tomato;
        ss_label.ForeColor = Color.White;
      }
      ss_label.Text = txt;
      Application.DoEvents();

      if(seconds.HasValue) {
        ss_label.Tag = "delayed";
        Task.Factory.StartNew(() => {
          System.Threading.Thread.Sleep(seconds.Value * 1000);
          reset_label();
        });
      }
    }

    protected void reset_label()
    {
      try {
        ss_label.BackColor = Color.White;
        ss_main.BackColor = Color.White;
        ss_label.ForeColor = Color.CornflowerBlue;
        ss_label.Text = "";
        ss_label.Tag = null;
      } catch { }
    }

    // synch folders    
    //protected bool _synch = false;
    //private void tmr_synch_Tick(object sender, EventArgs e)
    //{
    //  try {
    //    if(_synch) return;
    //    _synch = true;
    //    lbl_message(log.log_info($"synch folders..."));
    //    var a = new { action = "synch_folders", user_id = -1, user_name = "client" };
    //    json_result res = json_request.post(_c.base_url + _c.config.get_var("client.io-page").value, a);
    //    lbl_message(log.log_info($"synch folders!"), 2);
    //  } catch(Exception ex) {
    //    log.log_err(ex);
    //    lbl_message($"synch folders error {ex.Message}!", 5, true);
    //  } finally { _synch = false; }
    //}

    private void frm_main_FormClosed(object sender, FormClosedEventArgs e)
    {
      try { close_client(); } catch { }
    }

    bool _cmd = false;
    private void tmr_cmds_Tick(object sender, EventArgs e)
    {
      if(_cmd) return;
      _cmd = true;
      db_provider conn = null;
      try {
        conn = Program.open_conn();
        DataRow r = conn.first_row(_c.parse_query("lib-base.client-cmd-to-elab", new string[,] { { "client_key", _client_key } }));
        if(r != null) {
          int id_cmd = db_provider.int_val(r["id_client_cmd"]);
          string cmd = db_provider.str_val(r["cmd"]);
          log_txt($"elab command '{cmd}'");
          conn.exec(_c.parse_query("lib-base.client-remove-cmd", new string[,] { { "id_cmd", id_cmd.ToString() } }));

          client_cmd cc = new client_cmd(cmd);
          if(cc.function == "open_att")
            open_att(cc.par_int("file_id"), cc.par_int("user_id"), cc.par("user_name"));
        }

      } catch(Exception ex) { log_err(ex.Message); } finally { if(conn != null) conn.close_conn(); }
      _cmd = false;
    }

    private void pb_close_Click(object sender, EventArgs e) { this.WindowState = FormWindowState.Minimized; }

    private void chk_passo_CheckedChanged(object sender, EventArgs e)
    {
      try {
        if(chk_passo.Checked) {
          _cache_log.SelectAll();
          rt_main.SelectionStart = rt_main.TextLength;
          rt_main.SelectionLength = 0;
          rt_main.SelectedRtf = _cache_log.SelectedRtf;
          _cache_log.Clear();
        }
      } catch { }
    }
  }
}

