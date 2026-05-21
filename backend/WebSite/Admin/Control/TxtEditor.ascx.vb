Namespace Controls


    Partial Class Control_TxtEditor
        Inherits System.Web.UI.UserControl


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
                Return TxtVal.Content
            End Get
            Set(ByVal value As String)
                TxtVal.Content = value
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

        Private _Mask As String = ""
        Public Property Mask() As String
            Get
                Return _Mask
            End Get
            Set(ByVal value As String)
                _Mask = value
            End Set
        End Property

        Private _TextMode As TextBoxMode
        Public Property TextMode() As TextBoxMode
            Get
                Return _TextMode
            End Get
            Set(ByVal value As TextBoxMode)
                _TextMode = value
            End Set
        End Property

        Private _Enabled As Boolean = True
        Public Property Enabled() As Boolean
            Get
                Return _Enabled
            End Get
            Set(ByVal value As Boolean)
                _Enabled = value
                TxtVal.Enabled = _Enabled
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

        Private _Rows As Int32 = 1
        Public Property Rows() As Int32
            Get
                Return _Rows
            End Get
            Set(ByVal value As Int32)
                _Rows = value
            End Set
        End Property

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
            TxtVal.Enabled = _Enabled
            TxtVal.Height = _Height
            TxtVal.Width = _Width
            If Not (IsPostBack) Then
                If _Mask <> "" Then
                    TxtVal.Attributes.Add("onkeydown", "javascript:return dFilter (event.keyCode, this, '" & _Mask & "');")
                End If
            End If
        End Sub
    End Class
End Namespace