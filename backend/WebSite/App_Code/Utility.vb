#Region "Imports"
Imports System
Imports System.Net
Imports System.Diagnostics
Imports System.IO
Imports System.Text
Imports System.Data
#End Region

Public Class Utility

#Region "Shared Methods"


    ''' <date>24/01/2011</date>
    ''' <author>Marco Turri</author>
    ''' <summary>
    ''' Function VerString effettua la conversione di un generico valore
    ''' </summary>
    ''' <returns>String</returns>
    Public Shared Function VerString(ByVal poValue As Object,
                                     Optional ByVal pstrDefaultValue As String = "",
                                     Optional ByVal pblnReplaceCrLf As Boolean = False,
                                     Optional ByVal pblnTrim As Boolean = False) As String
        Dim strResult As String
        strResult = pstrDefaultValue

        If (poValue Is Nothing) Then
            Return pstrDefaultValue
        End If

        If (poValue Is System.DBNull.Value) Then
            Return pstrDefaultValue
        End If

        If String.IsNullOrEmpty(poValue) Then
            Return pstrDefaultValue
        End If

        strResult = poValue.ToString
        If pblnReplaceCrLf Then
            strResult = strResult.Replace(Environment.NewLine, " ")
        End If

        If pblnTrim Then
            strResult = strResult.Trim
        End If

        Return strResult
    End Function

    ''' <date>24/01/2011</date>
    ''' <author>Marco Turri</author>
    ''' <summary>
    ''' Function VerString effettua la conversione di un generico valore
    ''' </summary>
    ''' <returns>String</returns>
    Public Shared Function VerChar(ByVal poValue As Object, Optional ByVal pchrDefaultValue As Char = "") As String
        Dim chrResult As Char
        chrResult = pchrDefaultValue

        If poValue Is Nothing Then
            Return pchrDefaultValue
        End If

        If (poValue Is System.DBNull.Value) Then
            Return pchrDefaultValue
        End If

        If Not Char.TryParse(poValue.ToString, chrResult) Then
            Return pchrDefaultValue
        End If

        Return chrResult
    End Function

    ''' <date>24/01/2011</date>
    ''' <author>Marco Turri</author>
    ''' <summary>
    ''' Function VerInt16 effettua la conversione di un generico valore
    ''' </summary>
    ''' <returns>Int16</returns>
    Public Shared Function VerInt16(ByVal poValue As Object, Optional ByVal pintDefaultValue As Int16 = 0) As Int16
        Dim intResult As Int16
        intResult = pintDefaultValue

        If poValue Is Nothing Then
            Return pintDefaultValue
        End If

        If poValue Is DBNull.Value Then
            Return pintDefaultValue
        End If

        If Not Int16.TryParse(poValue.ToString, intResult) Then
            Return pintDefaultValue
        End If

        Return intResult
    End Function

    ''' <date>24/01/2011</date>
    ''' <author>Marco Turri</author>
    ''' <summary>
    ''' Function VerInt32 effettua la conversione di un generico valore
    ''' </summary>
    ''' <returns>Int32</returns>
    Public Shared Function VerInt32(ByVal poValue As Object, Optional ByVal pintDefaultValue As Int32 = 0) As Int32
        Dim intResult As Int32
        intResult = pintDefaultValue

        If poValue Is Nothing Then
            Return pintDefaultValue
        End If

        If poValue Is DBNull.Value Then
            Return pintDefaultValue
        End If

        If Not Int32.TryParse(poValue.ToString, intResult) Then
            Return pintDefaultValue
        End If

        Return intResult
    End Function

    ''' <date>24/01/2011</date>
    ''' <author>Marco Turri</author>
    ''' <summary>
    ''' Function VerInt64 effettua la conversione di un generico valore
    ''' </summary>
    ''' <returns>Int64</returns>
    Public Shared Function VerInt64(ByVal poValue As Object, Optional ByVal pintDefaultValue As Int64 = 0) As Int64
        Dim intResult As Int64
        intResult = pintDefaultValue

        If poValue Is Nothing Then
            Return pintDefaultValue
        End If

        If poValue Is DBNull.Value Then
            Return pintDefaultValue
        End If

        If Not Int64.TryParse(poValue.ToString, intResult) Then
            Return pintDefaultValue
        End If

        Return intResult
    End Function

    ''' <date>24/01/2011</date>
    ''' <author>Marco Turri</author>
    ''' <summary>
    ''' Function VerDecimal effettua la conversione di un generico valore
    ''' </summary>
    ''' <returns>Decimal</returns>
    Public Shared Function VerDecimal(ByVal poValue As Object, Optional ByVal pdecDefaultValue As Decimal = 0) As Decimal
        Dim decResult As Decimal
        decResult = pdecDefaultValue

        If poValue Is Nothing Then
            Return pdecDefaultValue
        End If

        If poValue Is System.DBNull.Value Then
            Return pdecDefaultValue
        End If

        If Not Decimal.TryParse(poValue.ToString, decResult) Then
            Return pdecDefaultValue
        End If

        Return decResult
    End Function


    '' <date>24/01/2011</date>
    ''' <author>Marco Turri</author>
    ''' <summary>
    ''' Function VerDecimal effettua la conversione di un generico valore
    ''' </summary>
    ''' <returns>Decimal</returns>
    Public Shared Function VerDouble(ByVal poValue As Object, Optional ByVal pdecDefaultValue As Double = 0) As Double
        Dim decResult As Double
        decResult = pdecDefaultValue

        If poValue Is Nothing Then
            Return pdecDefaultValue
        End If

        If poValue Is System.DBNull.Value Then
            Return pdecDefaultValue
        End If

        If Not Double.TryParse(poValue.ToString, decResult) Then
            Return pdecDefaultValue
        End If

        Return decResult
    End Function

    ''' <date>24/01/2011</date>
    ''' <author>Marco Turri</author>
    ''' <summary>
    ''' Function VerInteger effettua la conversione di un generico valore
    ''' </summary>
    ''' <returns>Integer</returns>
    Public Shared Function VerInteger(ByVal poValue As Object, Optional ByVal pintDefaultValue As Integer = 0) As Integer
        Dim intResult As Integer
        intResult = pintDefaultValue

        If poValue Is Nothing Then
            Return pintDefaultValue
        End If

        If poValue Is System.DBNull.Value Then
            Return pintDefaultValue
        End If

        If Not Integer.TryParse(poValue.ToString, intResult) Then
            Return pintDefaultValue
        End If

        Return intResult
    End Function

    ''' <date>24/01/2011</date>
    ''' <author>Marco Turri</author>
    ''' <summary>
    ''' Function VerLong effettua la conversione di un generico valore
    ''' </summary>
    ''' <returns>Long</returns>
    Public Shared Function VerLong(ByVal poValue As Object, Optional ByVal pintDefaultValue As Integer = 0) As Long
        Dim lngResult As Long
        lngResult = pintDefaultValue

        If poValue Is Nothing Then
            Return pintDefaultValue
        End If

        If poValue Is System.DBNull.Value Then
            Return pintDefaultValue
        End If

        If Not Long.TryParse(poValue.ToString, lngResult) Then
            Return pintDefaultValue
        End If

        Return lngResult
    End Function

    ''' <date>24/01/2011</date>
    ''' <author>Marco Turri</author>
    ''' <summary>
    ''' Function VerNull effettua la conversione di un generico valore
    ''' </summary>
    ''' <returns>Object</returns>
    Public Shared Function VerNull(ByVal poValue As Object) As Object
        If (poValue Is Nothing) Then
            Return DBNull.Value
        End If

        If (poValue Is DBNull.Value) Then
            Return DBNull.Value
        End If

        If String.IsNullOrEmpty(VerString(poValue)) Then
            Return DBNull.Value
        End If

        Return poValue
    End Function

    ''' <date>24/01/2011</date>
    ''' <author>Marco Turri</author>
    ''' <summary>
    ''' Function VerBool effettua la conversione di un generico valore
    ''' </summary>
    ''' <returns>Boolean</returns>
    Public Shared Function VerBool(ByVal poValue As Object, Optional ByVal pblnDefaultValue As Boolean = False) As Boolean
        Dim blnResult As Boolean
        blnResult = pblnDefaultValue

        If poValue Is Nothing Then
            Return pblnDefaultValue
        End If

        If poValue Is System.DBNull.Value Then
            Return pblnDefaultValue
        End If

        Select Case poValue.GetType.ToString
            Case "System.Int16", "System.Int32", "System.Integer", "System.Decimal", "System.Int64", "System.Long"
                If (Math.Abs(VerInteger(poValue)) = 1) Then
                    blnResult = True
                ElseIf (Math.Abs(VerInteger(poValue)) = 0) Then
                    blnResult = False
                End If

            Case "System.String"
                If IsNumeric(poValue) Then
                    If (Math.Abs(VerInteger(poValue)) = 1) Then
                        blnResult = True
                    ElseIf (Math.Abs(VerInteger(poValue)) = 0) Then
                        blnResult = False
                    End If
                Else
                    If (poValue.ToString.Trim.ToUpper = "TRUE") Then
                        blnResult = True
                    ElseIf (poValue.ToString.Trim.ToUpper = "FALSE") Then
                        blnResult = False
                    End If
                End If

            Case Else
                If Not Boolean.TryParse(poValue.ToString, blnResult) Then
                    blnResult = pblnDefaultValue
                End If
        End Select

        Return blnResult
    End Function

    ''' <date>24/01/2011</date>
    ''' <author>Marco Turri</author>
    ''' <summary>
    ''' Function VerDate effettua la conversione di un generico valore
    ''' </summary>
    ''' <returns>Date</returns>
    Public Shared Function VerDate(ByVal poValue As Object, Optional ByVal poDefaultValue As Object = Nothing) As Object
        Dim dtResult As Object
        dtResult = Date.MinValue

        If poValue Is Nothing Then
            Return poDefaultValue
        End If

        If poValue Is System.DBNull.Value Then
            Return poDefaultValue
        End If

        If Not IsDate(poValue) Then
            Return poDefaultValue
        End If

        If Not Date.TryParse(poValue.ToString, dtResult) Then
            Return poDefaultValue
        End If

        Return dtResult
    End Function

    ''' <date>24/01/2011</date>
    ''' <author>Marco Turri</author>
    ''' <summary>
    ''' Function VerDate effettua la conversione di un generico valore
    ''' </summary>
    ''' <returns>Date</returns>
    Public Shared Function VerDateTime(ByVal poValue As Object, Optional ByVal poDefaultValue As Object = Nothing) As Object
        Dim dtResult As Object
        dtResult = DateTime.MinValue

        If poValue Is Nothing Then
            Return poDefaultValue
        End If

        If poValue Is System.DBNull.Value Then
            Return poDefaultValue
        End If

        If Not IsDate(poValue) Then
            Return poDefaultValue
        End If

        If Not DateTime.TryParse(poValue.ToString, dtResult) Then
            Return poDefaultValue
        End If

        Return dtResult
    End Function

    ''' <date>24/01/2011</date>
    ''' <author>Marco Turri</author>
    ''' <summary>
    ''' Function VerDateNull effettua la conversione di un generico valore
    ''' </summary>
    ''' <returns>Object</returns>
    Public Shared Function VerDateNull(ByVal poValue As Object) As Object
        Dim dtResult As Date
        dtResult = Date.MinValue

        If poValue Is Nothing Then
            Return DBNull.Value
        End If

        If IsDBNull(poValue) Then
            Return DBNull.Value
        End If

        If Not IsDate(poValue) Then
            Return DBNull.Value
        End If

        If Not Date.TryParse(poValue.ToString(), dtResult) Then
            Return DBNull.Value
        End If

        If (dtResult = Date.MinValue) Then
            Return DBNull.Value
        End If

        Return dtResult
    End Function

    ''' <date>24/01/2011</date>
    ''' <author>Marco Turri</author>
    ''' <summary>
    ''' Function VerDateDB effettua la conversione di un generico valore
    ''' </summary>
    ''' <returns>Object</returns>
    Public Shared Function VerDateDB(ByVal poValue As Object) As SqlTypes.SqlDateTime
        Dim dtResult As DateTime
        dtResult = DateTime.MinValue

        If poValue Is Nothing Then
            Return SqlTypes.SqlDateTime.Null.Value
        End If

        If IsDBNull(poValue) Then
            Return SqlTypes.SqlDateTime.Null.Value
        End If

        If Not IsDate(poValue) Then
            Return SqlTypes.SqlDateTime.Null.Value
        End If

        If Not DateTime.TryParse(poValue.ToString(), dtResult) Then
            Return SqlTypes.SqlDateTime.Null.Value
        End If

        If (dtResult = DateTime.MinValue) Then
            Return SqlTypes.SqlDateTime.Null.Value
        End If

        Return New SqlTypes.SqlDateTime(dtResult.Year, dtResult.Month, dtResult.Day, dtResult.Hour, dtResult.Minute, dtResult.Second)
    End Function

    ''' <date>24/01/2011</date>
    ''' <author>Marco Turri</author>
    ''' <summary>
    ''' Function VerDateNull effettua la conversione di un generico valore
    ''' </summary>
    ''' <returns>Object</returns>
    Public Shared Function VerDateTimeNull(ByVal poValue As Object) As Object
        Dim dtResult As DateTime
        dtResult = DateTime.MinValue

        If poValue Is Nothing Then
            Return DBNull.Value
        End If

        If IsDBNull(poValue) Then
            Return DBNull.Value
        End If

        If Not IsDate(poValue) Then
            Return DBNull.Value
        End If

        If Not DateTime.TryParse(poValue.ToString(), dtResult) Then
            Return DBNull.Value
        End If

        If (dtResult = DateTime.MinValue) Then
            Return DBNull.Value
        End If

        Return dtResult
    End Function

    ''' <date>24/01/2011</date>
    ''' <author>Marco Turri</author>
    ''' <summary>
    ''' Function VerDBNull effettua la conversione di un generico valore
    ''' </summary>
    ''' <returns>Object</returns>
    Public Shared Function VerDBNull(ByVal poValue As Object) As Object
        Dim oResult As Object
        oResult = DBNull.Value

        Select Case poValue.GetType.ToString
            Case "System.Int16"
                If (VerInt16(poValue) <> 0) Then
                    oResult = poValue
                End If

            Case "System.Int32"
                If (VerInt32(poValue) <> 0) Then
                    oResult = poValue
                End If

            Case "System.Int64"
                If (VerInt64(poValue) <> 0) Then
                    oResult = poValue
                End If

            Case "System.Integer"
                If (VerInteger(poValue) <> 0) Then
                    oResult = poValue
                End If

            Case "System.Long"
                If (VerLong(poValue) <> 0) Then
                    oResult = poValue
                End If

            Case "System.Decimal"
                If (VerDecimal(poValue) <> 0) Then
                    oResult = poValue
                End If

            Case "System.Char"
                If (VerChar(poValue) <> "") Then
                    oResult = poValue
                End If

            Case "System.String"
                If (VerString(poValue, , , True) <> String.Empty) Then
                    oResult = poValue
                End If

            Case "System.Boolean"
                oResult = VerBool(poValue)

            Case "System.DateTime"
                If Not (VerDateTimeNull(poValue) Is DBNull.Value) Then
                    If (VerDateTime(poValue) <> DateTime.MinValue) Then
                        oResult = poValue
                    End If
                End If

            Case "System.Date"
                If Not (VerDateNull(poValue) Is DBNull.Value) Then
                    If (VerDate(poValue) <> Date.MinValue) Then
                        oResult = poValue
                    End If
                End If
        End Select

        Return oResult
    End Function

    ''' <date>24/01/2011</date>
    ''' <author>Marco Turri</author>
    ''' <summary>
    ''' Function FromBoolToInteger effettua la conversione di un generico valore
    ''' </summary>
    ''' <returns>String</returns>
    Public Shared Function FromBoolToInteger(ByVal pblnValue As Boolean) As String
        Const intTRUE As Integer = 1
        Const intFALSE As Integer = 0
        Dim strResult As String

        If pblnValue Then
            strResult = VerString(intTRUE)
        Else
            strResult = VerString(intFALSE)
        End If

        Return strResult
    End Function

    ''' <date>24/01/2011</date>
    ''' <author>Marco Turri</author>
    ''' <summary>
    ''' Function FormatKEY128 effettua una conversione di una stringa in una sequenza a 128 bit 
    ''' </summary>
    ''' <returns>String</returns>
    Public Shared Function FormatKEY128(ByVal pstrOriginalValue As String, Optional ByVal pchrFiller As Char = "#") As String
        Const intFormatSize As Integer = 16

        pstrOriginalValue = pstrOriginalValue.Trim
        pstrOriginalValue &= New String(pchrFiller, intFormatSize)

        Return Left(pstrOriginalValue, intFormatSize)
    End Function

    ''' <date>24/01/2011</date>
    ''' <author>Marco Turri</author>
    ''' <summary>
    ''' Function FormatKEY192 effettua una conversione di una stringa in una sequenza a 192 bit 
    ''' </summary>
    ''' <returns>String</returns>
    Public Shared Function FormatKEY192(ByVal pstrOriginalValue As String, Optional ByVal pchrFiller As Char = "#") As String
        Const intFormatSize As Integer = 24

        pstrOriginalValue = pstrOriginalValue.Trim
        pstrOriginalValue &= New String(pchrFiller, intFormatSize)

        Return Left(pstrOriginalValue, intFormatSize)
    End Function

    ''' <date>24/01/2011</date>
    ''' <author>Marco Turri</author>
    ''' <summary>
    ''' Function FormatKEY256 effettua una conversione di una stringa in una sequenza a 256 bit 
    ''' </summary>
    ''' <returns>String</returns>
    Public Shared Function FormatKEY256(ByVal pstrOriginalValue As String, Optional ByVal pchrFiller As Char = "#") As String
        Const intFormatSize As Integer = 32

        pstrOriginalValue = pstrOriginalValue.Trim
        pstrOriginalValue &= New String(pchrFiller, intFormatSize)

        Return Left(pstrOriginalValue, intFormatSize)
    End Function

    ''' <date>24/01/2011</date>
    ''' <author>Marco Turri</author>
    ''' <summary>
    ''' Function Left effettua una Trunk della stringa a partire da sinistra
    ''' </summary>
    ''' <returns>String</returns>
    Public Shared Function Left(ByVal poValue As Object, ByVal pintSize As Integer) As String
        Dim strResult As String
        strResult = VerString(poValue)

        If (strResult.Length <= pintSize) Then
            Return strResult
        End If

        strResult = strResult.Substring(0, pintSize)
        Return strResult
    End Function

    ''' <date>24/01/2011</date>
    ''' <author>Marco Turri</author>
    ''' <summary>
    ''' Function Right effettua una Trunk della stringa a partire da destra
    ''' </summary>
    ''' <returns>String</returns>
    Public Shared Function Right(ByVal poValue As Object, ByVal pintSize As Integer) As String
        Dim strResult As String
        strResult = VerString(poValue)

        If (strResult.Length <= pintSize) Then
            Return strResult
        End If

        strResult = strResult.Substring((strResult.Length - pintSize), pintSize)
        Return strResult
    End Function

    ''' <date>24/01/2011</date>
    ''' <author>Marco Turri</author>
    ''' <summary>
    ''' Function StringToXML effettua una conversione dei caratteri di escape di una stringa
    ''' </summary>
    ''' <returns>String</returns>
    Public Shared Function StringToXML(ByVal poValue As Object) As String
        Dim strResult As String
        strResult = VerString(poValue)

        'Replace quote|apostrophe|ampersand|less than(<)|great than (>)|carriage return
        strResult = strResult.Replace("""", "&quot;")
        strResult = strResult.Replace("'", "&apos;")
        strResult = strResult.Replace("&", "&amp;")
        strResult = strResult.Replace("<", "&lt;")
        strResult = strResult.Replace(">", "&gt;")
        strResult = strResult.Replace(Environment.NewLine, "\r\n")
        Return strResult
    End Function

    ''' <date>24/01/2011</date>
    ''' <author>Marco Turri</author>
    ''' <summary>
    ''' Function StringToXML effettua una ri-conversione dei caratteri di escape di una stringa
    ''' </summary>
    ''' <returns>String</returns>
    Public Shared Function XMLToString(ByVal poValue As Object) As String
        Dim strResult As String
        strResult = VerString(poValue)

        'Replace quote|apostrophe|ampersand|less than(<)|great than (>)|carriage return
        strResult = strResult.Replace("&quot;", """")
        strResult = strResult.Replace("&apos;", "'")
        strResult = strResult.Replace("&amp;", "&")
        strResult = strResult.Replace("&lt;", "<")
        strResult = strResult.Replace("&gt;", ">")
        strResult = strResult.Replace("\r\n", Environment.NewLine)
        Return strResult
    End Function

    ''' <date>24/01/2011</date>
    ''' <author>Marco Turri</author>
    ''' <summary>
    ''' Function TableCompare effettua una verifica della struttura di due generici datatable
    ''' </summary>
    ''' <returns>Boolean</returns>
    Public Shared Function TableCompare(ByVal poRowFirst As DataRowView, ByVal poRowSecond As DataRowView) As Boolean
        TableCompare = True

        If (poRowFirst Is Nothing) Or (poRowSecond Is Nothing) Then
            TableCompare = True
            Exit Function
        End If

        'Check Name Of DataTable
        If (poRowFirst.DataView.Table.TableName.ToLower <> poRowSecond.DataView.Table.TableName.ToLower) Then
            TableCompare = False
            Exit Function
        End If

        'Check Count Columns (Structures of Datatable)
        If (poRowFirst.DataView.Table.Columns.Count <> poRowSecond.DataView.Table.Columns.Count) Then
            TableCompare = False
            Exit Function
        End If

        'Check Structure Columns Name/Type
        For counter As Integer = poRowFirst.DataView.Table.Columns.Count - 1 To 0 Step -1
            If (poRowFirst.DataView.Table.Columns(counter).ColumnName <> poRowSecond.DataView.Table.Columns(counter).ColumnName) Or
                (poRowFirst.DataView.Table.Columns(counter).DataType.FullName.ToLower <> poRowSecond.DataView.Table.Columns(counter).DataType.FullName.ToLower) Then
                TableCompare = False
                Exit Function
            End If
        Next
    End Function

    ''' <date>24/01/2011</date>
    ''' <author>Marco Turri</author>
    ''' <summary>
    ''' Function ListInString effettua una conversione di un array in una lista di caratteri
    ''' delimitata da un separatore 
    ''' </summary>
    ''' <returns>String</returns>
    Public Shared Function ListInString(ByVal poList As ArrayList,
                                        Optional ByVal pstrDelimiter As String = ",",
                                        Optional ByVal pblnAddQuote As Boolean = True) As String
        Const strQuote As String = "'"
        Dim sbList As New StringBuilder

        'Retrieve List Of IP
        For counter As Integer = 0 To poList.Count - 1
            If (counter > 0) Then
                sbList.Append(pstrDelimiter)
            End If

            If pblnAddQuote Then
                sbList.Append(strQuote)
            End If

            sbList.Append(VerString(poList(counter)).Trim)

            If pblnAddQuote Then
                sbList.Append(strQuote)
            End If
        Next

        Return sbList.ToString
    End Function


    ''' <date>24/01/2011</date>
    ''' <author>Marco Turri</author>
    ''' <summary>
    ''' Function WriteEvent effettua il caricamento di una entry nel registro eventi
    ''' </summary>
    ''' <returns>Boolean</returns>
    Public Shared Function WriteEvent(ByVal pstrEventLogName As String, ByVal pstrMessage As String, Optional ByVal enType As EventLogEntryType = EventLogEntryType.Error) As Boolean
        Const C_APPLICATION_NAME As String = "Application"
        Dim objEventLog As New EventLog
        WriteEvent = True

        Try
            'Register the App as an Event Source
            If Not EventLog.SourceExists(pstrEventLogName) Then
                EventLog.CreateEventSource(pstrEventLogName, C_APPLICATION_NAME)
            End If

            'Write Entry LOG
            objEventLog.Source = pstrEventLogName
            objEventLog.WriteEntry(pstrMessage, enType)
        Catch
            WriteEvent = False
        Finally
            objEventLog.Dispose()
        End Try
    End Function


    ''' <date>24/01/2011</date>
    ''' <author>Marco Turri</author>
    ''' <summary>
    ''' Function CharInString effettua una verifica dei singoli caratteri di una stringa
    ''' a partire da un campione di caratteri fornito
    ''' </summary>
    ''' <returns>Boolean</returns>
    Public Shared Function CharInString(ByVal pstrSource As String, Optional ByVal pstrCharsCompare As String = "qwertyuiopasdfghjklzxcvbnmòàùèéì1234567890-") As Boolean
        For Each chrValue As Char In pstrSource.ToCharArray
            If Not pstrCharsCompare.Contains(chrValue) Then
                Return False
            End If
        Next

        Return True
    End Function


   
#End Region

End Class
