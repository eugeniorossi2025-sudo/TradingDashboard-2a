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

Public Module Export

    Public Function ExportAndDownload(ByVal strPath As String, ByVal FileName As String, ByVal Format As String, ByVal SheetName As String, ByVal RowFreeze As Integer, ByVal ColFreeze As Integer, ByVal Dt As DataTable) As String
        Dim RetValue As String = ""
        Select Case Format.ToLower()
            Case "xls" : RetValue = ExportXls(strPath, FileName, SheetName, RowFreeze, ColFreeze, Dt)
            Case "csv" : RetValue = ExportCsv(strPath, FileName, Dt)
                'Case "pdf" : ExportPdf(strPath, FileName, SheetName, Dt)
        End Select
        Return RetValue
    End Function

    Public Function ExportCsv(ByVal strPath As String, ByVal FileName As String, ByVal Dt As DataTable, Optional ByVal strSeparator As String = ";") As String

        Dim intNRow As Integer = 0
        Dim context As RadProgressContext = RadProgressContext.Current
        Dim total As Integer
        Dim CSVRow As String
        Dim RetValue As String = ""
        Dim strBuilder As New StringBuilder
        Try

            '************PROGRESS AREA************
            If Dt.Rows.Count >= 1 Then
                total = Dt.Rows.Count - 1
            End If
            context.SecondaryTotal = total
            context.SecondaryValue = 0
            context.SecondaryPercent = 0
            '*************************************

            'SCRIVO LE INTESTAZIONI
            CSVRow = ""
            For j As Integer = 0 To Dt.Columns.Count - 1
                If CSVRow <> "" Then CSVRow = CSVRow & strSeparator
                CSVRow = CSVRow & Dt.Columns(j).ColumnName
            Next
            CSVRow = CSVRow & Environment.NewLine
            strBuilder.Append(CSVRow)
            CSVRow = ""
            For i As Integer = 0 To Dt.Rows.Count - 1

                '************PROGRESS AREA************
                context.CurrentOperationText = String.Format("Record {0}", intNRow)
                context.SecondaryTotal = total
                context.SecondaryValue = intNRow.ToString()
                context.SecondaryPercent = Format((CDbl(intNRow) / total) * 100, "0.00")
                'System.Threading.Thread.Sleep(500)
                '*************************************
                CSVRow = ""

                For j As Integer = 0 To Dt.Columns.Count - 1
                    If CSVRow <> "" Then CSVRow = CSVRow & strSeparator
                    CSVRow = CSVRow & Dt.Rows(i)(j)
                Next
                CSVRow = CSVRow & Environment.NewLine
                strBuilder.Append(CSVRow)
                intNRow = intNRow + 1
            Next
            context.OperationComplete = True


            HttpContext.Current.Response.Clear()
            HttpContext.Current.Response.ContentType = "text/csv"
            HttpContext.Current.Response.AppendHeader("Content-Disposition", "attachment; filename=" & FileName)
            HttpContext.Current.Response.Write(strBuilder.ToString)
            HttpContext.Current.Response.Flush()
            HttpContext.Current.Response.End()
        Catch ex As Exception
            RetValue = ex.Message & " - " & ex.StackTrace
        Finally
            ExportCsv = RetValue
        End Try

    End Function



    Public Function ExportXls(ByVal strPath As String, ByVal FileName As String, ByVal SheetName As String, ByVal RowFreeze As Integer, ByVal ColFreeze As Integer, ByVal Dt As DataTable) As String

        Dim intNRow As Integer = 0
        Dim context As RadProgressContext = RadProgressContext.Current
        Dim total As Integer
        Dim Xls As ExcelFile = New XlsFile
        Dim RetValue As String = ""
        Try
            Xls.NewFile(1)
            Xls.SheetName = SheetName
            '************PROGRESS AREA************
            If Dt.Rows.Count >= 1 Then
                total = Dt.Rows.Count - 1
            End If
            context.SecondaryTotal = total
            context.SecondaryValue = 0
            context.SecondaryPercent = 0
            '***********HEADERS********************  
            Dim f As TFlxFormat = Xls.GetDefaultFormat
            'f.Font.Name = "Times New Roman"
            f.Font.Color = Color.Red
            'f.FillPattern.Pattern = TFlxPatternStyle.Gray25
            'f.FillPattern.FgColor = Color.Blue
            'f.FillPattern.BgColor = Color.Blue
            Dim XF As Integer = Xls.AddFormat(f)

            For j As Integer = 0 To Dt.Columns.Count - 1
                Xls.SetCellValue(1, j + 1, Dt.Columns(j).ColumnName)
                Xls.SetCellFormat(1, j + 1, XF)
            Next

            '***************************VALUES**************

            For i As Integer = 0 To Dt.Rows.Count - 1

                '************PROGRESS AREA************
                context.CurrentOperationText = String.Format("Record {0}", intNRow)
                context.SecondaryTotal = total
                context.SecondaryValue = intNRow.ToString()
                context.SecondaryPercent = Format((CDbl(intNRow) / total) * 100, "0.00")
                'System.Threading.Thread.Sleep(500)
                '*************************************

                For j As Integer = 0 To Dt.Columns.Count - 1
                    If Dt.Columns(j).DataType Is System.Type.GetType("System.Int32") Then
                        Xls.SetCellValue(i + 2, j + 1, Utility.VerInt32(Dt.Rows(i)(j)))
                    ElseIf Dt.Columns(j).DataType Is System.Type.GetType("System.Decimal") Then
                        Xls.SetCellValue(i + 2, j + 1, Utility.VerDecimal(Dt.Rows(i)(j)))
                    Else
                        Xls.SetCellValue(i + 2, j + 1, VerString(Dt.Rows(i)(j)))
                    End If
                Next

                intNRow = intNRow + 1
            Next
            context.OperationComplete = True
            Xls.AutofitRowsOnWorkbook(False, True, 1)
            For j As Integer = 0 To Dt.Columns.Count - 1
                Xls.AutofitCol(j + 1, False, 1)
                Xls.AutofitCol(j + 1, True, 2)
            Next
            Xls.FreezePanes(New TCellAddress(RowFreeze, ColFreeze))
            'Xls.SetAutoRowHeight( 1, True)

            Using ms As New MemoryStream
                Xls.Save(ms)
                ms.Position = 0
                HttpContext.Current.Response.Clear()
                HttpContext.Current.Response.AddHeader("Content-Disposition", "attachment; filename=" & FileName)
                HttpContext.Current.Response.AddHeader("Content-Length", ms.Length.ToString())
                HttpContext.Current.Response.ContentType = "application/excel"
                HttpContext.Current.Response.BinaryWrite(ms.ToArray())
                HttpContext.Current.Response.Flush()
                HttpContext.Current.Response.End()
            End Using

        Catch ex As Exception
            RetValue = ex.Message & " - " & ex.StackTrace
        Finally
            ExportXls = RetValue
        End Try
    End Function

End Module
