Namespace Controls

    Partial Class Control_TxtDate
        Inherits System.Web.UI.UserControl

        Private _MaxLenght As String
        Public Property MaxLenght() As Integer
            Get
                Return _MaxLenght
            End Get
            Set(ByVal value As Integer)
                _MaxLenght = value
                TxtVal.MaxLength = value
            End Set
        End Property

        Private _Numeric As Boolean
        Public Property Numeric() As Boolean
            Get
                Return _Numeric
            End Get
            Set(ByVal value As Boolean)
                _Numeric = value
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

        Private _Text As Date
        Public Property Text() As Date
            Get
                If IsDate(TxtVal.Text) Then
                    Return TxtVal.Text
                Else
                    Return Nothing
                End If
            End Get
            Set(ByVal value As Date)
                TxtVal.Text = value
            End Set
        End Property

        Private _Required As Boolean
        Public Property Required() As Boolean
            Get
                Return _Required
            End Get
            Set(ByVal value As Boolean)
                _Required = value
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

        Private _ReadOnly As Boolean = True
        Public Property Read_Only() As Boolean
            Get
                Return _ReadOnly
            End Get
            Set(ByVal value As Boolean)
                _ReadOnly = value
                TxtVal.ReadOnly = _ReadOnly
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
            TxtVal.TextMode = _TextMode
            TxtVal.Enabled = _Enabled
            TxtVal.Rows = Rows
            If Not (IsPostBack) Then
                If _Mask <> "" Then
                    TxtVal.Attributes.Add("onkeydown", "javascript:return dFilter (event.keyCode, this, '" & _Mask & "');")
                End If
            End If
        End Sub
    End Class 
End Namespace