<%@ Page Language="C#" MasterPageFile="~/default.master" AutoEventWireup="true" CodeFile="users.aspx.cs"
  Inherits="_users" ClientIDMode="Static" %>

<asp:Content ContentPlaceHolderID="head" runat="Server">
  <script type="text/javascript" language="javascript">

    $(document).ready(function () {
      // sub commands
      if (($("#cmd_action").val() == "view" && $("#cmd_obj").val() == "users")
       || ($("#cmd_action").val() == "view" && $("#cmd_obj").val() == "user")) {
        var cmds = [];
        cmds.push({ fnc: "add_user()", title: "<i class='fas fa-plus-circle mr-2'></i>Aggiungi utente..." });
        set_sub_cmds(cmds);
      }
    });

    function add_user() { window.location = $("#cmd_add").val(); }

    function confirm_user() {
      try {
        status_txt("aggiunta utente...")
        window.setTimeout(function () {
          var result = post_data({ "action": "add_user", "user_name": $("#user_name").val()
            , "email": $("#email").val(), "password": $("#password").val(), "c_password": $("#c_password").val()
          });
          if (result) {
            if (result.des_result == "ok") {
              status_txt_ms("utente aggiungo con successo!", function () { window.location = $("#cmd_users").val(); });
            } else { show_danger("Aggiunta utente", "Ci sono stati dei problemi!<br/><br/>" + result.message); end_status(); }
          } else end_status();
        }, 100);
      } catch (e) { show_danger("Attenzione!", e.message); end_status(); }
      return false;
    }

    function del_user(user, id) {
      show_danger_yn("Attenzione!", "Sei sicuro di voler cancellare l'utente '" + user + "'?"
      , function () { window.setTimeout(function () { del_user2(id); }, 500); });
    }

    function del_user2(id) {
      try {
        status_txt("eliminazione utente in corso...")
        window.setTimeout(function () {
          var result = post_data({ "action": "del_user", "id": id });
          if (result) {
            if (result.des_result == "ok") {
              status_txt_ms("utente eliminato con successo!");
              window.setTimeout(function () { window.location.reload(); }, 2000);
            } else { show_danger("Eliminazione utente", "Ci sono stati dei problemi!<br/><br/>" + result.message); end_status(); }
          } else end_status();
        }, 100);
      } catch (e) { show_danger("Attenzione!", e.message); end_status(); }
    }

    function riattiva_user(id) {
      try {
        status_txt("riattivazione utente in corso...")
        window.setTimeout(function () {
          var result = post_data({ "action": "active_user", "id": id });
          if (result) {
            if (result.des_result == "ok") {
              status_txt_ms("utente riattivato con successo!");
              window.setTimeout(function () { window.location.reload(); }, 2000);
            } else { show_danger("Riattivazione utente", "Ci sono stati dei problemi!<br/><br/>" + result.message); end_status(); }
          } else end_status();
        }, 100);
      } catch (e) { show_danger("Attenzione!", e.message); end_status(); }
    }

    function disattiva_user(user, id) {
      show_warning_yn("Attenzione!", "Sei sicuro di voler disattivare l'utente '" + user + "'?"
      , function () { window.setTimeout(function () { disattiva_user2(id); }, 500); });
    }

    function disattiva_user2(id) {
      try {
        status_txt("disattivazione utente in corso...")
        window.setTimeout(function () {
          var result = post_data({ "action": "disable_user", "id": id });
          if (result) {
            if (result.des_result == "ok") {
              status_txt_ms("utente disattivato con successo!");
              window.setTimeout(function () { window.location.reload(); }, 2000);
            } else { show_danger("Disattivazione utente", "Ci sono stati dei problemi!<br/><br/>" + result.message); end_status(); }
          } else end_status();
        }, 100);
      } catch (e) { show_danger("Attenzione!", e.message); end_status(); }
    }

  </script>
</asp:Content>
<asp:Content ContentPlaceHolderID="contents" runat="Server">
  <input id='cmd_add' type="hidden" runat='server' />
  <input id='cmd_users' type="hidden" runat='server' />
  <div class="container-fluid">
    <div class="row mb-4">
      <!-- sidebar menu -->
      <nav sidebar-tp='menu' class='d-none' sidebar-init='hide'>
        <div id='menu' class='sidebar-sticky' runat='server'>
        </div>
      </nav>
      <!-- contenuti -->
      <div class='col' sidebar-tp='body'>
        <!-- title -->
        <div class='d-block border-bottom p-1 mt-1 mb-4'>
          <h1 id='page_title' runat='server' class='light-blue-text'>
            ...</h1>
          <h3 id='page_des' runat='server'>
          </h3>
        </div>
        <!-- view user, users-->
        <div id='view' runat='server'>
        </div>
        <div id='add' visible="false" class='pt-3' runat='server'>
          <div class="md-form mb-3">
            <input id="user_name" type="text" autocomplete="nope" class="form-control" onkeydown="return event.key == 'Enter' ? confirm_user() : true;">
            <label for="user_name">
              Nome utente <small>(deve contenere SOLO caratteri alfanumerici)</small></label>
          </div>
          <div class="md-form mb-3">
            <input id="email" type="email" autocomplete="nope" class="form-control" onkeydown="return event.key == 'Enter' ? confirm_user() : true;">
            <label for="email">
              Email</label>
          </div>
          <div class="md-form mb-3">
            <input id="password" type="password" autocomplete="new-password" list="autocompleteOff"
              class="form-control" onkeydown="return event.key == 'Enter' ? confirm_user() : true;">
            <label for="password">
              Password <small>(dev'essere lunga almeno 3 caratteri e senza spazi)</small></label>
          </div>
          <div class="md-form mb-5">
            <input id="c_password" type="password" autocomplete="new-password" list="autocompleteOff"
              class="form-control" onkeydown="return event.key == 'Enter' ? confirm_user() : true;">
            <label for="c_password">
              Conferma Password</label>
          </div>
          <div class='text-right'>
            <button type="button" class="btn btn-primary" onclick='return confirm_user()'>
              AGGIUNGI</button>
          </div>
        </div>
      </div>
      <!-- sidebar icon -->
      <div sidebar-tp='sh' onclick='sh_side_menu()'>
        <i sidebar-tp='icon'></i>
      </div>
    </div>
  </div>
</asp:Content>
