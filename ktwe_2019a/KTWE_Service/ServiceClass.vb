Imports System.Xml
Imports System.IO
Imports System.Text
Imports System.Diagnostics
Imports System.Collections
Imports System.Data
Imports System.Data.SqlClient

Public Class ServiceClass
    'Dim PList As Hashtable
    Dim ProcessKTWE As System.Diagnostics.Process = Nothing
    Private LogPath As String = "c:\LOG\"
    Public Sub LogString(ByVal s As String)
        Try
            'Console.WriteLine(s)
            File.AppendAllText(LogPath & "log_" & Date.Today.ToString("yyyyMMdd") & ".txt", Date.Now.ToString() & ": " & s & vbCrLf)
        Catch ex As Exception

        End Try

    End Sub

    Private Function GetMyDir() As String
        Dim s As String
        s = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase)
        s = s.Substring(6)
        Return s
    End Function

    Private Sub CleanProcessKTWE() 'ByVal rID As String)
        'ProcessKTWE = Nothing
        'If PList.ContainsKey(rID.ToString) Then
        '    ProcessKTWE = PList(rID.ToString)
        If Not ProcessKTWE Is Nothing Then
            LogString("KTWE Close reader process ")
            If Not ProcessKTWE.HasExited Then
                ProcessKTWE.Kill()
            End If
            ProcessKTWE = Nothing
            '    PList.Remove(rID)
            'End If
        End If

    End Sub

    Private Function CheckProcessKTWE() 'ByVal rID As String)
        Dim ok As Boolean
        'ProcessKTWE = Nothing
        'If PList.ContainsKey(rID) Then
        '    ProcessKTWE = PList(rID)
        If Not ProcessKTWE Is Nothing Then
            If Not ProcessKTWE.HasExited Then
                ok = True
            End If
        Else
            ok = False
        End If
        Return ok
        'Else
        'Return False
        'End If

    End Function

    Private Sub StartProcessKTWE()
        LogString("KTWEReader Start Server process ")
        ProcessKTWE = Nothing
        ProcessKTWE = New Process()
            Dim FileName As String
            Dim DirName As String
        FileName = GetMyDir() + "\KTWE_PROC.exe"
            DirName = GetMyDir()
            'ProcessKTWE.StartInfo.Arguments = "-R " + rID
            ProcessKTWE.StartInfo.FileName = FileName
            ProcessKTWE.StartInfo.WorkingDirectory = DirName
            ProcessKTWE.Start()
        '   PList.Add(rID, ProcessKTWE)
        ' End If
    End Sub


    Public Sub OnStart(ByVal args() As String)
        If (pMainThread Is Nothing) Then
            pMainThread = New System.Threading.Thread(New System.Threading.ThreadStart(AddressOf MyThread))
            pMainThread.SetApartmentState(System.Threading.ApartmentState.MTA)
            pMainThread.Name = "KTWEReader server thread"
        End If

        m_StopServer = False
        pMainThread.Start()



    End Sub
    Private pMainThread As System.Threading.Thread

    ' принудительный рестарт сервера
    Private RestartXRCounter As Long = 0
    Private Inited As Boolean = False
    ' Private ReadersInfo As List(Of String)


    Public Function Init() As Boolean
  
        'Dim xml As XmlDocument
        'xml = New XmlDocument
        'Dim node As XmlElement
        'Dim root As XmlElement
        'Dim readers As XmlNodeList
        'Try
        '    xml.Load(System.IO.Path.GetDirectoryName(Me.GetType().Assembly.Location()) + "\Readers.xml")

        '    root = xml.LastChild()
        'Catch ex As Exception
        '    LogString("Readers.xml open error :" + ex.Message)
        '    Return False
        'End Try
        'ReadersInfo = New List(Of String)

        'readers = root.GetElementsByTagName("Reader")
        'Dim rIP As String
        'For Each node In readers
        '    rIP = node.Attributes.GetNamedItem("IP").Value
        '    ReadersInfo.Add(rIP)
        'Next

       
        Inited = True
        Return Inited
    End Function

    Private Sub MyThread()
        RestartXRCounter = 360 * 24
        While (Not m_StopServer)

            Init()

          
            'Dim i As Integer
            'Dim rIP As String

            'For i = 0 To ReadersInfo.Count - 1
            '    rIP = ReadersInfo(i)
            If Not CheckProcessKTWE() Then
                CleanProcessKTWE()
                StartProcessKTWE()

            End If
            'Next

            If m_StopServer Then Return

            If RestartXRCounter > 0 Then
                RestartXRCounter = RestartXRCounter - 1
            Else
                'For i = 0 To ReadersInfo.Count - 1
                '    rIP = ReadersInfo(i)
                If Not CheckProcessKTWE() Then
                    CleanProcessKTWE()
                    StartProcessKTWE()
                End If
                'Next
                RestartXRCounter = 360 * 24
            End If


            If m_StopServer Then Return


            System.Threading.Thread.Sleep(1000)
        End While

    End Sub

    Private m_StopServer As Boolean = False

    Public Sub OnStop()

        LogString("Stopping KTWEReader processor")
        m_StopServer = True
        System.Threading.Thread.Sleep(15000)

again:
        'For Each s As String In PList.Keys
        Try
            CleanProcessKTWE()
            ' GoTo again
        Catch ex As Exception
            LogString(ex.Message)
        End Try
        'Next s



        LogString("KTWE Server stopped")

    End Sub



    Public Sub New()
        'PList = New Hashtable

    End Sub
End Class
