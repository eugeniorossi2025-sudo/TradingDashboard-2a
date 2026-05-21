<%@ Control Language="VB" AutoEventWireup="false" CodeFile="TxtFileSingleButton.ascx.vb"
    Inherits="Controls.Control_TxtFileSingleButton" %>
<script type="text/javascript">

    function CheckFileExistence() {

        //        var filePath = document.getElementById('<%= fileupDoc.ClientID %>').value;

        //        if (filePath.length < 1) { 
        //            alert("Selezionare un documento pdf!"); return false;
        //        }



        //        var validExtensions = new Array(); var ext = filePath.substring(filePath.lastIndexOf('.') + 1).toLowerCase();


        //        validExtensions[0] = 'pdf'; 

        //        for (var i = 0; i < validExtensions.length; i++) {

        //            if (ext == validExtensions[i]) return true;
        //        }

        //        alert('Estensione ' + ext.toUpperCase() + ' non valida!');

        //        return false;
        return true;
    }



</script>
<asp:TextBox ID="TxtVal" runat="server" Visible="false"></asp:TextBox>
<asp:TextBox ID="TxtFileName" runat="server" Visible="false"></asp:TextBox>
<div class="clear">
</div>  
<div style="height: 89px; width: 120px; position: relative; top: 126px; left: 129px;">
    <asp:FileUpload ID="fileupDoc" runat="server" Style="top: 1px; left: -10px; width: 265px;
        position: relative; height: 26px; opacity: 0; filter: alpha(opacity=0)" Font-Size="30pt" />
    <asp:Button ID="btnUploadDoc" CssClass="button bianco" runat="server"  OnClientClick="return CheckFileExistence()" Text="Carica" Style="top: -5px; left: -265px;
        z-index: 1; width: 151px; position: relative;" Height="22px" />
</div> 
<asp:Label ID="lblObbl" ForeColor="Red" Visible="false" runat="server">*</asp:Label>
<asp:Label ID="LblFile" ForeColor="Blue" runat="server">*</asp:Label> 
<br />
<asp:Label ID="lblErr" runat="server"></asp:Label>
