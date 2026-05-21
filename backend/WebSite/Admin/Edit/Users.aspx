<%@ Page Title="" Language="VB" MasterPageFile="~/Admin/MasterFirst.master" AutoEventWireup="false"
    CodeFile="Users.aspx.vb" Inherits="Users" Trace="false" %>

<%@ Register TagPrefix="DtControl" TagName="Txt" Src="~/Admin/Control/Txt.ascx" %>
<%@ Register TagPrefix="DtControl" TagName="TxtDate" Src="~/Admin/Control/TxtDate.ascx" %>
<%@ Register TagPrefix="DtControl" TagName="Chk" Src="~/Admin/Control/Chk.ascx" %>
<%@ Register TagPrefix="DtControl" TagName="Drp" Src="~/Admin/Control/Drp.ascx" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register TagPrefix="DtControl" TagName="TxtFile" Src="~/Admin/Control/TxtFile.ascx" %> 
<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" runat="Server">

    <style>
        .MyGridClass .rgDataDiv
        {
        height : 60% !important ;
        }

    </style>
 <%--   <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>--%>
            <div class="tool-top-container">
                <asp:Label ID="LblInfo" runat="server" class="title-cat"></asp:Label>

                <div class="tool-top-link">
                    <asp:LinkButton ID="LnkNew" runat="server" class="btn btn-primary"  CommandName="InitInsert">
                        New User</asp:LinkButton>&nbsp;&nbsp;
                    <asp:LinkButton ID="LnkRefresh" runat="server" CommandName="RebindGrid">
                   <span id="Img2" runat="server" class="btn-ico material-symbols-outlined" title="Refresh">autorenew</span>
                </asp:LinkButton> 
                    <!-- <asp:LinkButton ID="LnkExportExcel" runat="server" CommandName="RebindGrid" visible="false">
                        <span id="Img4" runat="server" class="btn-ico material-symbols-outlined" title="Refresh">download</span>

                </asp:LinkButton>  -->
                </div>
    
            </div>

            <asp:MultiView ActiveViewIndex="0" ID="multiView" runat="server">
                <asp:View ID="viewGrid" runat="server">
                
                    <telerik:RadGrid 
                        ID="RadGrid1" 
                        AllowPaging="True" 
                        runat="server" 
                        OnNeedDataSource="RadGrid1_NeedDataSource"
                        AllowFilteringByColumn="false" 
                        EnableHeaderContextMenu="false" 
                        AllowSorting="True" 
                        ClientSettings-Scrolling-AllowScroll="true"
                        PageSize="50" 
                        ShowFooter="false" 
                        AutoGenerateColumns="false" 
                        ShowStatusBar="true"   
                        CssClass="MyGridClass table-grid">
                        <MasterTableView>
                            <Columns>
                                <telerik:GridTemplateColumn AllowFiltering="false" HeaderStyle-Width="30" UniqueName="Edit"
                                    HeaderText="Open">
                                    <ItemTemplate>
                                            <asp:LinkButton ID="ImgOpen" runat="server" CommandName="Open" CommandArgument='<%# DataBinder.Eval(Container, "DataItem.ID") %>'>
                                                <span class="btn-ico material-symbols-outlined" title="open">edit</span>
                                            </asp:LinkButton>
                                    </ItemTemplate>
                                </telerik:GridTemplateColumn> 
                                <telerik:GridTemplateColumn AllowFiltering="false" HeaderStyle-Width="30" UniqueName="Delete"
                                    HeaderText="Delete" Visible="true">
                                    <ItemTemplate>

                                        <asp:LinkButton ID="ImgDelete" runat="server" CommandName="Delete" OnClientClick="javascript:return confirm('Delete record?')" CommandArgument='<%# DataBinder.Eval(Container, "DataItem.ID") %>'>
                                            <span class="btn-ico material-symbols-outlined" title="delete" ToolTip="Delete">delete</span>
                                        </asp:LinkButton>
                                    </ItemTemplate>
                                </telerik:GridTemplateColumn>
                                <telerik:GridBoundColumn FilterCheckListEnableLoadOnDemand="true" FilterControlAltText="Filter ContactName column" DataField="Description" UniqueName="Description" HeaderText="Description"></telerik:GridBoundColumn>
                                <telerik:GridBoundColumn FilterCheckListEnableLoadOnDemand="true" FilterControlAltText="Filter ContactName column" DataField="Username" UniqueName="Username" HeaderText="Username"></telerik:GridBoundColumn>
                                <telerik:GridBoundColumn FilterCheckListEnableLoadOnDemand="true" FilterControlAltText="Filter ContactName column" DataField="Password" UniqueName="Password" HeaderText="Password"></telerik:GridBoundColumn>
                                <telerik:GridBoundColumn FilterCheckListEnableLoadOnDemand="true" FilterControlAltText="Filter ContactName column" DataField="Administrator" UniqueName="Administrator" HeaderText="Administrator"></telerik:GridBoundColumn>
                                <telerik:GridBoundColumn FilterCheckListEnableLoadOnDemand="true" FilterControlAltText="Filter ContactName column" DataField="LastLoginDate" UniqueName="LastLoginDate" HeaderText="LastLoginDate"></telerik:GridBoundColumn>                                
                            </Columns>
                        </MasterTableView>
                        <GroupingSettings CaseSensitive="false" />
                        <ClientSettings  >
                            <Scrolling AllowScroll="true"  /> 
                            <Selecting AllowRowSelect="true"></Selecting>
                        </ClientSettings>
                        <PagerStyle Mode="NextPrevAndNumeric"></PagerStyle>
                        <FilterMenu EnableTheming="True">
                            <CollapseAnimation Duration="200" Type="OutQuint" />
                        </FilterMenu>
                    </telerik:RadGrid>
                </asp:View>
                <asp:View ID="viewDetail" runat="server">
                    <div class="row">
                        <div class="col-md-12 col-sm-12 col-xs-12">
                            <div class="x_panel">
                                <div class="x_content">
                                    <div class="form-horizontal form-label-left">
                                        <div class="form-group" style="display: none">
                                            <label class="control-label col-md-3 col-sm-3 col-xs-12" >
                                                Id<span class="required">*</span>
                                            </label>
                                            <div class="col-md-5 col-sm-5 col-xs-12">
                                                <asp:TextBox ID="TxtId" Style="visibility: hidden" Width="400" runat="server"></asp:TextBox>
                                            </div>
                                        </div> 
                                        <div class="form-group row">
                                            <div class="col-md-5 col-sm-5 col-xs-12">
                                                <label class="control-label" >
                                                Description<span class="required">*</span>
                                            </label>
                                                <DtControl:Txt ID="Txt2" MaxLenght="255" DataField="Description" runat="server" Required="true" />
                                            </div> 

                                        </div>
                                        
                                        <div class="form-group row">
                                            
                                            <div class="col-md-6 col-sm-6 col-xs-12">
                                                <label class="control-label " >
                                                    Username<span class="required">*</span>
                                                </label>
                                            
                                                <DtControl:Txt ID="Txt4" MaxLenght="255" DataField="Username" runat="server" Required="true" />
                                            </div>
                                            <div class="col-md-6 col-sm-6 col-xs-12">
                                                <label class="control-label" >
                                                    Password<span class="required">*</span>
                                                </label>
                                                <DtControl:Txt ID="Txt_Password" TextMode="Password" MaxLenght="255" DataField="Password" Type="password" runat="server" Required="true" />
                                            </div>
                                        </div>  
                                        <div class="form-group">
                                            <div class="col-md-6 col-sm-6 col-xs-12">
                                            <label class="control-label" >
                                                Administrator
                                            </label>
                                            
                                                <DtControl:Chk ID="Chk2" MaxLenght="255" DataField="Administrator" runat="server"   />
                                            </div>
                                        </div>
                                        <div class="form-group">
                                            <div class="col-md-6 col-sm-6 col-xs-12 ">
                                                <asp:Button ID="btnSave" CssClass="btn btn-primary outline submit" Text="Save" runat="server" />
                                                <asp:Button ID="btnCancel" CssClass="btn onlytext submit" Text="Cancel" runat="server" />
                                                
                                            </div>
                                        </div>
                                     
                                        <div class="form-group">
                                            <div class="col-md-6 col-sm-6 col-xs-12 col-md-offset-3">
                                                <div runat="server" id="Div_Error" class="alert alert-warning alert-dismissible fade in" role="alert">
                                                  Enter all the required fields
                                                </div>
                                                <div runat="server" id="Div_Warning" class="alert alert-warning alert-dismissible fade in" role="alert">
                                                  Enter all fields correctly
                                                </div>
                                                <div runat="server" id="Div_Terminated" class="alert alert-success alert-dismissible fade in" role="alert">
                                                  Operation Finished
                                                </div>
                                                <br />
                                                <br />
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </asp:View>
            </asp:MultiView>
       <%-- </ContentTemplate>
    </asp:UpdatePanel>--%>
</asp:Content>
