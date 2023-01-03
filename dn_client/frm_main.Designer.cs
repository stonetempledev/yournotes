namespace dn_client {
  partial class frm_main {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing) {
      if (disposing && (components != null)) {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent() {
      this.components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frm_main));
      this.lbl_title = new System.Windows.Forms.Label();
      this.ntf_main = new System.Windows.Forms.NotifyIcon(this.components);
      this.tmr_state = new System.Windows.Forms.Timer(this.components);
      this.tmr_att = new System.Windows.Forms.Timer(this.components);
      this.ss_main = new System.Windows.Forms.StatusStrip();
      this.ss_label = new System.Windows.Forms.ToolStripStatusLabel();
      this.tmr_cmds = new System.Windows.Forms.Timer(this.components);
      this.tab_main = new System.Windows.Forms.TabControl();
      this.tab_eventi = new System.Windows.Forms.TabPage();
      this.chk_passo = new System.Windows.Forms.CheckBox();
      this.rt_main = new System.Windows.Forms.RichTextBox();
      this.tab_impostazioni = new System.Windows.Forms.TabPage();
      this.pb_close = new System.Windows.Forms.PictureBox();
      this.tab_comandi = new System.Windows.Forms.TabPage();
      this.ss_main.SuspendLayout();
      this.tab_main.SuspendLayout();
      this.tab_eventi.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pb_close)).BeginInit();
      this.SuspendLayout();
      // 
      // lbl_title
      // 
      this.lbl_title.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.lbl_title.BackColor = System.Drawing.Color.DimGray;
      this.lbl_title.Cursor = System.Windows.Forms.Cursors.Hand;
      this.lbl_title.Font = new System.Drawing.Font("Segoe UI Light", 16F);
      this.lbl_title.ForeColor = System.Drawing.Color.Tomato;
      this.lbl_title.Location = new System.Drawing.Point(0, 0);
      this.lbl_title.Name = "lbl_title";
      this.lbl_title.Size = new System.Drawing.Size(815, 33);
      this.lbl_title.TabIndex = 0;
      this.lbl_title.Text = "Deepa Notes Pannello";
      this.lbl_title.DoubleClick += new System.EventHandler(this.lbl_title_DoubleClick);
      this.lbl_title.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lbl_title_MouseDown);
      this.lbl_title.MouseMove += new System.Windows.Forms.MouseEventHandler(this.lbl_title_MouseMove);
      this.lbl_title.MouseUp += new System.Windows.Forms.MouseEventHandler(this.lbl_title_MouseUp);
      // 
      // ntf_main
      // 
      this.ntf_main.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
      this.ntf_main.BalloonTipText = "Apri il pannello di controllo";
      this.ntf_main.BalloonTipTitle = "Deepa Notes Client";
      this.ntf_main.Icon = ((System.Drawing.Icon)(resources.GetObject("ntf_main.Icon")));
      this.ntf_main.Text = "deepa notes client";
      this.ntf_main.DoubleClick += new System.EventHandler(this.ntf_main_DoubleClick);
      // 
      // tmr_state
      // 
      this.tmr_state.Enabled = true;
      this.tmr_state.Interval = 300000;
      this.tmr_state.Tick += new System.EventHandler(this.tmr_state_Tick);
      // 
      // tmr_att
      // 
      this.tmr_att.Enabled = true;
      this.tmr_att.Interval = 5000;
      this.tmr_att.Tick += new System.EventHandler(this.tmr_att_Tick);
      // 
      // ss_main
      // 
      this.ss_main.BackColor = System.Drawing.Color.White;
      this.ss_main.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ss_label});
      this.ss_main.Location = new System.Drawing.Point(0, 514);
      this.ss_main.Name = "ss_main";
      this.ss_main.Size = new System.Drawing.Size(813, 22);
      this.ss_main.TabIndex = 6;
      this.ss_main.Text = "statusStrip1";
      // 
      // ss_label
      // 
      this.ss_label.Font = new System.Drawing.Font("Segoe UI Light", 9F);
      this.ss_label.ForeColor = System.Drawing.Color.CornflowerBlue;
      this.ss_label.Name = "ss_label";
      this.ss_label.Size = new System.Drawing.Size(0, 17);
      // 
      // tmr_cmds
      // 
      this.tmr_cmds.Enabled = true;
      this.tmr_cmds.Interval = 1000;
      this.tmr_cmds.Tick += new System.EventHandler(this.tmr_cmds_Tick);
      // 
      // tab_main
      // 
      this.tab_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.tab_main.Controls.Add(this.tab_eventi);
      this.tab_main.Controls.Add(this.tab_impostazioni);
      this.tab_main.Controls.Add(this.tab_comandi);
      this.tab_main.Location = new System.Drawing.Point(4, 35);
      this.tab_main.Name = "tab_main";
      this.tab_main.SelectedIndex = 0;
      this.tab_main.Size = new System.Drawing.Size(807, 466);
      this.tab_main.TabIndex = 41;
      // 
      // tab_eventi
      // 
      this.tab_eventi.Controls.Add(this.chk_passo);
      this.tab_eventi.Controls.Add(this.rt_main);
      this.tab_eventi.Location = new System.Drawing.Point(4, 26);
      this.tab_eventi.Name = "tab_eventi";
      this.tab_eventi.Padding = new System.Windows.Forms.Padding(3);
      this.tab_eventi.Size = new System.Drawing.Size(799, 436);
      this.tab_eventi.TabIndex = 0;
      this.tab_eventi.Text = "Log Eventi";
      this.tab_eventi.UseVisualStyleBackColor = true;
      // 
      // chk_passo
      // 
      this.chk_passo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.chk_passo.AutoSize = true;
      this.chk_passo.Checked = true;
      this.chk_passo.CheckState = System.Windows.Forms.CheckState.Checked;
      this.chk_passo.Cursor = System.Windows.Forms.Cursors.Hand;
      this.chk_passo.ForeColor = System.Drawing.Color.Gray;
      this.chk_passo.Location = new System.Drawing.Point(698, 412);
      this.chk_passo.Name = "chk_passo";
      this.chk_passo.Size = new System.Drawing.Size(99, 23);
      this.chk_passo.TabIndex = 6;
      this.chk_passo.Text = "tieni il passo";
      this.chk_passo.UseVisualStyleBackColor = true;
      this.chk_passo.CheckedChanged += new System.EventHandler(this.chk_passo_CheckedChanged);
      // 
      // rt_main
      // 
      this.rt_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.rt_main.BackColor = System.Drawing.Color.Black;
      this.rt_main.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.rt_main.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.rt_main.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
      this.rt_main.HideSelection = false;
      this.rt_main.Location = new System.Drawing.Point(0, 2);
      this.rt_main.Name = "rt_main";
      this.rt_main.ReadOnly = true;
      this.rt_main.Size = new System.Drawing.Size(799, 408);
      this.rt_main.TabIndex = 4;
      this.rt_main.Text = "";
      // 
      // tab_impostazioni
      // 
      this.tab_impostazioni.Location = new System.Drawing.Point(4, 26);
      this.tab_impostazioni.Name = "tab_impostazioni";
      this.tab_impostazioni.Size = new System.Drawing.Size(799, 436);
      this.tab_impostazioni.TabIndex = 2;
      this.tab_impostazioni.Text = "Impostazioni";
      this.tab_impostazioni.UseVisualStyleBackColor = true;
      // 
      // pb_close
      // 
      this.pb_close.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.pb_close.BackColor = System.Drawing.Color.DimGray;
      this.pb_close.Cursor = System.Windows.Forms.Cursors.Hand;
      this.pb_close.Image = ((System.Drawing.Image)(resources.GetObject("pb_close.Image")));
      this.pb_close.Location = new System.Drawing.Point(783, 0);
      this.pb_close.Name = "pb_close";
      this.pb_close.Size = new System.Drawing.Size(32, 32);
      this.pb_close.TabIndex = 42;
      this.pb_close.TabStop = false;
      this.pb_close.Click += new System.EventHandler(this.pb_close_Click);
      // 
      // tab_comandi
      // 
      this.tab_comandi.Location = new System.Drawing.Point(4, 26);
      this.tab_comandi.Name = "tab_comandi";
      this.tab_comandi.Size = new System.Drawing.Size(799, 436);
      this.tab_comandi.TabIndex = 3;
      this.tab_comandi.Text = "Comandi";
      this.tab_comandi.UseVisualStyleBackColor = true;
      // 
      // frm_main
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.White;
      this.ClientSize = new System.Drawing.Size(813, 536);
      this.ControlBox = false;
      this.Controls.Add(this.pb_close);
      this.Controls.Add(this.tab_main);
      this.Controls.Add(this.ss_main);
      this.Controls.Add(this.lbl_title);
      this.Font = new System.Drawing.Font("Segoe UI Light", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "frm_main";
      this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frm_main_FormClosing);
      this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frm_main_FormClosed);
      this.Load += new System.EventHandler(this.frm_main_Load);
      this.Resize += new System.EventHandler(this.frm_main_Resize);
      this.ss_main.ResumeLayout(false);
      this.ss_main.PerformLayout();
      this.tab_main.ResumeLayout(false);
      this.tab_eventi.ResumeLayout(false);
      this.tab_eventi.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pb_close)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label lbl_title;
    private System.Windows.Forms.NotifyIcon ntf_main;
    private System.Windows.Forms.Timer tmr_state;
    private System.Windows.Forms.Timer tmr_att;
    private System.Windows.Forms.StatusStrip ss_main;
    private System.Windows.Forms.ToolStripStatusLabel ss_label;
    private System.Windows.Forms.Timer tmr_cmds;
    private System.Windows.Forms.TabControl tab_main;
    private System.Windows.Forms.TabPage tab_eventi;
    private System.Windows.Forms.RichTextBox rt_main;
    private System.Windows.Forms.TabPage tab_impostazioni;
    private System.Windows.Forms.PictureBox pb_close;
    private System.Windows.Forms.CheckBox chk_passo;
    private System.Windows.Forms.TabPage tab_comandi;
  }
}

