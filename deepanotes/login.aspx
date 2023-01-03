<%@ Page Language="C#" MasterPageFile="~/user.master" AutoEventWireup="true" CodeFile="login.aspx.cs"
  Inherits="login" ClientIDMode="Static" %>

<asp:Content ContentPlaceHolderID="head" runat="Server">
  <script language="javascript">
    $(document).ready(function () {
      if ($("#user_mail").val()) $("#user_pass").focus(); else $("#user_mail").focus();
      $("#user_pass").val("");
    });
  </script>
</asp:Content>
<asp:Content ContentPlaceHolderID="contents" runat="Server">
  <!-- navbar -->
  <nav class="navbar navbar-expand-lg navbar-dark bg-primary">
    <a class="navbar-brand" href="#">
      <img src="images/dn-40.png" class="d-inline-block align-middle" alt="">
    </a>
   
    <button class="navbar-toggler" style='float:right;' type="button" data-toggle="collapse" data-target="#coll_nav" aria-controls="coll_nav" aria-expanded="false" aria-label="Toggle navigation">
      <span class="navbar-toggler-icon"></span>
    </button>

    <div class="collapse navbar-collapse" id="coll_nav">
      <ul class="navbar-nav mx-md-auto">
      </ul>
      <ul class="navbar-nav mx-mr-auto">
        <li class="nav-item">
        <asp:LinkButton ID="btn_new" CssClass="nav-link" OnClick="New_Click"
          Text="REGISTRATI" runat="server" />
        </li>
        <li class="nav-item">
          <div class="dropdown-divider"></div>
        </li>
        <li class="nav-item">
        <asp:LinkButton ID="btn_lost" CssClass="nav-link" OnClick="Lost_Click"
          Text="REIMPOSTA LA PASSWORD" runat="server" />
        </li>
      </ul>
    </div>
  </nav>
  <div class='container'>
    <div class='row'>
      <div class="col mt-3">
        <h2>
          Chi sei?</h2>
      </div>
    </div>
    <div class='row' style='padding-top: 40px;'>
      <div class="col">
        <label class='h4'>
          Nome <small>(o nomignolo)</small></label>
        <input id="user_mail" type="text" runat="server" class="form-control" autocomplete="nope" placeholder="Nome o nomignolo"
          autofocus="" />
      </div>
    </div>
    <div class='row' style='padding-top: 20px;'>
      <div class="col">
        <label class='h4'>
          Password</label>
        <input id="user_pass" type="password" runat='server' class="form-control" autocomplete="new-password" placeholder="Password" />
      </div>
    </div>
    <div class='row' style='padding-top: 40px;'>
      <div class="col">
        <button id="btn_logon" runat='server' class="btn btn-lg btn-primary btn-block" onserverclick="Logon_Click">
          ENTRA</button>
        <div id='lbl_alert' class='alert alert-danger' runat='server' visible='false' style='margin-top: 25px;'>
        </div>
      </div>
    </div>
  </div>
</asp:Content>
