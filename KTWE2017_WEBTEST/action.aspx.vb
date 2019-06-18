
Partial Class action
    Inherits System.Web.UI.Page

    Public Sub New()

    End Sub

    Private Sub action_Load(sender As Object, e As EventArgs) Handles Me.Load
        If IsPostBack Then Exit Sub
        Dim tag As String
        Dim rip As String
        Dim ant As String

        tag = Request.QueryString("tag")
        rip = Request.QueryString("ip")
        ant = Request.QueryString("ant")



        Response.Clear()
        Response.Write("<?xml version=""1.0"" encoding=""utf-8""?>" & vbCrLf & "<root><action>pin1</action><action>relay2</action><id>" + tag + "_" + ant + "</id></root>")
        Response.End()
    End Sub
End Class
