<System.ComponentModel.RunInstaller(True)> Partial Class KTWEServiceInstaller
    Inherits System.Configuration.Install.Installer

    'Installer overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Component Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Component Designer
    'It can be modified using the Component Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.KTWEServicePrc = New System.ServiceProcess.ServiceProcessInstaller()
        Me.KTWEServiceInst = New System.ServiceProcess.ServiceInstaller()
        '
        'KTWEServicePrc
        '
        Me.KTWEServicePrc.Password = Nothing
        Me.KTWEServicePrc.Username = Nothing
        '
        'KTWEServiceInst
        '
        Me.KTWEServiceInst.Description = "KTWE RFID Reader Servic 2017"
        Me.KTWEServiceInst.DisplayName = "KTWEService2017"
        Me.KTWEServiceInst.ServiceName = "KTWEService2017"
        '
        'KTWEServiceInstaller
        '
        Me.Installers.AddRange(New System.Configuration.Install.Installer() {Me.KTWEServicePrc, Me.KTWEServiceInst})

    End Sub
    Friend WithEvents KTWEServiceInst As System.ServiceProcess.ServiceInstaller
    Friend WithEvents KTWEServicePrc As System.ServiceProcess.ServiceProcessInstaller
End Class
