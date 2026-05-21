Imports Microsoft.VisualBasic
Imports System.Data.SqlClient
Imports System.Data.SqlTypes
Imports System.Net.Mail
Imports System.Data
Imports Utility

Public Module Database

    Public Const CONSTBlankTextField = "seleziona ....."

    Private connString As System.Configuration.ConnectionStringSettings = Web.Configuration.WebConfigurationManager.ConnectionStrings("CmsConnectionString")


    Public Function GetConn() As SqlConnection
        Dim Conn As New SqlConnection
        Conn = New SqlConnection(connString.ConnectionString)
        Conn.Open()
        Dim Cm As SqlCommand = Nothing
        Cm = New SqlCommand("SET LANGUAGE Italian", Conn)
        Cm.ExecuteNonQuery()
        Cm.Dispose()
        Return Conn
    End Function

    Function SessionGetConn() As SqlConnection
        ' 1. Definisci la chiave da usare in Session
        Const SESSION_KEY_CONN As String = "DBCONNECTION"

        Dim conn As SqlConnection

        ' 2. Verifica se la connessione esiste già in Sessione
        If HttpContext.Current.Session(SESSION_KEY_CONN) IsNot Nothing Then
            ' La connessione esiste in Sessione
            conn = TryCast(HttpContext.Current.Session(SESSION_KEY_CONN), SqlConnection)

            ' Verifica che la connessione sia ancora aperta o in uno stato utilizzabile
            If conn IsNot Nothing AndAlso conn.State = ConnectionState.Open Then
                ' La connessione è valida e aperta, la restituiamo.
                Return conn
            End If

            ' Se è in Sessione ma non è aperta (es. è stata chiusa in precedenza o ha fallito), 
            ' la rimuoviamo per ricrearla.
            HttpContext.Current.Session.Remove(SESSION_KEY_CONN)
        End If

        ' 3. Se non esiste o non è valida, creala, aprila e salvala
        Try
            conn = New SqlConnection(connString.ConnectionString)
            conn.Open() ' Apri la connessione
            Dim Cm As SqlCommand = Nothing
            Cm = New SqlCommand("SET LANGUAGE Italian", conn)
            Cm.ExecuteNonQuery()
            Cm.Dispose()
            ' Salva la connessione aperta in Sessione per l'uso futuro
            HttpContext.Current.Session(SESSION_KEY_CONN) = conn

            Return conn

        Catch ex As Exception
            ' Gestione dell'errore di connessione
            Throw New ApplicationException("Errore durante la creazione o l'apertura della connessione al database.", ex)
        End Try

    End Function

    Public Function SqlDate(ByVal Value As Date) As String
        SqlDate = "CONVERT(DATETIME, '" & Format(Value.Year, "00") & "-" & Format(Value.Month, "00") & "-" & Format(Value.Day, "00") & " 00:00:00', 102)"
    End Function

    Public Function SqlBool(ByVal Value As Boolean) As String
        SqlBool = IIf(Value, 1, 0)
    End Function
    Public Function DbtoStr(ByVal Value) As String
        If (IsDBNull(Value)) Then
            DbtoStr = ""
        Else
            DbtoStr = Value
        End If
    End Function

    Public Function GetParameter(ByVal ParameterName As String, ByVal Value As Object, ByVal DBType As SqlDbType) As SqlParameter
        Dim Par As New SqlParameter(ParameterName, DBType)
        If Value IsNot Nothing Then
            If Value.GetType Is GetType(Date) Then
                If Value = New Date Then
                    Par.Value = DBNull.Value
                Else
                    Par.Value = Value
                End If
            Else
                Par.Value = Value
            End If
        Else
            Par.Value = DBNull.Value
        End If
        Return Par
    End Function
    Public Sub AddParameter(ByVal ParameterName As String, ByVal Value As Object, ByVal DBType As SqlDbType, ByVal Cm As SqlCommand)
        Cm.Parameters.Add(GetParameter(ParameterName, Value, DBType))
    End Sub


    Public Function ExecuteSQL(ByVal sSql As String, ByVal ParColl As List(Of SqlParameter), ByVal Tr As SqlTransaction) As Integer
        Dim Cm As SqlCommand = Nothing

        Try
            Cm = New SqlCommand(sSql, Tr.Connection, Tr)

            If ParColl IsNot Nothing Then
                Cm.Parameters.Clear()
                Cm.Parameters.AddRange(ParColl.ToArray)
            End If

            Return Cm.ExecuteNonQuery()
        Catch ex As Exception
            Throw ex
        Finally
            If Cm IsNot Nothing Then Cm.Dispose()
        End Try
    End Function
    Public Function ExecuteSQL(ByVal sSql As String, ByVal ParColl As List(Of SqlParameter), Optional ByVal oConn As SqlConnection = Nothing) As Integer
        Dim Cn As SqlConnection = Nothing
        Dim Tr As SqlTransaction = Nothing
        Dim Ret As Integer
        Try
            If oConn Is Nothing Then
                Cn = GetConn()
            Else
                Cn = oConn
            End If
            Tr = Cn.BeginTransaction
            Ret = ExecuteSQL(sSql, ParColl, Tr)
            Tr.Commit()
            Return Ret
        Catch ex As Exception
            Tr.Rollback()
            Throw ex
        Finally
            If Tr IsNot Nothing Then Tr.Dispose()
            If oConn Is Nothing Then
                Cn.Close()
                Cn.Dispose()
            End If
        End Try
    End Function
    Public Function ExecuteSQL(ByVal sSql As String) As Integer
        Return ExecuteSQL(sSql, Nothing)
    End Function

    Public Function GetScalar(ByVal sSql As String, ByVal ParColl As List(Of SqlParameter), ByVal Tr As SqlTransaction) As Object
        Dim Cm As SqlCommand = Nothing

        Try
            Cm = New SqlCommand(sSql, Tr.Connection, Tr)

            If ParColl IsNot Nothing Then
                Cm.Parameters.Clear()
                Cm.Parameters.AddRange(ParColl.ToArray)
            End If

            Return Cm.ExecuteScalar
        Catch ex As Exception
            Throw ex
        Finally
            If Cm IsNot Nothing Then Cm.Dispose()
        End Try
    End Function
    Public Function GetScalar(ByVal sSql As String, ByVal ParColl As List(Of SqlParameter), Optional ByVal oConn As SqlConnection = Nothing) As Object
        Dim Cn As SqlConnection = Nothing
        Dim Tr As SqlTransaction = Nothing
        Dim Ret As Object
        Try
            If oConn Is Nothing Then
                Cn = GetConn()
            Else
                Cn = oConn
            End If
            Tr = Cn.BeginTransaction
            Ret = GetScalar(sSql, ParColl, Tr)
            Tr.Commit()
            Return Ret
        Catch ex As Exception
            Tr.Rollback()
            Throw ex
        Finally
            If Tr IsNot Nothing Then Tr.Dispose()
            If oConn Is Nothing Then
                Cn.Close()
                Cn.Dispose()
            End If
        End Try
    End Function
    Public Function GetScalar(ByVal sSql As String) As Object
        Return GetScalar(sSql, Nothing)
    End Function

    Public Function GetDs(ByVal sSql As String, ByVal ParColl As List(Of SqlParameter), ByVal Tr As SqlTransaction) As DataSet
        Dim Cm As SqlCommand = Nothing
        Dim Da As SqlDataAdapter = Nothing
        Dim Ds As DataSet

        Try
            Cm = New SqlCommand(sSql, Tr.Connection, Tr)

            If ParColl IsNot Nothing Then
                Cm.Parameters.Clear()
                Cm.Parameters.AddRange(ParColl.ToArray)
            End If

            Da = New SqlDataAdapter(Cm)
            Ds = New DataSet
            Da.Fill(Ds)

            Return Ds
        Catch ex As Exception
            Throw ex
        Finally
            If Da IsNot Nothing Then Da.Dispose()
            If Cm IsNot Nothing Then Cm.Dispose()
        End Try
    End Function
    Public Function GetDs(ByVal sSql As String, ByVal ParColl As List(Of SqlParameter), Optional ByVal oConn As SqlConnection = Nothing) As DataSet
        Dim Cn As SqlConnection = Nothing
        Dim Tr As SqlTransaction = Nothing
        Dim Ret As DataSet
        Try
            If oConn Is Nothing Then
                Cn = GetConn()
            Else
                Cn = oConn
            End If
            Tr = Cn.BeginTransaction
            Ret = GetDs(sSql, ParColl, Tr)
            Tr.Commit()
            Return Ret
        Catch ex As Exception
            Tr.Rollback()
            Throw ex
        Finally
            If Tr IsNot Nothing Then Tr.Dispose()
            If oConn Is Nothing Then
                Cn.Close()
                Cn.Dispose()
            End If
        End Try
    End Function

    Public Function GetDs(ByVal sSql As String) As DataSet
        Return GetDs(sSql, Nothing)
    End Function

    Public Function GetDt(ByVal sSql As String, Optional ByVal oConn As SqlConnection = Nothing) As DataTable
        Dim Ds As DataSet
        Ds = GetDs(sSql, Nothing, oConn)
        Return Ds.Tables(0)
    End Function

    Public Function GetDbValue(ByVal Value As Object) As Object
        If Value Is DBNull.Value Then Return Nothing
        Return Value
    End Function

    Public Function GetEmptyDs(ByVal DataValueType As Type) As DataSet
        Dim Ds As New DataSet
        Dim Dt As New DataTable
        Dt.Columns.Add("Id", DataValueType)
        Dt.Columns.Add("Description", GetType(String))
        Ds.Tables.Add(Dt)
        Ds.AcceptChanges()
        Return Ds
    End Function


    Public Function CopyRecord(ByVal TableNameFrom As String, ByVal TableNameDestination As String, ByVal strAfter As String, ByVal DescriptionField As String, ByVal IdRecordFrom As Integer, ByVal mod_type As String, ByVal IsLog As Boolean, Optional ByVal oConn As SqlClient.SqlConnection = Nothing) As Integer

        Dim Cmd As New Data.SqlClient.SqlCommand
        Dim Dt As New Data.DataTable()
        Dim Da As New System.Data.SqlClient.SqlDataAdapter()
        Dim SqlInsert As String = ""
        Dim continua As Boolean
        Dim Con As SqlClient.SqlConnection = Nothing
        If oConn Is Nothing Then
            Con = GetConn()
        Else
            Con = oConn
        End If
        With Cmd

            .Connection = Con
            .CommandText = "Ups_RetrieveTableField"
            .CommandType = Data.CommandType.StoredProcedure
            Data.SqlClient.SqlCommandBuilder.DeriveParameters(Cmd)
            .Parameters("@DatabaseName").Value = .Connection.Database
            .Parameters("@TableName").Value = TableNameFrom
        End With


        Da.SelectCommand = Cmd
        Da.Fill(Dt)

        For Each oRow As DataRow In Dt.Rows
            If IsLog = False And VerString(oRow("COLUMN_NAME")).ToLower = "id" Then
                continua = False
            Else
                continua = True
            End If
            If continua Then
                If SqlInsert <> "" Then
                    SqlInsert += ","
                End If
                SqlInsert += VerString(oRow("COLUMN_NAME"))
            End If
        Next

        '****
        'Dim Dp As Integer

        'Dp = DatePart(DateInterval.WeekOfYear, Now)
        'Dim err As String = "insert into " & TableNameDestination & " (" & SqlInsert & ") select " & SqlInsert & " from " & TableNameFrom & " where id=" & IdRecordFrom & "  SELECT SCOPE_IDENTITY()" & System.Environment.NewLine
        'Dim aPath As String = ConfigurationSettings.AppSettings("Path.Log")
        'aPath = HttpContext.Current.Server.MapPath(aPath)
        'System.IO.File.AppendAllText(aPath & "\log_" & Year(Now) & "_" & Dp.ToString() & ".txt", err)
        '*****
        Dim IdRecord As Integer = GetScalar("insert into " & TableNameDestination & " (" & SqlInsert & ") select " & SqlInsert & " from " & TableNameFrom & " where id=" & IdRecordFrom & "  SELECT SCOPE_IDENTITY()", Nothing, oConn)
        If IsLog = False Then
            'duplicazione
            ExecuteSQL("update " & TableNameDestination & " set " & DescriptionField & " = " & DescriptionField + " + '" & strAfter & "' where id=" & IdRecord, Nothing, oConn)
        Else
            If DescriptionField = "" Then
                ExecuteSQL("update " & TableNameDestination & " set  mod_ute='" & RetUserInfo("ID") & "',mod_type='" & mod_type & "' where id_log=" & IdRecord, Nothing, oConn)
            Else
                ExecuteSQL("update " & TableNameDestination & " set " & DescriptionField & " = " & DescriptionField + " + '" & strAfter & "',mod_ute='" & RetUserInfo("ID") & "',mod_type='" & mod_type & "' where id_log=" & IdRecord, Nothing, oConn)
            End If
        End If

        Cmd.Dispose()
        If oConn Is Nothing Then
            Con.Close()
            Con.Dispose()
        End If

        Return IdRecord
    End Function
End Module
