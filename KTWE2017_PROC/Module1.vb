

Imports System.Xml
Imports System.IO
Imports KTWE_PROC2017
Imports KTWE2017_lib

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

        Dim ktwesrv3 As UDPServerNew
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


        Dim node As XmlElement
        node = Xml.LastChild()

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




        node = Nothing
        Xml = Nothing



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


        LogString("Error while connecting to database")
        Return

    End Sub

End Module
