<%@ Page Language="VB" AutoEventWireup="false" CodeFile="~/Admin/Edit/Message.aspx.vb"
    Inherits="Message" %>

<%@ Register TagPrefix="DtControl" TagName="Txt" Src="~/Admin/Control/Txt.ascx" %>
<%@ Register TagPrefix="DtControl" TagName="TxtDate" Src="~/Admin/Control/TxtDate.ascx" %>
<%@ Register TagPrefix="DtControl" TagName="Chk" Src="~/Admin/Control/Chk.ascx" %>
<%@ Register TagPrefix="DtControl" TagName="Drp" Src="~/Admin/Control/Drp.ascx" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register TagPrefix="DtControl" TagName="TxtFile" Src="~/Admin/Control/TxtFile.ascx" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <!-- Bootstrap core CSS -->
    <link href="/Admin/css/bootstrap.min.css" rel="stylesheet">
    <link href="/Admin/fonts/css/font-awesome.min.css" rel="stylesheet">
    <link href="/Admin/css/animate.min.css" rel="stylesheet">
    <link rel="stylesheet" href="/Admin/css/chosen/chosen.css">   
    <link href="/Admin/css/datepicker/datepicker.css" rel="stylesheet" type="text/css" />

    <!-- Custom styling plus plugins -->
    <link href="/Admin/css/custom.css" rel="stylesheet">
    <link rel="stylesheet" type="text/css" href="/Admin/css/maps/jquery-jvectormap-2.0.3.css" />
    <link href="/Admin/css/icheck/flat/green.css" rel="stylesheet" />
    <link href="/Admin/css/floatexamples.css" rel="stylesheet" type="text/css" />
    <script src="/Admin/js/jquery.min.js"></script>
    <script src="/Admin/js/nprogress.js"></script>
    
     <script src="https://code.jquery.com/ui/1.13.1/jquery-ui.js"></script>
     <link rel="stylesheet" href="//code.jquery.com/ui/1.13.1/themes/base/jquery-ui.css">
    <link href="/Admin/css/select/bootstrap-combobox.css" rel="stylesheet">
    <!--[if lt IE 9]>
        <script src="../assets/js/ie8-responsive-file-warning.js"></script>
        <![endif]-->
    <!-- HTML5 shim and Respond.js for IE8 support of HTML5 elements and media queries -->
    <!--[if lt IE 9]>
          <script src="https://oss.maxcdn.com/html5shiv/3.7.2/html5shiv.min.js"></script>
          <script src="https://oss.maxcdn.com/respond/1.4.2/respond.min.js"></script>
        <![endif]-->
    <style>
        body {
            background-color: #fff;
        }

        .btn-primary {
            color: #fff;
            background-color: #25a0da;
            border-color: #003565;
            min-width: 200px;
        }
    </style>
    <script type="text/javascript">
        function GetRadWindow() {
            var oWindow = null;
            if (window.radWindow) oWindow = window.radWindow;
            else if (window.frameElement.radWindow) oWindow = window.frameElement.radWindow;
            return oWindow;
        }


    </script>

</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager runat="server" ID="ScriptManager1" EnableScriptGlobalization="True">
        </asp:ScriptManager>
        <div runat="server" >
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
                                        <asp:TextBox ID="TxtId" Style="visibility: hidden" Width="400" runat="server" Text="0"></asp:TextBox>
                                    </div>

                                </div>
                                


                                <div class="form-group">
                                    <label class="control-label col-md-1 col-sm-1 col-xs-1">
                                        Note
                                    </label>
                                    <div class="col-md-9 col-sm-8 col-xs-12">
                                        <DtControl:Txt ID="Txt2" DataField="message" TextMode="MultiLine" Required="true" Rows="5" runat="server" />
                                    </div>
                                </div>

                                <div class="form-group">

                                    <div class="col-md-6 col-sm-6 col-xs-12 col-md-offset-3">

                                        <asp:Button ID="btnCancel" CssClass="btn btn-upload submit" Text="Annulla" runat="server" Visible="false"/>
                                        <asp:Button ID="btnSave" CssClass="btn btn-primary submit" Text="Salva" runat="server" />
                                    </div>
                                </div>
                               
                                    
                                      
                                  
                                 
                                <div class="ln_solid">
                                </div>
                                <div class="form-group">
                                    <div class="col-md-6 col-sm-6 col-xs-12 col-md-offset-3">
                                        <div runat="server" id="Div_Error" class="alert alert-warning alert-dismissible fade in" role="alert" visible="false">
                                            Enter all the required fields
                                        </div>
                                        <div runat="server" id="Div_Warning" class="alert alert-warning alert-dismissible fade in" role="alert" visible="false">
                                            Enter all fields correctly
                                        </div>
                                        <div runat="server" id="Div_Terminated" class="alert alert-success alert-dismissible fade in" role="alert" visible="false">
                                            Operation Finished
                                        </div>
                                        <div style="visibility:hidden">
                                          <DtControl:Txt ID="TxtType" DataField="type"  runat="server" /> 
                                        <DtControl:Txt ID="TxtIdProspect" DataField="Id_Prospect"  runat="server" /></div>
                                        <br />
                                        <br />
                                    </div>
                                </div>

                            </div>
                        </div>
                    </div>
                </div>


            </div>
        </div>
    </form>
</body>
</html>
