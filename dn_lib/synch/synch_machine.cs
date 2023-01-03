using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dn_lib {
  public class synch_machine {
    public enum states { none, start }

    public int synch_machine_id { get; set; }
    public string pc_name { get; set; }
    public string pc_des { get; set; }
    public string ip_address { get; set; }
    public int seconds { get; set; }
    public bool active { get; set; }
    public states state { get; set; }

    public DateTime? dt_start { get; set; }
    public DateTime? dt_stop { get; set; }
    public DateTime? dt_lastsynch { get; set; }

    public int c_folders { get; set; }
    public int c_files { get; set; }
    public int c_deleted { get; set; }
    public int s_synch { get; set; }

    public synch_machine(int synch_machine_id, string pc_name, string pc_des, int seconds, bool active, string state) {
      this.synch_machine_id = synch_machine_id; this.pc_name = pc_name; this.pc_des = pc_des;
      this.seconds = seconds; this.active = active; 
      this.state = !string.IsNullOrEmpty(state) ? (states)Enum.Parse(typeof(states), state) : states.none;
    }

    public synch_machine(int synch_machine_id, string pc_name, string pc_des, int seconds, bool active, string state
      , string ip_address, DateTime? dt_start, DateTime? dt_stop, DateTime? dt_lastsynch
      , int c_folders, int c_files, int c_deleted, int s_synch) {
      this.synch_machine_id = synch_machine_id; this.pc_name = pc_name; this.pc_des = pc_des;
      this.seconds = seconds; this.active = active; this.ip_address = ip_address;
      this.state = !string.IsNullOrEmpty(state) ? (states)Enum.Parse(typeof(states), state) : states.none;
      this.dt_start = dt_start; this.dt_stop = dt_stop; this.dt_lastsynch = dt_lastsynch;
      this.c_folders = c_folders; this.c_files = c_files; this.c_deleted = c_deleted; this.s_synch = s_synch;
    }
  }
}
