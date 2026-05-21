Imports System.IO
Imports System.String
Imports Telerik.Web.UI
Namespace Controls



    Partial Class Control_TxtFile
        Inherits System.Web.UI.UserControl


        Private _Enabled As Boolean = True
        Public Property Enabled() As Boolean
            Get
                Return _Enabled
            End Get
            Set(ByVal value As Boolean)
                _Enabled = value
            End Set
        End Property


        Private _DataField As String
        Public Property DataField() As String
            Get
                Return _DataField
            End Get
            Set(ByVal value As String)
                _DataField = value
            End Set
        End Property

        Private _Text As String
        Public Property Text() As String
            Get
                Return TxtVal.Text
            End Get
            Set(ByVal value As String)
                TxtVal.Text = value
                ReloadImage()
            End Set
        End Property


        Private _Required As Boolean
        Public Property Required() As Boolean
            Get
                Return _Required
            End Get
            Set(ByVal value As Boolean)
                _Required = value
                lblObbl.Visible = value
            End Set
        End Property

        Private _Width As Int32 = 200
        Public Property Width() As Int32
            Get
                Return _Width
            End Get
            Set(ByVal value As Int32)
                _Width = value
            End Set
        End Property

        Private _Height As Int32 = 15
        Public Property Height() As Int32
            Get
                Return _Height
            End Get
            Set(ByVal value As Int32)
                _Height = value
            End Set
        End Property

        Private _Path As String
        Public Property Path() As String
            Get
                Return _Path
            End Get
            Set(ByVal value As String)
                _Path = value
            End Set
        End Property

        Public Sub ReloadImage()
            If File.Exists(Server.MapPath(_Path & "/" & TxtVal.Text)) Then
                pvwImage.ImageUrl = _Path & "/" & TxtVal.Text
            Else
                pvwImage.ImageUrl = "/Admin/images/image.png"
            End If
        End Sub

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
            TxtVal.Enabled = _Enabled
            btnUploadLogo.Visible = _Enabled
            fileupLogo.Visible = _Enabled
            btnUploadLogo.Visible = _Enabled
            TxtVal.Height = _Height
            TxtVal.Width = _Width
        End Sub


        Public Sub UploadFileTemp(ByVal objFileUpload As FileUpload, ByVal sPath As String, ByVal sFileName As String, ByVal objImage As Image)

            If Not objFileUpload.HasFile Then
                Me.lblErr.Text = "Select a file to upload"
            Else
                Dim filename As String = Me.Server.MapPath(sPath & "/" & sFileName)
                objFileUpload.SaveAs(filename)
                objImage.ImageUrl = sPath & "/" & sFileName
                TxtVal.Text = sFileName
            End If

        End Sub

        Protected Sub buttonSubmit_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnUploadLogo.Click

            Dim newguid As Guid
            Dim newfilename As String
            newguid = Guid.NewGuid
            newfilename = newguid.ToString & Right(fileupLogo.FileName, 4)
            Me.UploadFileTemp(Me.fileupLogo, _Path, newfilename, Me.pvwImage)
        End Sub

    End Class
End Namespace