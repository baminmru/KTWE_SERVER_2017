Imports KTWE
Imports KTWE2
Imports KTWE4

Imports System.Xml
Imports System.IO
Imports System.Reflection

Module Module1
    Private LogPath As String = "\KTWE_LOG_"
    Private Xml As XmlDocument
    Public Sub LogString(ByVal s As String)
        Try
            Console.WriteLine(s)
            My.Computer.FileSystem.WriteAllText(GetMyDir() + LogPath & Date.Now.ToString("yyyy_MM_dd") & ".txt", "'" & Date.Now.ToString("yyyy.MM.dd HH:mm:ss.fff") & ",' " & s & "'" & vbCrLf, True)
        Catch ex As Exception

        End Try
    End Sub
    Private Function GetMyDir() As String
        Dim s As String

        s = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase)
        s = s.Substring(6)
        Return s
    End Function


    Sub Main()
        Dim s As String
        s = GetMyDir()
        Dim ktwesrv As KTWE.TCPServer
        Dim ktwesrv2 As KTWE2.UDPServer
        Dim ktwesrv3 As KTWE2.UDPServerNew
        Dim ktwesrv4 As KTWE4.UDPServer4
        Xml = New XmlDocument
        Try
            Xml.Load(s + "\KTWEConfig.xml")
        Catch
            LogString("Не найден файл конфигурации:" + s + "\KTWEConfig.xml")
            Return
        End Try

        Dim ServerIP As String
        Dim Port As Integer
        Dim CLIENTPORT As Integer
        Dim CloseTime As Integer
        Dim ServerType As String
        'Dim Site As String
        'Dim User As String
        'Dim Password As String

        Dim node As XmlElement
        node = Xml.LastChild()

        Try
            ServerType = node.Attributes.GetNamedItem("SERVERTYPE").Value
        Catch ex As Exception
            ServerType = "TCP"
        End Try


        Try
            ServerIP = node.Attributes.GetNamedItem("ServerIP").Value
        Catch ex As Exception
            ServerIP = "192.168.0.1"
        End Try

        Try
            Port = Int(node.Attributes.GetNamedItem("Port").Value)
        Catch ex As Exception
            Port = 5678
        End Try

        Try
            CLIENTPORT = Int(node.Attributes.GetNamedItem("CLIENTPORT").Value)
        Catch ex As Exception
            CLIENTPORT = 6789
        End Try


        Try
            CloseTime = Int(node.Attributes.GetNamedItem("AUTOCLOSETIME").Value)
        Catch ex As Exception
            CloseTime = 120
        End Try

        'Site = (node.Attributes.GetNamedItem("Site").Value)
        'User = (node.Attributes.GetNamedItem("User").Value)
        'Password = (node.Attributes.GetNamedItem("Password").Value)

        node = Nothing
        Xml = Nothing

        If ServerType = "TCP" Then
            ktwesrv = New TCPServer(ServerIP, Port, CloseTime) 'Site, User, Password)

            If Not ktwesrv Is Nothing Then
                ktwesrv.StartServer()
                While (ktwesrv.IsLive())
                    Threading.Thread.Sleep(1000)
                    'Console.Write(".")
                End While
                LogString("Stopping server and exit, service start new one. (" + ServerIP.ToString() + ":" + Port.ToString() + ")")
                ktwesrv.StopServer()
                Return

            End If
        ElseIf ServerType = "UDP" Then

            LogString("Starting UDP server  (" + ServerIP.ToString() + ":" + Port.ToString() + ")")
            ktwesrv2 = New UDPServer(ServerIP, Port, CLIENTPORT)

            If Not ktwesrv2 Is Nothing Then
                ktwesrv2.StartServer()
                While (ktwesrv2.IsLive())
                    Threading.Thread.Sleep(1000)
                    'Console.Write(".")
                End While
                LogString("Stopping server and exit, service start new one. (" + ServerIP.ToString() + ":" + Port.ToString() + ")")
                ktwesrv2.StopServer()
                Return

            End If

        ElseIf ServerType = "UDPNEW" Then

            LogString("Starting UDP server  (" + ServerIP.ToString() + ":" + Port.ToString() + ")")
            ktwesrv3 = New UDPServerNew(ServerIP, Port, CLIENTPORT)

            If Not ktwesrv3 Is Nothing Then
                ktwesrv3.StartServer()
                While (ktwesrv3.IsLive())
                    Threading.Thread.Sleep(1000)
                    'Console.Write(".")
                End While
                LogString("Stopping server and exit, service start new one. (" + ServerIP.ToString() + ":" + Port.ToString() + ")")
                ktwesrv3.StopServer()
                Return

            End If
        ElseIf ServerType = "UDP4" Then

            LogString("Starting UDP server  (" + ServerIP.ToString() + ":" + Port.ToString() + ")")
            ktwesrv4 = New UDPServer4(ServerIP, Port, CLIENTPORT)

            If Not ktwesrv4 Is Nothing Then
                ktwesrv4.StartServer()
                While (ktwesrv4.IsLive())
                    Threading.Thread.Sleep(1000)
                    'Console.Write(".")
                End While
                LogString("Stopping server and exit, service start new one. (" + ServerIP.ToString() + ":" + Port.ToString() + ")")
                ktwesrv4.StopServer()
                Return

            End If
        End If

        LogString("Error while connecting to database")
        Return

    End Sub

End Module
