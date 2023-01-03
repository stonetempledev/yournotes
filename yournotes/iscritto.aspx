<%@ Page Language="C#" MasterPageFile="~/user.master" AutoEventWireup="true" CodeFile="iscritto.aspx.cs" Inherits="login"
  ClientIDMode="Static" %>

<asp:Content ContentPlaceHolderID="head" runat="Server">
  <script language="javascript">
    $(document).ready(function () {
    });
  </script>
</asp:Content>
<asp:Content ContentPlaceHolderID="contents" runat="Server">
  <div class='container'>
    <div class='row'>
      <div class="col">
        <div class="jumbotron">
          <h1 id='txt_title' class="display-5" runat='server'>
            ...</h1>
          <hr class="my-4">
          <p id='txt_body' class="lead" runat='server'>
            ...</p>
        </div>
      </div>
    </div>
  </div>
</asp:Content>
