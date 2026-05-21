<%@ Control Language="VB" AutoEventWireup="false" CodeFile="TxtEditor.ascx.vb" Inherits="Controls.Control_TxtEditor" %>
<telerik:RadEditor ToolsFile="~/css/FullSetOfTools.xml" ID="TxtVal" runat="server">
    <Tools>
        <telerik:EditorToolGroup Tag="FileManagers">
            <telerik:EditorTool Name="ImageManager"></telerik:EditorTool>
            <telerik:EditorTool Name="FlashManager"></telerik:EditorTool>
            <telerik:EditorTool Name="SilverlightManager"></telerik:EditorTool>
            <telerik:EditorTool Name="MediaManager"></telerik:EditorTool>
            <telerik:EditorTool Name="DocumentManager"></telerik:EditorTool>
            <telerik:EditorTool Name="TemplateManager"></telerik:EditorTool>
        </telerik:EditorToolGroup>
        <telerik:EditorToolGroup>
            <telerik:EditorTool Name="Bold"></telerik:EditorTool>
            <telerik:EditorTool Name="Italic"></telerik:EditorTool>
            <telerik:EditorTool Name="Underline"></telerik:EditorTool>
            <telerik:EditorSeparator></telerik:EditorSeparator>
            <telerik:EditorTool Name="ForeColor"></telerik:EditorTool>
            <telerik:EditorTool Name="BackColor"></telerik:EditorTool>
            <telerik:EditorSeparator></telerik:EditorSeparator>
            <telerik:EditorTool Name="FontName"></telerik:EditorTool>
            <telerik:EditorTool Name="RealFontSize"></telerik:EditorTool>
        </telerik:EditorToolGroup>
    </Tools>
    <ImageManager ViewPaths="~/Repository/Articles/Images"
        UploadPaths="~/Repository/Articles/Images"
        DeletePaths="~/Repository/Articles/Images"
        EnableAsyncUpload="true" />
    <FlashManager ViewPaths="~/Repository/Articles/Images"
        UploadPaths="~/Repository/Articles/Images"
        DeletePaths="~/Repository/Articles/Images"
        EnableAsyncUpload="true" />
    <MediaManager ViewPaths="~/Repository/Articles/Images"
        UploadPaths="~/Repository/Articles/Images"
        DeletePaths="~/Repository/Articles/Images"
        EnableAsyncUpload="true" />
    <DocumentManager ViewPaths="~/Repository/Articles/Images"
        UploadPaths="~/Repository/Articles/Images"
        DeletePaths="~/Repository/Articles/Images"
        EnableAsyncUpload="true" />
    <TemplateManager ViewPaths="~/Repository/Articles/Images"
        UploadPaths="~/Repository/Articles/Images"
        DeletePaths="~/Repository/Articles/Images"
        EnableAsyncUpload="true" />
    <SilverlightManager ViewPaths="~/Repository/Articles/Images"
        UploadPaths="~/Repository/Articles/Images"
        DeletePaths="~/Repository/Articles/Images"
        EnableAsyncUpload="true" />
    <CssFiles>
        <telerik:EditorCssFile Value="~/css/editorstyle.css" />
    </CssFiles>
</telerik:RadEditor>
<asp:Label ID="lblObbl" ForeColor="Red" Visible="false" runat="server">*</asp:Label>