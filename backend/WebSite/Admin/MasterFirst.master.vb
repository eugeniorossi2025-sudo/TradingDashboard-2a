
Imports System.Data
Imports System.Data.SqlClient

Partial Class MasterFirst
    Inherits System.Web.UI.MasterPage

    Protected Sub Lnk_SignOut_Click(sender As Object, e As System.EventArgs) Handles Lnk_SignOut.Click
        SignOut()
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles Form1.Load
        'Dim Cmd As New Data.SqlClient.SqlCommand
        'Dim Dt As New Data.DataTable()
        'Dim Da As New System.Data.SqlClient.SqlDataAdapter()
        'Dim oCon As SqlClient.SqlConnection

        'oCon = GetConn()
        'With Cmd
        '    .Connection = oCon
        '    .CommandText = "Ups_Prospect_Messages"
        '    .CommandType = Data.CommandType.StoredProcedure
        '    SqlCommandBuilder.DeriveParameters(Cmd)
        '    .Parameters("@id_user").Value = Utility.VerInt32(RetUserInfo("ID"))
        '    .Parameters("@bit_read").Value = 0
        'End With

        'Da.SelectCommand = Cmd
        'Da.Fill(Dt)
        'oCon.Close()
        'oCon.Dispose()
        'Rp_Messages.DataSource = Dt
        'Rp_Messages.DataBind()


        'LblCount.Text = Dt.Rows.Count
    End Sub
End Class

