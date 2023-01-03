<%@ Page Language="C#" MasterPageFile="~/user.master" AutoEventWireup="true" CodeFile="confirm.aspx.cs" Inherits="confirm"
  ClientIDMode="Static" %>

<asp:Content ContentPlaceHolderID="head" runat="Server">
  <script language="javascript">
    function startTimer(duration, display, url) {
      var timer = duration, minutes, seconds;
      setInterval(function () {
        minutes = parseInt(timer / 60, 10)
        seconds = parseInt(timer % 60, 10);

        minutes = minutes == 0 ? "" : (minutes < 10 ? "0" + minutes : minutes);
        seconds = seconds < 10 ? "0" + seconds : seconds;

        display.textContent = (minutes != "" ? minutes + ":" : "") + seconds;

        if (--timer < 0) { window.location.href = url; };
      }, 1000);
    }

    $(document).ready(function () {
      if ($("#c_down")) startTimer(15, document.querySelector('#time'), 'login.aspx?nm=' + $("#lbl_user").val());
    });
  </script>
</asp:Content>
<asp:Content ContentPlaceHolderID="contents" runat="Server">
  <input id='lbl_user' runat='server' type='hidden' />
  <div class='container'>
    <div class='row'>
      <div class="col">
        <div class="jumbotron">
          <h1 id='txt_title' class="display-5" runat='server'>
            ...</h1>
          <hr class="my-4">
          <p id='txt_body' class="lead" runat='server'>
            ...</p>
          <p id='c_down' class='lead' runat='server' visible='false'>
            <b>Ti faccio entrare fra <span id="time">15</span> secondi!</b></p>
        </div>
      </div>
    </div>
  </div>
</asp:Content>
