<%@ Page Title="" Language="VB" MasterPageFile="~/Admin/MasterFirst.master" AutoEventWireup="false"
    CodeFile="LogJson.aspx.vb" Inherits="LogJson" Trace="false" %>

<%@ Register TagPrefix="DtControl" TagName="Txt" Src="~/Admin/Control/Txt.ascx" %>
<%@ Register TagPrefix="DtControl" TagName="TxtDate" Src="~/Admin/Control/TxtDate.ascx" %>
<%@ Register TagPrefix="DtControl" TagName="Chk" Src="~/Admin/Control/Chk.ascx" %>
<%@ Register TagPrefix="DtControl" TagName="Drp" Src="~/Admin/Control/Drp.ascx" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register TagPrefix="DtControl" TagName="TxtFile" Src="~/Admin/Control/TxtFile.ascx" %>
<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" runat="Server">

    <style>
        .MyGridClass .rgDataDiv {
            height: 60% !important;
        }
    </style>


    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>
            <div class="tool-top-container">
                <asp:Label ID="LblInfo" runat="server" class="title-cat"></asp:Label>
                
                   
                </div>
            </div> 
                    <telerik:RadGrid
                        ID="RadGrid1"
                        AllowPaging="True"
                        runat="server"
                        OnNeedDataSource="RadGrid1_NeedDataSource" 
                        EnableHeaderContextMenu="True"
                        AllowSorting="True"
                        ClientSettings-Scrolling-AllowScroll="True"
                        PageSize="50"
                        ShowFooter="False"
                        AutoGenerateColumns="True"
                        ShowStatusBar="True" >

                        <MasterTableView>

                            
                        </MasterTableView>
                        <GroupingSettings CaseSensitive="false" />
                        <ClientSettings>
                            <Scrolling AllowScroll="true" />
                            <Selecting AllowRowSelect="true"></Selecting>
                        </ClientSettings>
                        <PagerStyle Mode="NextPrevAndNumeric"></PagerStyle>
                        <FilterMenu EnableTheming="True">
                            <CollapseAnimation Duration="200" Type="OutQuint" />
                        </FilterMenu>
                    </telerik:RadGrid> 
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
