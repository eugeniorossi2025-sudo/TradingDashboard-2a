Imports System.Data
Imports System.Data.SqlClient
Public Class Users

    Public Enum UsersError
        DuplicateEmail = 7
        DuplicateUserName = 6
        InvalidEmail = 5
        InvalidPassword = 2
        InvalidUserName = 1
        ProviderError = 11
        UserRejected = 8
    End Enum

    Public Shared Function ValidateUser(ByVal UserName As String, ByVal Password As String, oCon As SqlConnection) As Boolean
        Try
            Dim Cmd_User As New SqlCommand
           
            With Cmd_User
                .CommandTimeout = 100000
                .Connection = oCon
                .CommandText = "Ups_Users_Login"
                .CommandType = Data.CommandType.StoredProcedure
                Data.SqlClient.SqlCommandBuilder.DeriveParameters(Cmd_User)
                .Parameters("@Username").Value = UserName
                .Parameters("@Password").Value = Password
                .ExecuteNonQuery()
                If .Parameters("@cnt").Value > 0 Then
                    Return True
                Else
                    Return False
                End If

            End With
        Catch ex As Exception
            Log("Users.ValidateUser", ex.Message & " - " & ex.StackTrace)
            Return False
        End Try
    End Function

    Public Shared Function ChangePassword(ByVal UserName As String, ByVal OldPassword As String, ByVal NewPassword As String, ByVal Otrans As SqlClient.SqlTransaction) As Boolean
        Try
            Return True
        Catch ex As Exception
            Log("Users.ChangePassword", ex.Message & " - " & ex.StackTrace)
            Return False
        End Try
    End Function

    Public Shared Function Create(ByVal EMail As String, ByVal Password As String, ByRef RegStatus As Integer, Otran As SqlTransaction) As Boolean
        Try
            Dim Cmd_User As New SqlCommand
            Dim Da As New SqlDataAdapter
            Dim Dt As New DataTable

            With Cmd_User
                .CommandTimeout = 100000
                .Connection = Otran.Connection
                .Transaction = Otran
                .CommandText = "upI_Users_Check"
                .CommandType = Data.CommandType.StoredProcedure
                Data.SqlClient.SqlCommandBuilder.DeriveParameters(Cmd_User)
                .Parameters("@Email").Value = EMail
                .Parameters("@Password").Value = Password
                .ExecuteNonQuery()
                RegStatus = .Parameters("@Error_Ret").Value()
                If RegStatus = 0 Then
                    Return True
                Else
                    Return False
                End If

            End With
        Catch ex As Exception
            Log("Users.ValidateUser", ex.Message & " - " & ex.StackTrace)
            Return False
        End Try
    End Function
End Class
