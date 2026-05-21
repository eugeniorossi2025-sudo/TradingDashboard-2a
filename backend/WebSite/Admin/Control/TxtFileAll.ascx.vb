Imports System.IO
Imports System.String
Imports Telerik.Web.UI
Namespace Controls



    Partial Class Control_TxtFileAll
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

        Private _NomeFile As String
        Public Property NomeFile() As String
            Get
                _NomeFile = TxtFileName.Text
                Return _NomeFile
            End Get
            Set(ByVal value As String)
                _NomeFile = value
                TxtFileName.Text = _NomeFile
            End Set
        End Property

        Private _Text As String
        Public Property Text() As String
            Get
                Return TxtVal.Text
            End Get
            Set(ByVal value As String)
                TxtVal.Text = value
                LblFile.Text = value
                Lnk_File.NavigateUrl = "/Repository/UserFiles/" & value
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
        Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
            TxtVal.Enabled = _Enabled
            btnUploadDoc.Visible = _Enabled
            fileupDoc.Visible = _Enabled
            btnUploadDoc.Visible = _Enabled
            TxtVal.Height = _Height
            TxtVal.Width = _Width
        End Sub


        Public Sub UploadFileTemp(ByVal objFileUpload As FileUpload, ByVal sPath As String, ByVal sFileName As String)

            If Not objFileUpload.HasFile Then
                Me.lblErr.Text = "Selezionare un file da caricare"
            Else
                Dim filename As String = Me.Server.MapPath(sPath & "/" & sFileName)
                objFileUpload.SaveAs(filename)
                TxtVal.Text = sFileName
                LblFile.Text = TxtVal.Text
                Lnk_File.NavigateUrl = "/Repository/UserFiles/" & TxtVal.Text
            End If

        End Sub

        Protected Sub buttonSubmit_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnUploadDoc.Click

            Dim newguid As Guid
            Dim newfilename As String
            newguid = Guid.NewGuid
            If fileupDoc.FileName <> "" Then
                TxtFileName.Text = fileupDoc.FileName
                newfilename = newguid.ToString & Mid(fileupDoc.FileName, InStrRev(fileupDoc.FileName, "."))
                Me.UploadFileTemp(Me.fileupDoc, _Path, newfilename)
            End If
        End Sub

    End Class
End Namespace