Namespace Controls


    Partial Class Control_Drp
        Inherits System.Web.UI.UserControl
        Public Event SelectedIndexChanged As System.EventHandler
 

        Private _WhereCond As String
        Public Property WhereCond() As String
            Get
                Return _WhereCond
            End Get
            Set(ByVal value As String)
                _WhereCond = value
            End Set
        End Property

        Public ReadOnly Property ItemsCount() As Integer
            Get
                Return TxtVal.Items.Count
            End Get
        End Property

        Private _IdField As String
        Public Property IdField() As String
            Get
                Return _IdField
            End Get
            Set(ByVal value As String)
                _IdField = value
            End Set
        End Property

        Private _NoUpdate As Boolean = False
        Public Property NoUpdate() As Boolean
            Get
                Return _NoUpdate
            End Get
            Set(ByVal value As Boolean)
                _NoUpdate = value

            End Set
        End Property

        Private _DescrField As String
        Public Property DescrField() As String
            Get
                Return _DescrField
            End Get
            Set(ByVal value As String)
                _DescrField = value
            End Set
        End Property

        Private _TableName As String
        Public Property TableName() As String
            Get
                Return _TableName
            End Get
            Set(ByVal value As String)
                _TableName = value
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

        Private _UnSorted As Boolean
        Public Property UnSorted() As Boolean
            Get
                Return _UnSorted
            End Get
            Set(ByVal value As Boolean)
                Try
                    _UnSorted = value
                Catch
                End Try
            End Set
        End Property

        Private _Text As String
        Public Property Text() As String
            Get
                Return TxtVal.SelectedValue
            End Get
            Set(ByVal value As String)
                Try
                    TxtVal.SelectedValue = value
                Catch
                End Try
            End Set
        End Property

        Private _Text2 As String
        Public ReadOnly Property Text2() As String
            Get
                Return TxtVal.SelectedItem.Text
            End Get
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

        Protected Sub TxtVal_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles TxtVal.SelectedIndexChanged
            RaiseEvent SelectedIndexChanged(sender, e)
        End Sub

        Private _Autopostback As Boolean
        Public Property Autopostback() As String
            Get
                Return _Autopostback
            End Get
            Set(ByVal value As String)
                _Autopostback = value
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
                BtnAddNew.Enabled = _Enabled
                BtnEdit.Enabled = _Enabled
                BtnDelete.Enabled = _Enabled
            End Set
        End Property

       
        Public Sub Rebind()
            TxtVal.DataSource = Nothing
            LoadDropDown(TxtVal, "Select " & _IdField & " as Id," & _DescrField & " as Description from " & _TableName & " " & _WhereCond & IIf(_UnSorted, "", " ORDER BY Description"), Nothing, True)
        End Sub

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
            TxtVal.Enabled = _Enabled
            

            TxtVal.AutoPostBack = _Autopostback
            If Enabled Then
                BtnAddNew.Visible = Not (_NoUpdate)
                BtnEdit.Visible = Not (_NoUpdate)
                BtnDelete.Visible = Not (_NoUpdate)
            Else
                BtnAddNew.Visible = False
                BtnEdit.Visible = False
                BtnDelete.Visible = False
            End If
            If Not IsPostBack Then
                TxtVal.DataSource = Nothing
                LoadDropDown(TxtVal, "Select " & _IdField & " as Id," & _DescrField & " as Description from " & _TableName & " " & _WhereCond & IIf(_UnSorted, "", " ORDER BY Description"), Nothing, True)
            End If
        End Sub

        Protected Sub BtnDelete_Click(ByVal sender As Object, ByVal e As System.Web.UI.ImageClickEventArgs) Handles BtnDelete.Click
            If TxtVal.SelectedItem.Text = "" Then Exit Sub

           
            ExecuteSQL("delete from  " & _TableName & " where " & _IdField & " = " & TxtVal.SelectedValue)
            LoadDropDown(TxtVal, "Select " & _IdField & " as Id," & _DescrField & " as Description from " & _TableName & " " & _WhereCond & IIf(_UnSorted, "", " ORDER BY Description"), Nothing, True)
        End Sub

        Protected Sub BtnSave_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles BtnSave.Click
            TxtVal.Enabled = True
            DivEdit.Visible = False

            If LblOperazione.Text = "0" Then 'se aggiornamento
                ExecuteSQL("Update " & _TableName & " set " & _DescrField & " = '" & TxtEdit.Text.Replace("'", "''") & "' where " & _IdField & " = " & LblId.Text)

                'aggiorno il controllo
                LoadDropDown(TxtVal, "Select " & _IdField & " as Id," & _DescrField & " as Description from " & _TableName & " " & _WhereCond & IIf(_UnSorted, "", " ORDER BY Description"), Nothing, True, LblId.Text)

            Else
                Dim Idvalue As Integer = GetScalar("Insert into " & _TableName & " (" & _DescrField & ") values ('" & TxtEdit.Text.Replace("'", "''") & "')" + " SELECT SCOPE_IDENTITY()")


                'aggiorno il controllo
                LoadDropDown(TxtVal, "Select " & _IdField & " as Id," & _DescrField & " as Description from " & _TableName & " " & _WhereCond & IIf(_UnSorted, "", " ORDER BY Description"), Nothing, True, Idvalue)

            End If


            'imposto il controllo con tale valore
            'TxtVal.SelectedIndex = 0

            'LblId.Text = 0
            'TxtEdit.Text = ""
        End Sub

        Protected Sub BtnEdit_Click(ByVal sender As Object, ByVal e As System.Web.UI.ImageClickEventArgs) Handles BtnEdit.Click

            If TxtVal.SelectedItem.Text = "" Then Exit Sub

            TxtVal.Enabled = False
            DivEdit.Visible = True
            LblId.Text = TxtVal.SelectedValue
            TxtEdit.Text = TxtVal.SelectedItem.Text
            LblOperazione.Text = "0" 'aggiornamento

        End Sub

        Protected Sub BtnAddNew_Click(ByVal sender As Object, ByVal e As System.Web.UI.ImageClickEventArgs) Handles BtnAddNew.Click
            TxtVal.Enabled = False
            DivEdit.Visible = True
            LblId.Text = 0
            TxtEdit.Text = ""
            LblOperazione.Text = "1" 'inserimento
        End Sub

        Protected Sub BtnCancel_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles BtnCancel.Click
            DivEdit.Visible = False
            LblId.Text = 0
            TxtEdit.Text = ""
        End Sub
    End Class
End Namespace