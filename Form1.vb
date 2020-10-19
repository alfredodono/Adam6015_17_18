Imports Advantech.Adam
Imports System.Net.Sockets

Public Class Form1
    Private m_bStart As Boolean
    Private adamModbus As AdamSocket
    Private adamUDP As AdamSocket
    Private m_Adam6000Type As Adam6000Type
    Private m_szIP As String
    Private m_szFwVersion As String
    Const m_Adam6000M3FwVer As Integer = 5
    Private m_DeviceFwVer As Integer = 4
    Private m_iPort As Integer
    Private m_iCount As Integer
    Private m_iAiTotal As Integer, m_iDoTotal As Integer
    Private m_bChEnabled() As Boolean
    Private m_byRange As Byte()
    Private m_usRange As UShort() 'for newer version Adam

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        m_bStart = False   ' the action stops at the beginning
        m_szIP = "169.254.219.134" ' modbus slave IP address
        m_iPort = 502    ' modbus TCP port is 502
        adamModbus = New AdamSocket
        adamModbus.SetTimeout(1000, 1000, 1000) ' set timeout for TCP
        adamUDP = New AdamSocket
        adamUDP.SetTimeout(1000, 1000, 1000) ' set timeout for UDP

        m_Adam6000Type = Adam6000Type.Adam6060
        ' the sample is for ADAM-6015
        'm_Adam6000Type = Adam6000Type.Adam6017 ' the sample is for ADAM-6017
        'm_Adam6000Type = Adam6000Type.Adam6018 ' the sample is for ADAM-6018

        adamUDP.Connect(AdamType.Adam6000, m_szIP, ProtocolType.Udp)
        If (adamUDP.Configuration().GetFirmwareVer(m_szFwVersion) = True) Then
            m_DeviceFwVer = Integer.Parse(m_szFwVersion.Trim().Substring(0, 1))
        End If
        adamUDP.Disconnect()

        m_iAiTotal = AnalogInput.GetChannelTotal(m_Adam6000Type)
        m_iDoTotal = DigitalOutput.GetChannelTotal(m_Adam6000Type)

        txtModule.Text = m_Adam6000Type.ToString()
        m_bChEnabled = New Boolean(m_iAiTotal - 1) {}

        If (m_DeviceFwVer < m_Adam6000M3FwVer) Then
            m_byRange = New Byte(m_iAiTotal - 1) {}
        Else 'for newer version Adam
            m_usRange = New UShort(m_iAiTotal - 1) {}
        End If

        ' arrange channel text box

        If (m_Adam6000Type = Adam6000Type.Adam6015) Then
            ' Channel

            ' DO
            panelDO.Visible = False
        ElseIf (m_Adam6000Type = Adam6000Type.Adam6017) Then
            ' DO
            btnCh2.Visible = False
            txtCh2.Visible = False
            btnCh3.Visible = False
            txtCh3.Visible = False
            btnCh4.Visible = False
            txtCh4.Visible = False
            btnCh5.Visible = False
            txtCh5.Visible = False

        Else  'Adam6018
        End If
    End Sub

    Private Sub Form1_FormClosing(ByVal sender As System.Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles MyBase.FormClosing
        adamUDP.Disconnect()

        If (m_bStart = True) Then
            Timer1.Enabled = False
            adamModbus.Disconnect() ' disconnect slave
        End If
    End Sub

    Private Sub RefreshChannelRangeByteFormat(ByVal i_iChannel As Integer)
        Dim byRange As Byte
        If (adamModbus.AnalogInput().GetInputRange(i_iChannel, byRange) = True) Then
            m_byRange(i_iChannel) = byRange
        Else
            txtReadCount.Text += "GetRangeCode() failed"
        End If
    End Sub

    Private Sub RefreshChannelRangeUshortFormat(ByVal i_iChannel As Integer)
        'for newer version Adam
        Dim usRange As UShort
        If (adamModbus.AnalogInput().GetInputRange(i_iChannel, usRange) = True) Then
            m_usRange(i_iChannel) = usRange
        Else
            txtReadCount.Text += "GetRangeCode() failed"
        End If
    End Sub

    Private Sub RefreshChannelEnabled()
        Dim bEnabled() As Boolean

        If (adamModbus.AnalogInput().GetChannelEnabled(m_iAiTotal, bEnabled) = True) Then
            Array.Copy(bEnabled, 0, m_bChEnabled, 0, m_iAiTotal)

            If (m_bChEnabled(0) = False) Then
                txtCh0.Text = ""
            End If
            If (m_bChEnabled(1) = False) Then
                txtCh1.Text = ""
            End If
            If (m_bChEnabled(2) = False) Then
                txtCh2.Text = ""
            End If
            If (m_bChEnabled(3) = False) Then
                txtCh3.Text = ""
            End If
            If (m_bChEnabled(4) = False) Then
                txtCh4.Text = ""
            End If
            If (m_bChEnabled(5) = False) Then
                txtCh5.Text = ""
            End If

        Else
            txtReadCount.Text += "GetChannelEnabled() failed"
        End If
    End Sub

    Private Sub buttonStart_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles buttonStart.Click
        If (m_bStart = True) Then ' was started
            panelDO.Enabled = False
            m_bStart = False  ' starting flag
            Timer1.Enabled = False ' disable timer
            adamModbus.Disconnect() ' disconnect slave
            buttonStart.Text = "Start"
        Else ' was stoped
            If (adamModbus.Connect(m_szIP, ProtocolType.Tcp, m_iPort)) Then

                panelDO.Enabled = True
                m_iCount = 0 ' reset the reading counter
                Timer1.Enabled = True ' enable timer
                buttonStart.Text = "Stop"
                m_bStart = True ' starting flag

                If (m_DeviceFwVer < m_Adam6000M3FwVer) Then

                    If (m_Adam6000Type = Adam6000Type.Adam6017 Or m_Adam6000Type = Adam6000Type.Adam6018) Then
                        RefreshChannelRangeByteFormat(7)
                    End If

                    RefreshChannelRangeByteFormat(6)
                    RefreshChannelRangeByteFormat(5)
                    RefreshChannelRangeByteFormat(4)
                    RefreshChannelRangeByteFormat(3)
                    RefreshChannelRangeByteFormat(2)
                    RefreshChannelRangeByteFormat(1)
                    RefreshChannelRangeByteFormat(0)

                Else

                    'for newer version Adam
                    If (m_Adam6000Type = Adam6000Type.Adam6017 Or m_Adam6000Type = Adam6000Type.Adam6018) Then
                        RefreshChannelRangeUshortFormat(7)
                    End If
                    RefreshChannelRangeUshortFormat(6)
                    RefreshChannelRangeUshortFormat(5)
                    RefreshChannelRangeUshortFormat(4)
                    RefreshChannelRangeUshortFormat(3)
                    RefreshChannelRangeUshortFormat(2)
                    RefreshChannelRangeUshortFormat(1)
                    RefreshChannelRangeUshortFormat(0)

                End If

                RefreshChannelEnabled()
            Else
                MessageBox.Show("Connect to " + m_szIP + " failed", "Error")
            End If
        End If
    End Sub

    Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer1.Tick
        Timer1.Enabled = False

        m_iCount = m_iCount + 1 ' increment the reading counter
        txtReadCount.Text = "Read coil " + m_iCount.ToString() + " times..."

        If (m_DeviceFwVer < m_Adam6000M3FwVer) Then

        Else

        End If

        RefreshDO()

        Timer1.Enabled = True
    End Sub

    Private Sub RefreshDO()
        Dim iStart As Integer = 17
        Dim bData() As Boolean = New Boolean(m_iDoTotal - 1) {}

        If (m_iDoTotal = 0) Then

        Else
            If (adamModbus.Modbus().ReadCoilStatus(iStart, m_iDoTotal, bData)) Then

                If (m_iDoTotal > 0) Then
                    txtCh0.Text = bData(0).ToString()
                End If
                If (m_iDoTotal > 1) Then
                    txtCh1.Text = bData(1).ToString()
                End If
                If (m_iDoTotal > 2) Then
                    txtCh2.Text = bData(2).ToString()
                End If
                If (m_iDoTotal > 3) Then
                    txtCh3.Text = bData(3).ToString()
                End If
                If (m_iDoTotal > 4) Then
                    txtCh4.Text = bData(4).ToString()
                End If
                If (m_iDoTotal > 5) Then
                    txtCh5.Text = bData(5).ToString()
                End If

            Else
                txtCh0.Text = "Fail"
                txtCh1.Text = "Fail"
                txtCh2.Text = "Fail"
                txtCh3.Text = "Fail"
                txtCh4.Text = "Fail"
                txtCh5.Text = "Fail"

            End If
        End If
    End Sub










    Private Sub btnCh_Click(ByVal i_iCh As Integer, ByVal txtBox As TextBox)
        Dim iOnOff As Integer, iStart As Integer = 17 + i_iCh

        Timer1.Enabled = False
        If (txtBox.Text = "True") Then ' was ON, now set to OFF
            iOnOff = 0
        Else
            iOnOff = 1
        End If
        If (adamModbus.Modbus().ForceSingleCoil(iStart, iOnOff)) Then
            RefreshDO()
        Else
            MessageBox.Show("Set digital output failed!", "Error")
        End If
        Timer1.Enabled = True
    End Sub

    Private Sub btnCh0_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCh0.Click
        btnCh_Click(0, txtCh0)
    End Sub

    Private Sub btnCh1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCh1.Click
        btnCh_Click(1, txtCh1)
    End Sub

    Private Sub btnCh2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCh2.Click
        btnCh_Click(2, txtCh2)
    End Sub

    Private Sub btnCh3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCh3.Click
        btnCh_Click(3, txtCh3)
    End Sub

    Private Sub btnCh4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCh4.Click
        btnCh_Click(4, txtCh4)
    End Sub

    Private Sub btnCh5_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCh5.Click
        btnCh_Click(5, txtCh5)
    End Sub


End Class
