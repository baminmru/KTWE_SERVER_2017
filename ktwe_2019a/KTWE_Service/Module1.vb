Module Module1
    Sub Main()
        Dim srv As KTWEService.ServiceClass
        srv = New KTWEService.ServiceClass
        Dim s() As String
        s = Split("1 2 3 ", " ")
        srv.OnStart(s)

    End Sub
End Module
