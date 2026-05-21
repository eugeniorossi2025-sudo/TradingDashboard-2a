Imports FlexCel.Core
Imports FlexCel.Render
Imports FlexCel.XlsAdapter
Imports Telerik.Web.UI
Imports Telerik.Web.UI.Upload
Imports System.IO
Imports System.Drawing
Imports Utility
Imports System.Data
Imports System

Public Module Import

    Private Function GetColumnName(ByVal Dt As DataTable, ByVal ColumnName As String) As String
        Dim i As Integer = 0
        Dim str As String = ColumnName
        Dim bit_Foud As Boolean = False

        While Not (bit_Foud)
            If Dt.Columns.Contains(str) = False Then
                bit_Foud = True
            Else
                i = i + 1
                str = ColumnName & i
            End If
        End While
        Return str
    End Function

    Public Function ExcelToDS(ByVal Path As String, ByVal SheetNameToTest As String) As DataSet
        Try
            'Open the Excel file.
            Dim xls As New XlsFile(False)
            xls.Open(Path)
            Dim dataSet1 As New DataSet()
            For sheet As Integer = 1 To xls.SheetCount
                xls.ActiveSheet = sheet
                Dim Data As DataTable = dataSet1.Tables.Add(xls.SheetName)
                Data.BeginLoadData()
                Try
                    Dim ColCount As Integer = xls.ColCount
                    'Add one column on the dataset for each used column on Excel. 
                    For c As Integer = 1 To ColCount
                        'Here we will add all strings, since we do not know what we are waiting for. 
                        If GetColumnName(Data, xls.GetStringFromCell(1, c).Value) <> "" Then
                            Data.Columns.Add(GetColumnName(Data, xls.GetStringFromCell(1, c)), GetType([String]))
                        End If
                    Next


                    Dim RowCount As Integer = xls.RowCount

                    For r As Integer = 2 To RowCount
                        Dim ColumnsCount As Integer = Data.Columns.Count
                        Dim dr As String() = New String(ColumnsCount - 1) {}
                        Array.Clear(dr, 0, dr.Length)

                        For c = 1 To ColumnsCount
                            Dim rs As TRichString = xls.GetStringFromCell(r, c)
                            dr(c - 1) = rs.Value
                        Next
                        Data.Rows.Add(dr)
                    Next
                Finally
                    Data.EndLoadData()
                End Try

            Next
            If SheetNameToTest <> "" Then
                If dataSet1.Tables.Contains(SheetNameToTest) Then
                    Return dataSet1
                Else
                    Return Nothing
                End If
            Else
                Return dataSet1
            End If
        Catch ex As Exception
            Return Nothing
        End Try
    End Function



End Module
