Imports System.IO
Imports HttpPostRequestLib
Public Class frmMain
    Declare Function GetForegroundWindow Lib "user32" () As IntPtr
    Declare Auto Function GetWindowRect Lib "User32" (ByVal hWnd As IntPtr, _
      ByRef lpRect As Rect) As Int32
    Dim WithEvents Uploader As HttpPostRequestLib.Net.HTTPPostRequest
    Public Structure Rect
        Public Left As Int32
        Public Top As Int32
        Public Right As Int32
        Public Bottom As Int32
    End Structure
    Dim WithEvents HotKey As New clsHotKey(Me)
    Dim RunningBackground As Boolean = False
    Dim ConfigPath As String = Path.Combine(My.Computer.FileSystem.SpecialDirectories.CurrentUserApplicationData, "config.ini")
    Private Sub frmMain_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If File.Exists(ConfigPath) Then
            Dim Config() As String = File.ReadAllLines(ConfigPath)
            cbModifier.SelectedIndex = Config(1)
            cbHotkey.SelectedIndex = Config(2)
            cbModifier_Area.SelectedIndex = Config(6)
            cbHotkey_Area.SelectedIndex = Config(7)
            tbURL.Text = Config(3)

            tbUsername.Text = Config(4)
            tbPassword.Text = Config(5)
            Uploader = New Net.HTTPPostRequest(tbURL.Text)
            btnOK.Enabled = True
            RunningBackground = True
            Me.Opacity = 0
            Me.WindowState = FormWindowState.Minimized
            HotKey.AddHotKey(cbHotkey.SelectedIndex + 112, GetModFlags(cbModifier.SelectedIndex), "SaveScreenShot")
            HotKey.AddHotKey(cbHotkey_Area.SelectedIndex + 112, GetModFlags(cbModifier_Area.SelectedIndex), "SelectArea")
        Else
            Try
                Directory.CreateDirectory(My.Computer.FileSystem.SpecialDirectories.CurrentUserApplicationData)
            Catch ex As Exception
            End Try
            cbModifier.SelectedIndex = 0
            cbHotkey.SelectedIndex = 10
            cbModifier_Area.SelectedIndex = 0
            cbHotkey_Area.SelectedIndex = 11
        End If
    End Sub
    Function GetModFlags(id As Integer) As Integer
        Select Case id
            Case 0
                Return 1
            Case 1
                Return 2
            Case 2
                Return 4
            Case 3
                Return 8
        End Select
    End Function
    Private Sub frmMain_Shown(sender As Object, e As EventArgs) Handles Me.Shown
        If RunningBackground = True Then
            Me.Hide()
            Me.Opacity = 1
        End If
    End Sub
    Dim SelectMode As Boolean = False
    Dim FirstMouse As Point
    Dim SecondMouse As Point
    Private Sub HotKey_HotKeyPressed(HotKeyID As String) Handles HotKey.HotKeyPressed
        Select Case HotKeyID
            Case "SaveScreenShot"
                GetWindow()
            Case "SelectArea"
                If SelectMode = False Then
                    FirstMouse = Cursor.Position
                    SelectMode = True
                    niShots.Icon = My.Resources.cloudred
                    niShots.ShowBalloonTip(1000, "WoodShots", "Bitte wähle den zweiten Punkt des Bereiches!", ToolTipIcon.Info)
                Else
                    SelectMode = False
                    SecondMouse = Cursor.Position
                    If (FirstMouse.X < SecondMouse.X) And (FirstMouse.Y < SecondMouse.Y) Then
                        GetScreenshot(SecondMouse.X - FirstMouse.X, SecondMouse.Y - FirstMouse.Y, FirstMouse.X, FirstMouse.Y)
                    Else
                        ' GetScreenshot(FirstMouse.X - SecondMouse.X, FirstMouse.Y - SecondMouse.Y, SecondMouse.X, SecondMouse.Y)
                        niShots.ShowBalloonTip(1000, "WoodShots", "Bitte erst den Punkt oben links markieren!", ToolTipIcon.Error)
                    End If
                End If
        End Select
    End Sub
    Private Sub GetWindow()
        Dim r As New Rect
        GetWindowRect(GetForegroundWindow, r)
        GetScreenshot(r.Right - r.Left, r.Bottom - r.Top, r.Left, r.Top)
    End Sub

    Private Sub GetScreenshot(sizex As Integer, sizey As Integer, startpointx As Integer, startpointy As Integer)
        Dim sbitmap As New Bitmap(sizex, sizey)
        Dim sgraphics As Graphics = Graphics.FromImage(sbitmap)
        Dim smemorystream As New IO.MemoryStream
        sgraphics.CopyFromScreen(New Point(startpointx, startpointy), New Point(0, 0), New Size(sizex, sizey))
        sbitmap.Save(smemorystream, System.Drawing.Imaging.ImageFormat.Png)
        UploadImage(smemorystream.ToArray)
    End Sub

    Private Sub UploadImage(imagedata As Byte())
        Uploader = New Net.HTTPPostRequest(tbURL.Text)
        Uploader.AddVar("username", tbUsername.Text)
        Uploader.AddVar("password", tbPassword.Text)
        Uploader.AddFile("image", "upload.png", imagedata)
        Uploader.SubmitASync()
        niShots.Icon = My.Resources.cloudgreen
    End Sub
    Private Sub btnAbort_Click(sender As Object, e As EventArgs) Handles btnAbort.Click
        Me.Hide()
        Me.Opacity = 1
    End Sub
    Private Sub btnOK_Click(sender As Object, e As EventArgs) Handles btnOK.Click
        Me.Hide()
        Me.Opacity = 1
        System.IO.File.WriteAllText(ConfigPath, "//Vorsicht! Die Konfigspeicherfrickelei ist extrem ranzig. Hier besser nicht selbst Hand anlegen!" + vbNewLine + cbModifier.SelectedIndex.ToString + vbNewLine + cbHotkey.SelectedIndex.ToString + vbNewLine + tbURL.Text + vbNewLine + tbUsername.Text + vbNewLine + tbPassword.Text + vbNewLine + cbModifier_Area.SelectedIndex.ToString + vbNewLine + cbHotkey_Area.SelectedIndex.ToString())
        Try
            HotKey.RemoveHotKey("SaveScreenShot")
            HotKey.RemoveHotKey("SelectArea")
        Catch ex As Exception
        End Try
        HotKey.AddHotKey(cbHotkey.SelectedIndex + 112, GetModFlags(cbModifier.SelectedIndex), "SaveScreenShot")
        HotKey.AddHotKey(cbHotkey_Area.SelectedIndex + 112, GetModFlags(cbModifier_Area.SelectedIndex), "SelectArea")
    End Sub
    Private Sub btnAbout_Click(sender As Object, e As EventArgs) Handles btnAbout.Click
        MessageBox.Show(My.Resources.abouttext, "WoodShots", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub
    Private Sub frmMain_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        niShots.Visible = False
    End Sub
    Private Sub niShots_DoubleClick(sender As Object, e As EventArgs) Handles niShots.DoubleClick

        Me.Show()
        Me.WindowState = FormWindowState.Normal
    End Sub
    Private Sub BeendenToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles BeendenToolStripMenuItem.Click
        Me.Opacity = 0
        Me.Show()
        Close()
    End Sub
    Private Sub btnTest_Click(sender As Object, e As EventArgs) Handles btnTest.Click
        TestMode = True
        Uploader = New Net.HTTPPostRequest(tbURL.Text)
        Uploader.AddVar("username", tbUsername.Text)
        Uploader.AddVar("password", tbPassword.Text)
        Uploader.AddFile("image", "upload.png", My.Resources.thisworks)
        Uploader.SubmitASync()
        
    End Sub
    Dim TestMode As Boolean = False
    Private Sub Uploader_SubmitComplete(sender As Object, e As Net.SubmitEventArgs) Handles Uploader.SubmitComplete
        If TestMode Then
            TestMode = False
            If e.Sourcecode.StartsWith("#success") Then
                btnOK.Enabled = True
                MessageBox.Show("Test erfolgreich!", "WoodShots", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Else
                btnOK.Enabled = False
                MessageBox.Show("Seite gibt nicht die korrekten Daten zurück!" + vbNewLine + vbNewLine + "Die Daten sollten folgendermaßen übertragen werden:" + vbNewLine + "#success http://bildurlhier", "WoodShots", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End If
        Else
            If e.Sourcecode.StartsWith("#success") Then
                niShots.Icon = My.Resources.cloud
                niShots.ShowBalloonTip(1000)
                Try
                    My.Computer.Clipboard.SetText(e.Sourcecode.Split(" ")(1))
                Catch ex As Exception
                End Try
            Else

            End If
        End If
    End Sub
 
    Private Sub tbURL_TextChanged(sender As Object, e As EventArgs) Handles tbURL.TextChanged
        btnOK.Enabled = False
    End Sub

    Private Sub tbUsername_TextChanged(sender As Object, e As EventArgs) Handles tbUsername.TextChanged
        btnOK.Enabled = False
    End Sub

    Private Sub tbPassword_TextChanged(sender As Object, e As EventArgs) Handles tbPassword.TextChanged
        btnOK.Enabled = False
    End Sub
End Class
