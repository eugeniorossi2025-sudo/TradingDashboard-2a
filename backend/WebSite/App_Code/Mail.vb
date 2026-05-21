Imports Microsoft.VisualBasic
Imports System.Data.SqlClient
Imports System.Data.SqlTypes
Imports System.Net.Mail
Imports System.Data

Public Module Mail
    Public Function IsMail(ByVal sEmail As String) As Boolean
        Try
            Dim Addr As New MailAddress(sEmail)
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function
 

    Function StripHTMLTags(ByVal HTMLToStrip As String) As String
        Dim stripped As String
        If HTMLToStrip <> "" Then
            stripped = Regex.Replace(HTMLToStrip, "<(.|\n)+?>", String.Empty)
            Return stripped
        Else
            Return ""
        End If
    End Function


    Public Sub SendMail(ByVal sSubject As String, ByVal sHtmlBody As String, ByVal sToAddress As String)

        Dim mail As New MailMessage()
        'If FromMail <> "" Then
        '    mail.From = New MailAddress(FromMail, FromMail)
        'End If
        mail.[To].Add(sToAddress)
        mail.Subject = sSubject

        Dim plainView As AlternateView = AlternateView.CreateAlternateViewFromString(StripHTMLTags(sHtmlBody), Nothing, "text/plain")
        Dim htmlView As AlternateView

        'sostituzione immagini embedded
        Dim StrReturn As String = ""
        Dim Testo As String = sHtmlBody
        Dim Pos As Integer
        Dim Pos2 As Integer
        Dim CurId As String = ""
        Dim TestoStart As String
        Dim TestoFine As String
        Dim NumeroImmaggine As Integer = 0
        Dim Lista As New Hashtable


        While InStr(Testo, "<img src=") > 0
            Pos = InStr(Testo, "<img src=")
            If Pos > 0 Then
                Pos2 = InStr(Pos + 10, Testo, Chr(34)) 'cerco la virgoletta chiusa
                CurId = Mid(Testo, Pos + 9 + 1, Pos2 - (Pos + 10)) 'nome del file
                TestoStart = Mid(Testo, 1, Pos - 1) 'prima dell'inizio del tag <a            
                TestoFine = Mid(Testo, Pos2 + 1) 'dopo l'id del tag <a
                NumeroImmaggine = NumeroImmaggine + 1
                If CurId <> "" Then
                    StrReturn = "cid:IMG" & NumeroImmaggine
                    Testo = TestoStart + "<img  src=" + Chr(34) + StrReturn + Chr(34) + TestoFine
                    Dim NomeFile As String
                    If InStr(8, CurId, "/") = 0 Then
                        NomeFile = HttpContext.Current.Server.MapPath(CurId)
                    Else
                        NomeFile = HttpContext.Current.Server.MapPath(Mid(CurId, InStr(8, CurId, "/")))
                    End If

                    'Lista.Add(NumeroImmaggine,NomeFile)  
                    Lista(NumeroImmaggine) = NomeFile
                End If
            End If
        End While
        htmlView = AlternateView.CreateAlternateViewFromString(Testo, Nothing, "text/html")
        mail.AlternateViews.Add(plainView)
        mail.AlternateViews.Add(htmlView)

        Dim entry As DictionaryEntry
        NumeroImmaggine = 0
        For Each entry In Lista

            Dim logo As New LinkedResource(CType(entry.Value, String))
            NumeroImmaggine = CType(entry.Key, Integer)
            logo.ContentId = "IMG" & NumeroImmaggine

            htmlView.LinkedResources.Add(logo)
        Next

        Dim smtp As New SmtpClient
        smtp.Send(mail)


    End Sub



End Module
