<%@ Control Language="VB" AutoEventWireup="false" CodeFile="TxtFileAll.ascx.vb" Inherits="Controls.Control_TxtFileAll" %>
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
<asp:FileUpload ID="fileupDoc" runat="server">
</asp:FileUpload>
<asp:Label ID="lblObbl" ForeColor="Red" Visible="false" runat="server">*</asp:Label>
<asp:HyperLink runat="server" ID="Lnk_File">
<asp:Label ID="LblFile" ForeColor="Blue"  runat="server">*</asp:Label></asp:HyperLink>
<asp:Button ID="btnUploadDoc" runat="server"  OnClientClick="return CheckFileExistence()"  Text="Carica" />
<br />
<asp:Label ID="lblErr" runat="server"></asp:Label>
 
