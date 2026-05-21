
Namespace Controls

    Partial Class Control_Chk
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

        Private _Value As String
        Public Property Value() As String
            Get
                Return ChkVal.Checked
            End Get
            Set(ByVal value As String)
                ChkVal.Checked = value
            End Set
        End Property
        Private _Enabled As Boolean = True
        Public Property Enabled() As Boolean
            Get
                Return _Enabled
            End Get
            Set(ByVal value As Boolean)
                _Enabled = value
            End Set
        End Property

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
            ChkVal.Enabled = _Enabled
        End Sub

    End Class
End Namespace