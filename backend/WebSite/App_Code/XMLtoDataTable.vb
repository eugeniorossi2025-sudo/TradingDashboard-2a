Imports System.Xml
Imports System.IO
Imports System.IO.StreamReader
Imports System.IO.StreamWriter
Imports System.Data

Public NotInheritable Class XMLtoDataTable
    Private Sub New()
    End Sub
    Private Shared Function getDefaultType() As ColumnType
        Return New ColumnType(GetType([String]))
    End Function

    Private Structure ColumnType
        Public type As Type
        Private name As String
        Public Sub New(type As Type)
            Me.type = type
            Me.name = type.ToString().ToLower()
        End Sub
        Public Function ParseString(input As String) As Object
            If [String].IsNullOrEmpty(input) Then
                Return DBNull.Value
            End If
            Select Case type.ToString()
                Case "system.datetime"
                    Return DateTime.Parse(input)
                Case "system.decimal"
                    Return Decimal.Parse(input)
                Case "system.boolean"
                    Return Boolean.Parse(input)
                Case Else
                    Return input
            End Select
        End Function
    End Structure


    Private Shared Function [getType](data As XmlNode) As ColumnType
        Dim type As String = Nothing
        If data.Attributes("ss:Type") Is Nothing OrElse data.Attributes("ss:Type").Value Is Nothing Then
            type = ""
        Else
            type = data.Attributes("ss:Type").Value
        End If

        Select Case type
            Case "DateTime"
                Return New ColumnType(GetType(DateTime))
            Case "Boolean"
                Return New ColumnType(GetType([Boolean]))
            Case "Number"
                Return New ColumnType(GetType([Decimal]))
            Case ""
                Dim test2 As Decimal
                If data Is Nothing OrElse [String].IsNullOrEmpty(data.InnerText) OrElse Decimal.TryParse(data.InnerText, test2) Then
                    Return New ColumnType(GetType([Decimal]))
                Else
                    Return New ColumnType(GetType([String]))
                End If
            Case Else
                '"String"
                Return New ColumnType(GetType([String]))
        End Select
    End Function

    Public Shared Function ImportExcelXML(fileName As String, hasHeaders As Boolean, autoDetectColumnType As Boolean) As DataSet
        Dim sr As New StreamReader(fileName)
        Dim st As Stream = DirectCast(sr.BaseStream, Stream)
        Return ImportExcelXML(st, hasHeaders, autoDetectColumnType)
    End Function

    Private Shared Function ImportExcelXML(inputFileStream As Stream, hasHeaders As Boolean, autoDetectColumnType As Boolean) As DataSet
        Dim doc As New XmlDocument()
        doc.Load(New XmlTextReader(inputFileStream))
        Dim nsmgr As New XmlNamespaceManager(doc.NameTable)

        nsmgr.AddNamespace("o", "urn:schemas-microsoft-com:office:office")
        nsmgr.AddNamespace("x", "urn:schemas-microsoft-com:office:excel")
        nsmgr.AddNamespace("ss", "urn:schemas-microsoft-com:office:spreadsheet")

        Dim ds As New DataSet()

        For Each node As XmlNode In doc.DocumentElement.SelectNodes("//ss:Worksheet", nsmgr)
            Dim dt As New DataTable(node.Attributes("ss:Name").Value)
            ds.Tables.Add(dt)
            Dim rows As XmlNodeList = node.SelectNodes("ss:Table/ss:Row", nsmgr)
            If rows.Count > 0 Then

                '*************************
                'Add Columns To Table from header row
                '*************************
                Dim columns As New List(Of ColumnType)()
                Dim startIndex As Integer = 0
                If hasHeaders Then
                    For Each data As XmlNode In rows(0).SelectNodes("ss:Cell/ss:Data", nsmgr)
                        columns.Add(New ColumnType(GetType(String)))
                        'default to text
                        dt.Columns.Add(data.InnerText, GetType(String))
                    Next
                    startIndex += 1
                End If
                '*************************
                'Update Data-Types of columns if Auto-Detecting
                '*************************
                If autoDetectColumnType AndAlso rows.Count > 0 Then
                    Dim cells As XmlNodeList = rows(startIndex).SelectNodes("ss:Cell", nsmgr)
                    Dim actualCellIndex As Integer = 0
                    For cellIndex As Integer = 0 To cells.Count - 1
                        Dim cell As XmlNode = cells(cellIndex)
                        If cell.Attributes("ss:Index") IsNot Nothing Then
                            actualCellIndex = Integer.Parse(cell.Attributes("ss:Index").Value) - 1
                        End If

                        Dim autoDetectType As ColumnType = [getType](cell.SelectSingleNode("ss:Data", nsmgr))

                        If actualCellIndex >= dt.Columns.Count Then
                            dt.Columns.Add("Column" + actualCellIndex.ToString(), autoDetectType.type)
                            columns.Add(autoDetectType)
                        Else
                            dt.Columns(actualCellIndex).DataType = autoDetectType.type
                            columns(actualCellIndex) = autoDetectType
                        End If

                        actualCellIndex += 1
                    Next
                End If
                '*************************
                'Load Data
                '*************************
                For i As Integer = startIndex To rows.Count - 1
                    Dim row As DataRow = dt.NewRow()
                    Dim cells As XmlNodeList = rows(i).SelectNodes("ss:Cell", nsmgr)
                    Dim actualCellIndex As Integer = 0
                    For cellIndex As Integer = 0 To cells.Count - 1
                        Dim cell As XmlNode = cells(cellIndex)
                        If cell.Attributes("ss:Index") IsNot Nothing Then
                            actualCellIndex = Integer.Parse(cell.Attributes("ss:Index").Value) - 1
                        End If

                        Dim data As XmlNode = cell.SelectSingleNode("ss:Data", nsmgr)

                        If actualCellIndex >= dt.Columns.Count Then
                            For ii As Integer = dt.Columns.Count To actualCellIndex - 1
                                dt.Columns.Add("Column" + actualCellIndex.ToString(), GetType(String))
                                columns.Add(getDefaultType())
                            Next
                            ' ii
                            Dim autoDetectType As ColumnType = [getType](cell.SelectSingleNode("ss:Data", nsmgr))
                            dt.Columns.Add("Column" + actualCellIndex.ToString(), GetType(String))
                            columns.Add(autoDetectType)
                        End If
                        If data IsNot Nothing Then
                            row(actualCellIndex) = data.InnerText
                        End If

                        actualCellIndex += 1
                    Next

                    dt.Rows.Add(row)
                Next
            End If
        Next
        Return ds
    End Function


End Class
