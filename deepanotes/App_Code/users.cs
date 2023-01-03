using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using dn_lib.db;
using dn_lib.tools;

namespace deepanotes {
  public class users : bo {
    public users() {
    }

    public List<user> list_users() {
      return db_conn.dt_table(core.parse(config.get_query("users.users").text
        , new Dictionary<string, object>() { { "id", "" } })).Rows.Cast<DataRow>()
        .Select(x => create_user(x)).ToList();
    }

    public user get_user(int id) {
      return create_user(db_conn.first_row(core.parse(config.get_query("users.users").text, new Dictionary<string, object>() { { "id", id.ToString() } })));
    }

    protected user create_user(DataRow r) {
      return new user(db_provider.int_val(r["user_id"]), db_provider.str_val(r["nome"])
          , db_provider.str_val(r["email"]), user.type_user.normal, db_provider.int_val(r["activated"]) == 1, db_provider.dt_val(r["dt_activate"])
          , db_provider.dt_val(r["dt_upd"]), db_provider.dt_val(r["dt_ins"]), db_provider.int_val(r["activated"]) == 3);
    }

    public int add_utente(string u, string mail, string pass, string pass2, out string tkey, out string akey, int ikey = 2) {
      bool close_trans = false; tkey = akey = "";
      try {
        if (mail == "") throw new Exception("devi scrivere la email!");
        else if (u == "") throw new Exception("qual'è il tuo nomignolo?");
        else if (!strings.is_alpha(u)) throw new Exception("il tuo nomignolo contiene almeno un carattere sbagliato!");
        else if (pass == "") throw new Exception("devi scrivere la password!");
        else if (pass.Length < 3) throw new Exception("la password dev'essere almeno di 3 caratteri!");
        else if (pass.Contains(' ')) throw new Exception("la password ha uno spazio e non va bene!");
        else if (pass2 == "") throw new Exception("devi confermare la password!");
        else if (pass != pass2) throw new Exception("la conferma della password è andata male!");
        else {

          close_trans = db_conn.check_begin_trans();

          // check nomignolo
          DataRow dr = db_conn.first_row(@"select count(*) as cc from users 
                where isnull(activated, 0) in (1, 2, 3) and nome = '" + u + "';");
          if ((int)dr["cc"] > 0) throw new Exception("c'è già uno che si chiama " + u + "!");

          // registrazione
          tkey = cry.rnd_str(32); akey = cry.rnd_str(32);
          int user_id = int.Parse(db_conn.exec(string.Format(@"insert into users (nome, email, pwd, dt_ins, tmp_key, activate_key, activated)
            values ('{0}', '{1}', '{2}', getdate(), '{3}', '{4}', {5});", u, mail, cry.encode_tobase64(pass), tkey, akey, ikey), true));
          if (close_trans) db_conn.commit();
          return user_id;
        }
      } catch (Exception ex) {
        if (close_trans) db_conn.rollback(); log.log_err(ex); throw ex;
      }
    }

    public void del_utente(int id) { db_conn.exec(core.parse_query("users.del-user", new string[,] { { "id", id.ToString() } })); }

    public void disable_utente(int id) { db_conn.exec(core.parse_query("users.disable-user", new string[,] { { "id", id.ToString() } })); }

    public void riactive_utente(int id) {

      // check nomignolo
      DataRow dr = db_conn.first_row(@"select nome from users u
        where isnull(u.activated, 0) in (1, 2, 3) and u.user_id <> " + id.ToString()
          + " and nome = (select nome from users where user_id = " + id.ToString() + ");");
      if (dr != null) throw new Exception("c'è già uno che si chiama " + dr["name"] + "!");

      db_conn.exec(core.parse_query("users.active-user", new string[,] { { "id", id.ToString() } }));
    }
  }
}