<%@ Control Language="VB" AutoEventWireup="false" CodeFile="TxtFile.ascx.vb" Inherits="Controls.Control_TxtFile" %>
<script type="text/javascript">

    function CheckFileExistence() {

        var filePath = document.getElementById('<%= fileupLogo.ClientID %>').value;

        if (filePath.length < 1) {

            alert("Select an image!"); return false;
        }



        var validExtensions = new Array(); var ext = filePath.substring(filePath.lastIndexOf('.') + 1).toLowerCase();


        validExtensions[0] = 'jpg';

        validExtensions[1] = 'jpeg';
        validExtensions[2] = 'bmp';

        validExtensions[3] = 'png';
        validExtensions[4] = 'gif';

        validExtensions[5] = 'tif'; validExtensions[6] = 'eps';


        for (var i = 0; i < validExtensions.length; i++) {

            if (ext == validExtensions[i]) return true;
        }

        alert('Invalid extension (' + ext.toUpperCase() + ') !');

        return false;
    }



</script>
<asp:TextBox ID="TxtVal" runat="server" Visible="false"></asp:TextBox>
<div class="clear">
</div>
<asp:FileUpload  ID="fileupLogo" runat="server">
</asp:FileUpload>
<asp:Label ID="lblObbl" ForeColor="Red" Visible="false" runat="server">*</asp:Label>
<asp:Button ID="btnUploadLogo" runat="server" CssClass="btn btn-primary outline submit"     Text="Upload" />
<br />
<asp:Label ID="lblErr" runat="server"></asp:Label>
<br />
<asp:Image ID="pvwImage" runat="server" AlternateText="Anteprima" Style="margin: 3px;background-color:#eceddc;padding:5px;width: 200px; height: 180px; vertical-align: middle;border-radius: 5px;box-shadow: inset 0 0 5px #ccc;" />
