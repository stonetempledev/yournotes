<%@ Page Language="C#" MasterPageFile="~/user.master" AutoEventWireup="true" CodeFile="new.aspx.cs"
  Inherits="login" ClientIDMode="Static" %>

<asp:Content ContentPlaceHolderID="head" runat="Server">
  <script language="javascript">
    $(document).ready(function () {
      if (!$("#user_mail").val()) $("#user_mail").focus();
      else if (!$("#user_name").val()) $("#user_name").focus();
      else $("#user_pass").focus();
    });
  </script>
</asp:Content>
<asp:Content ContentPlaceHolderID="contents" runat="Server">
  <div class='container'>
    <div class='row'>
      <div class="col">
        <h2>
          Entra nella deepa-notes!</h2>
      </div>
    </div>
    <div class='row' style='padding-top: 40px;'>
      <div class="col">
        <label class='h4'>
          Email</label>
        <input id="user_mail" type="text" runat="server" class="form-control" placeholder="Email"
          autofocus="" autocomplete="nope" />
      </div>
    </div>
    <div class='row' style='padding-top: 40px;'>
      <div class="col">
        <label class='h4'>
          il tuo nomignolo <small>(deve contenere SOLO caratteri alfanumerici)</small></label>
        <input id="user_name" type="text" runat="server" class="form-control" placeholder="Nomignolo" autocomplete="nope" />
      </div>
    </div>
    <div class='row' style='padding-top: 20px;'>
      <div class="col">
        <label class='h4'>
          Password <small>(dev'essere lunga almeno 3 caratteri e senza spazi)</small></label>
        <input id="user_pass" type="password" runat='server' class="form-control" placeholder="Password" autocomplete="new-password" />
      </div>
    </div>
    <div class='row' style='padding-top: 20px;'>
      <div class="col">
        <label class='h4'>
          Conferma la password</label>
        <input id="user_pass2" type="password" runat='server' class="form-control" placeholder="Conferma Password" autocomplete="new-password" />
      </div>
    </div>
    <div class='row' style='padding-top: 40px;'>
      <div class="col">
        <button id="btn_go" class="btn btn-lg btn-primary btn-block" onserverclick="Go_Click"
          runat="server">
          REGISTRATI</button>
        <div id='lbl_alert' class='alert alert-danger' runat='server' visible='false' style='margin-top: 25px;'>
        </div>
        <div id='lbl_ok' class='alert alert-success' runat='server' visible='false' style='margin-top: 25px;'>
        </div>
      </div>
    </div>
  </div>
</asp:Content>
