<%@ Control Language="VB" AutoEventWireup="false" CodeFile="Drp.ascx.vb" Inherits="Controls.Control_Drp" %>
<asp:UpdatePanel runat="server">
    <ContentTemplate>
        <div class="">
            <asp:DropDownList ID="TxtVal" runat="server" class="chosen-select form-control"  >
            </asp:DropDownList>
            <div style="float:right;width:14%;margin-top: 4px;">
                <asp:ImageButton ToolTip="Open" ID="BtnEdit" ImageUrl="~/Admin/images/pen_dropdown.png"
                    runat="server" />
                <asp:ImageButton ID="BtnAddNew" ToolTip="Add New" ImageUrl="~/Admin/images/add.png" runat="server" />
                <asp:ImageButton ID="BtnDelete" ToolTip="Delete" ImageUrl="~/Admin/images/remove_configuration.png"
                    OnClientClick="javascript:return confirm('Delete record?')" runat="server" />
                
            </div>
            <div id="DivEdit" visible="false" runat="server" style="float:left">
                    <asp:Label Visible="false" runat="server" ID="LblId"></asp:Label>
                    <asp:Label Visible="false" runat="server" ID="LblOperazione"></asp:Label>
                    <asp:TextBox runat="server" ID="TxtEdit"></asp:TextBox>
                    <asp:Button ID="BtnSave" Text="Save" runat="server" />&nbsp;<asp:Button ID="BtnCancel"
                        Text="Cancel" runat="server" />
                </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
