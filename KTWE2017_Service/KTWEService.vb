Imports System.Xml
Imports System.IO
Imports System.Text
Imports System.Diagnostics
Imports System.Collections
Imports System.Data
Imports System.Data.SqlClient

Public Class KTWEReaderService

    Dim srv As ServiceClass


    Protected Overrides Sub OnStart(ByVal args() As String)
        srv.OnStart(args)
    End Sub

    Protected Overrides Sub OnStop()
        srv.OnStop()
    End Sub



    Public Sub New()
        InitializeComponent()
        srv = New ServiceClass
    End Sub
End Class
