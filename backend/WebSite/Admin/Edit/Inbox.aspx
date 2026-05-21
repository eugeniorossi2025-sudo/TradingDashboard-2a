<%@ Page Title="" Language="VB" MasterPageFile="~/Admin/MasterFirst.master" AutoEventWireup="false"
    CodeFile="Inbox.aspx.vb" Inherits="Contact_Type" Trace="false" %>

<%@ Register TagPrefix="DtControl" TagName="Txt" Src="~/Admin/Control/Txt.ascx" %>
<%@ Register TagPrefix="DtControl" TagName="TxtDate" Src="~/Admin/Control/TxtDate.ascx" %>
<%@ Register TagPrefix="DtControl" TagName="Chk" Src="~/Admin/Control/Chk.ascx" %>
<%@ Register TagPrefix="DtControl" TagName="Drp" Src="~/Admin/Control/Drp.ascx" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register TagPrefix="DtControl" TagName="TxtFile" Src="~/Admin/Control/TxtFile.ascx" %>
<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" runat="Server">

    <style>
        input[type=checkbox], input[type=radio] {
            margin: 11px 0 0; 
        }
    </style>


    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>
            
            <asp:MultiView ActiveViewIndex="0" ID="multiView" runat="server">
                <asp:View ID="viewGrid" runat="server">
                    <div class="tool-top-container">
                        <asp:Label ID="LblInfo" runat="server" class="title-cat"></asp:Label>
                        <div class="tool-top-link">
                            <asp:LinkButton ID="LnkNew" runat="server" CommandName="InitInsert">
                            <img id="Img1" style="border: 0px; vertical-align: middle;" alt="" runat="server"
                                src="~/Admin/images/add.png" />Aggiungi
                        </asp:LinkButton>&nbsp;&nbsp;
                        <asp:LinkButton ID="LnkRefresh" runat="server" CommandName="RebindGrid">
                            <img id="Img2" style="border: 0px; vertical-align: middle;" alt="" runat="server"
                                src="~/Admin/images/refresh.png" />Refresh
                        </asp:LinkButton>&nbsp;&nbsp;  
                        <asp:LinkButton ID="LnkExportExcel" runat="server" CommandName="RebindGrid"  visible="false">
                            <img id="Img4" style="border: 0px; vertical-align: middle;" alt="" runat="server"
                                src="~/Admin/images/export_excel.png" />Excel
                        </asp:LinkButton> 
                    </div>
    
                </div>
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
                                    HeaderText="">
                                    <ItemTemplate>
                            
                                            <asp:LinkButton ID="ImgOpen" runat="server" CommandName="Open" CommandArgument='<%# DataBinder.Eval(Container, "DataItem.ID") %>'>
                                                <span class="btn-ico material-symbols-outlined" title="open">edit</span>
                                            </asp:LinkButton>
                                        </ItemTemplate>
                                </telerik:GridTemplateColumn>
                                <telerik:GridTemplateColumn AllowFiltering="false" HeaderStyle-Width="30" UniqueName="Delete"
                                    HeaderText="Delete" Visible="true">
                                    <ItemTemplate>
                                        <asp:ImageButton ID="ImgDelete" CommandName="Delete" OnClientClick="javascript:return confirm('Eliminare?')"
                                            CommandArgument='<%# DataBinder.Eval(Container, "DataItem.ID") %>' ImageAlign="Middle"
                                            runat="server" ImageUrl="~/Admin/images/bin.png" ToolTip="Delete" />
                                    </ItemTemplate>
                                </telerik:GridTemplateColumn>
                                <telerik:GridBoundColumn DataField="CompanyName" UniqueName="Azienda" HeaderText="Azienda"></telerik:GridBoundColumn>
                                <telerik:GridBoundColumn DataField="Note" UniqueName="Note" HeaderText="Progetto"></telerik:GridBoundColumn>
                                <telerik:GridBoundColumn DataField="Message" UniqueName="Message" HeaderText="Messaggio"></telerik:GridBoundColumn> 
                                <telerik:GridBoundColumn DataField="bit_read" UniqueName="bit_read" HeaderText="Letto"></telerik:GridBoundColumn> 
                            </Columns>
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
                </asp:View>
                <asp:View ID="viewDetail" runat="server">
                    <div class="row">
                        <div class="col-md-12 col-sm-12 col-xs-12">
                            <div class="x_panel">
                                <div class="x_content">
                                    <div class="form-horizontal form-label-left">
                                        <div class="form-group" style="display: none">
                                            <label class="control-label col-md-3 col-sm-3 col-xs-12">
                                                Id<span class="required">*</span>
                                            </label>
                                            <div class="col-md-6 col-sm-6 col-xs-12">
                                                <asp:TextBox ID="TxtId" Style="visibility: hidden" Width="400" runat="server"></asp:TextBox>
                                            </div>
                                        </div>
                                       
                                        <div class="form-group">
                                            <label class="control-label col-md-1 col-sm-1 col-xs-1">
                                                Segna come letto
                                            </label>
                                            <div class="col-md-8 col-sm-8 col-xs-12">
                                                <DtControl:Chk ID="Txt2"  MaxLenght="255" DataField="bit_read" runat="server" />
                                            </div>
                                        </div>
                                          
                                          
                                        <div class="form-group">
                                        
                                           <div class="col-md-6 col-sm-6 col-xs-12 col-md-offset-3">

                                                <asp:Button ID="btnCancel" CssClass="btn btn-upload submit" Text="Esci" runat="server" />
                                                <asp:Button ID="btnSave" CssClass="btn btn-primary submit" Text="Salva" runat="server" />
                                            </div>
                                        </div>
                                        <div class="ln_solid">
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
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
