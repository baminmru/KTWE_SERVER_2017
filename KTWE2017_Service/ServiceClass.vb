Imports System.Xml
Imports System.IO
Imports System.Text
Imports System.Diagnostics
Imports System.Collections
Imports System.Data
Imports System.Data.SqlClient

Public Class ServiceClass

    Dim ProcessKTWE As System.Diagnostics.Process = Nothing
    Private LogPath As String = "c:\LOG\"
    Public Sub LogString(ByVal s As String)
        Try
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

    Private Sub CleanProcessKTWE()

        If Not ProcessKTWE Is Nothing Then
            LogString("KTWE Close reader process ")
            If Not ProcessKTWE.HasExited Then
                ProcessKTWE.Kill()
            End If
            ProcessKTWE = Nothing

        End If

    End Sub

    Private Function CheckProcessKTWE()
        Dim ok As Boolean

        If Not ProcessKTWE Is Nothing Then
            If Not ProcessKTWE.HasExited Then
                ok = True
            End If
        Else
            ok = False
        End If
        Return ok
    End Function

    Private Sub StartProcessKTWE()
        LogString("KTWE 2017 Reader Start Server process ")
        ProcessKTWE = Nothing
        ProcessKTWE = New Process()
            Dim FileName As String
            Dim DirName As String
        FileName = GetMyDir() + "\KTWE2017_PROC.exe"
        DirName = GetMyDir()

        ProcessKTWE.StartInfo.FileName = FileName
        ProcessKTWE.StartInfo.WorkingDirectory = DirName
        ProcessKTWE.Start()

    End Sub


    Public Sub OnStart(ByVal args() As String)
        If (pMainThread Is Nothing) Then
            pMainThread = New System.Threading.Thread(New System.Threading.ThreadStart(AddressOf MyThread))
            pMainThread.SetApartmentState(System.Threading.ApartmentState.MTA)
            pMainThread.Name = "KTWE 2017 Reader server thread"
        End If

        m_StopServer = False
        pMainThread.Start()



    End Sub
    Private pMainThread As System.Threading.Thread

    ' принудительный рестарт сервера
    Private RestartXRCounter As Long = 0
    Private Inited As Boolean = False



    Public Function Init() As Boolean


        Inited = True
        Return Inited
    End Function

    Private Sub MyThread()
        RestartXRCounter = 360 * 24
        While (Not m_StopServer)

            Init()

            If Not CheckProcessKTWE() Then
                CleanProcessKTWE()
                StartProcessKTWE()

            End If

            If m_StopServer Then Return

            If RestartXRCounter > 0 Then
                RestartXRCounter = RestartXRCounter - 1
            Else
                If Not CheckProcessKTWE() Then
                    CleanProcessKTWE()
                    StartProcessKTWE()
                End If
                RestartXRCounter = 360 * 24
            End If


            If m_StopServer Then Return


            System.Threading.Thread.Sleep(1000)
        End While

    End Sub

    Private m_StopServer As Boolean = False

    Public Sub OnStop()

        LogString("Stopping KTWE 2017 Reader processor")
        m_StopServer = True
        System.Threading.Thread.Sleep(15000)

again:
        Try
            CleanProcessKTWE()
        Catch ex As Exception
            LogString(ex.Message)
        End Try



        LogString("KTWE 2017 Server stopped")

    End Sub



    Public Sub New()
    End Sub
End Class
