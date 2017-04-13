Imports System.IO
Imports System.Text

Partial Class log
    Inherits System.Web.UI.Page



    Private Sub log_Load(sender As Object, e As EventArgs) Handles Me.Load
        If IsPostBack Then Exit Sub
        Dim tag As String
        Dim rip As String
        Dim ant As String

        tag = Request.QueryString("tag") & ""
        rip = Request.QueryString("ip") & ""
        ant = Request.QueryString("ant") & ""


        Response.Clear()
        If tag <> "" And ant <> "" And rip <> "" Then
            File.AppendAllText("c:\logs\" & rip & "_" & ant & ".txt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") & " '" & tag & "'," & ant & vbCrLf)
            Response.Write("OK")
        Else
            Response.Write("param error")
        End If

        Response.End()
    End Sub




End Class
