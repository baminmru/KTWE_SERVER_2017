Module Module1
    Sub Main()
        Dim srv As KTWEService2017.ServiceClass
        srv = New KTWEService2017.ServiceClass
        Dim s() As String
        s = Split("1 2 3 ", " ")
        srv.OnStart(s)

    End Sub
End Module
