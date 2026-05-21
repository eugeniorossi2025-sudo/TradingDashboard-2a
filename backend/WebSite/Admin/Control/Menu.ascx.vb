
Partial Class Control_Menu
    Inherits System.Web.UI.UserControl


    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim WhereCond As String = ""
        Dim WhereCondMeeting As String = ""
        Dim oCon As System.Data.SqlClient.SqlConnection = GetConn()
        Dim Dt As System.Data.DataTable



        If Utility.VerBool(RetUserInfo("Administrator")) Then
            WhereCond = " and IsAdmin=1"
        Else
            WhereCond = " and IsAdmin=0"
        End If


        If Not Me.IsPostBack Then


            'ADMINISTRATOR
            Dt = GetDt("SELECT  * from BOMenu  where id_menu=1 and bit_visible=1  " & WhereCond & " order by pos_order", oCon)
            If Dt.Rows.Count > 0 Then
                Rp_A1.DataSource = Dt
                Rp_A1.DataBind()
                A1.Visible = True
            Else
                A1.Visible = False
            End If

            'MANAGEMENT
            Dt = GetDt("SELECT  * from BOMenu where id_menu=3 and bit_visible=1  " & WhereCond & " order by pos_order", oCon)
            If Dt.Rows.Count > 0 Then
                Rp_A3.DataSource = Dt
                Rp_A3.DataBind()
                A3.Visible = True
            Else
                A3.Visible = False
            End If

            'OTHER DATA
            Dt = GetDt("SELECT  * from BOMenu where id_menu=6 and bit_visible=1  " & WhereCond & " order by pos_order", oCon)
            If Dt.Rows.Count > 0 Then
                Rp_A6.DataSource = Dt
                Rp_A6.DataBind()
                A6.Visible = True
            Else
                A6.Visible = False
            End If
        End If
        oCon.Close()
        oCon.Dispose()
    End Sub

End Class
