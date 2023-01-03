using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;

namespace dn_client {
  public class sql_conn {

    public DbConnection conn { get; protected set; }
    public string provider { get; protected set; }
    public string conn_string { get; protected set; }

    protected DbProviderFactory _factory = null;

    public sql_conn(string provider, string conn_string) { this.provider = provider; this.conn_string = conn_string; }

    protected void open_conn() {
      if (string.IsNullOrEmpty(this.conn_string) && this.conn == null) 
        throw new Exception("non è stata specificata la stringa di connessione!");
      if (string.IsNullOrEmpty(this.provider)) 
        throw new Exception("non è stato specificato il provider!");
      if (_factory == null) _factory = DbProviderFactories.GetFactory(this.provider);
      if (this.conn == null) this.conn = create_conn(_factory, conn_string);
      this.conn.Open();
    }

    public void close_conn() { if (this.is_open) this.conn.Close(); }

    public bool is_open { get { return this.conn != null && this.conn.State != ConnectionState.Closed; } }

    protected DbConnection create_conn(DbProviderFactory provider, string conn_string) {
      DbConnection connection = provider.CreateConnection();
      connection.ConnectionString = conn_string;
      return connection;
    }

    protected void check_conn() { if (!this.is_open) open_conn(); }

    public DataTable open_set(string sql, int? timeout = null) {
      check_conn();

      DbCommand cmd = this.conn.CreateCommand();
      //if (_trans != null) cmd.Transaction = _trans;
      cmd.CommandText = sql;
      if (timeout.HasValue) cmd.CommandTimeout = timeout.Value;
      cmd.CommandType = CommandType.Text;

      DbDataAdapter ad = _factory.CreateDataAdapter();
      ad.SelectCommand = cmd;
      DataTable dt = new DataTable();
      ad.Fill(dt);
      return dt;
    }

    public int execute(string sql, int? timeout = null, DbParameter[] pars = null) {
      check_conn();
      DbCommand cmd = this.conn.CreateCommand();
      if (timeout.HasValue) cmd.CommandTimeout = timeout.Value;
      //if (_trans != null) cmd.Transaction = _trans;
      if (pars != null) { foreach (DbParameter p in pars) cmd.Parameters.Add(p); }
      cmd.CommandText = sql;
      return cmd.ExecuteNonQuery();
    }
  }
}
