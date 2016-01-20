
Imports System.Net
Imports System.IO
Imports System.Threading
Imports System.Windows.Forms
Imports System.Drawing


<ComClass(FtpSolution.ClassId, FtpSolution.InterfaceId, FtpSolution.EventsId)> _
Public Class FtpSolution

#Region "COM GUIDs"

    ' These  GUIDs provide the COM identity for this class 
    ' and its COM interfaces. If you change them, existing 
    ' clients will no longer be able to access the class.
    Public Const ClassId As String = "463dfed1-a385-4faa-b7ae-2e7079f06cfe"
    Public Const InterfaceId As String = "1d4bf8ea-93b1-4061-a9e7-b6456a604d74"
    Public Const EventsId As String = "b9597139-89f1-4c05-846d-16dcdd75a71e"

#End Region

    Private _FtpUser As String 'Usuário Ftps
    Private _FtpDomain As String 'Dominio Ftp
    Private _FtpPass As String 'Senha Ftp
    Private _Credentials As System.Net.NetworkCredential
    Private _UploadedPath As String 'Caminho onde foi salvo o arquivo durante upload
    Private _DownloadedPath As String 'Caminho de um diretório Windows para download
    Private _DeletedPath As String 'Caminho do arquivo deletado após a chamada do método deleteFile
    Private _DirWeb As String 'Local onde salvar o arquivo no ftp

    'Utilizado apenas em caso de download pelo método MakeDir 
    Private _date As String
    Private _hour As String
    Private _dateAndHour As String


#Region "Construtores"

    ''' <summary>
    ''' Classe utilizada para Upload e Download de arquivos utilizando requisição FTP(File Transfer Protocol)
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub New()
        MyBase.New()
    End Sub

    ''' <summary>
    ''' Classe utilizada para Upload e Download de arquivos utilizando requisição FTP(File Transfer Protocol)
    ''' </summary>
    ''' <param name="FTPUser">Usuário do servidor FTP</param>
    ''' <param name="FTPPass">Senha de Usuário do servidor FTP</param>
    ''' <remarks></remarks>
    Public Sub New(ByVal ftpUser As String, ByVal ftpPass As String, ByVal ftpDomain As String)

        MyBase.New()

        'Credenciais de acesso ao ftp
        Me.setCredentials(ftpUser, ftpPass, ftpDomain)

    End Sub

#End Region

#Region "UploadFiles"

    ''' <summary>
    ''' Realiza o Upload de Arquivos para um servidor FTP (File Transfer Protocol)
    ''' </summary>
    ''' <param name="fileName"> Arquivo que será Upado</param>
    ''' <param name="uploadPath">Caminho onde será salvo o arquivo no FTP </param>
    ''' <remarks></remarks>
    ''' <exception cref="WebException">Caso não seja possível acessar o caminho Ftp informado para upload.</exception>
    Public Overloads Sub uploadFile(ByVal fileName As String, ByVal uploadPath As String)
        ''_FileName As String, _UploadPath As String

        uploadPath = trataUrlDirFtp(uploadPath)

        Dim _FileInfo As New System.IO.FileInfo(fileName)

        ' Create FtpWebRequest object from the Uri provided
        Dim _FtpWebRequest As System.Net.FtpWebRequest = CType(System.Net.FtpWebRequest.Create(New Uri(uploadPath)), System.Net.FtpWebRequest)

        ' Provide the WebPermission Credintials
        _FtpWebRequest.Credentials = _Credentials  ''recupera o valor dos atributos da classe para senha e usuário

        ' By default KeepAlive is true, where the control connection is not closed
        ' after a command is executed.
        _FtpWebRequest.KeepAlive = False

        Dim proxy = _FtpWebRequest.Proxy
        proxy = Nothing

        ' set timeout for 20 seconds
        _FtpWebRequest.Timeout = 20000

        ' Specify the command to be executed.
        _FtpWebRequest.Method = System.Net.WebRequestMethods.Ftp.UploadFile

        ' Specify the data transfer type.
        _FtpWebRequest.UseBinary = True

        ' Notify the server about the size of the uploaded file
        _FtpWebRequest.ContentLength = _FileInfo.Length

        ' The buffer size is set to 2kb
        Dim buffLength As Integer = 2048
        Dim buff(buffLength - 1) As Byte

        ' Opens a file stream (System.IO.FileStream) to read the file to be uploaded
        Dim _FileStream As System.IO.FileStream = _FileInfo.OpenRead()

        Try

            ' Stream to which the file to be upload is written
            Dim _Stream As System.IO.Stream = _FtpWebRequest.GetRequestStream()

            ' Read from the file stream 2kb at a time
            Dim contentLen As Integer = _FileStream.Read(buff, 0, buffLength)

            ' Till Stream content ends
            Do While contentLen <> 0
                ' Write Content from the file stream to the FTP Upload Stream
                _Stream.Write(buff, 0, contentLen)
                contentLen = _FileStream.Read(buff, 0, buffLength)
            Loop

            Me.UploadedPath = uploadPath

            ' Close the file stream and the Request Stream
            _Stream.Close()
            _Stream.Dispose()
            _FileStream.Close()
            _FileStream.Dispose()

        Catch ex As WebException
            Throw New WebException("Não Foi possível realizar a ação solicitada." + vbCrLf + "MÉTODO : uploadFile" + vbCrLf + ex.Message)
        Catch ex As Exception
            Throw New Exception("Ocorreram problemas durante o upload do arquivo." + vbCrLf + "MÉTODO : uploadFile" + vbCrLf + ex.Message)
        End Try
    End Sub

#End Region

#Region "DeleteFiles"

    Public Overloads Sub deleteFile(ByVal nomeArquivoADeletar As String)

        nomeArquivoADeletar = trataUrlDirFtp(nomeArquivoADeletar)

        ' Create FtpWebRequest object from the Uri provided
        Dim _FtpWebRequest As System.Net.FtpWebRequest = CType(System.Net.FtpWebRequest.Create(New Uri(nomeArquivoADeletar)), System.Net.FtpWebRequest)

        ' Provide the WebPermission Credintials
        _FtpWebRequest.Credentials = _Credentials  ''recupera o valor dos atributos da classe para senha e usuário

        ' By default KeepAlive is true, where the control connection is not closed
        ' after a command is executed.
        _FtpWebRequest.KeepAlive = False

        Dim proxy = _FtpWebRequest.Proxy
        proxy = Nothing

        ' set timeout for 20 seconds
        _FtpWebRequest.Timeout = 20000

        ' Specify the command to be executed.
        _FtpWebRequest.Method = System.Net.WebRequestMethods.Ftp.DeleteFile

        ' Specify the data transfer type.
        _FtpWebRequest.UseBinary = True

        Try
            ' Stream to which the file to be upload is written
            Dim resposta As FtpWebResponse = _FtpWebRequest.GetResponse()
            Me.DeletedPath = nomeArquivoADeletar

        Catch ex As WebException
            Throw New WebException("Não Foi possível realizar a ação solicitada." + vbCrLf + "MÉTODO : deleteFile" + vbCrLf + ex.Message)
        Catch ex As Exception
            Throw New Exception("Ocorreram problemas durante o upload do arquivo." + vbCrLf + "MÉTODO : deleteFile" + vbCrLf + ex.Message)
        End Try

    End Sub

#End Region

#Region "DownloadScript"

    Public Overloads Sub DownloadScripts(ByRef ftpPath As String, ByVal destinePath As String)

        'Utilizado na progressBar
        Dim intFinal As Integer
        Dim intInicio As Integer

        ' Faz o Download do arquivo definido via FTP e salva em uma pasta da aplicação
        Dim strArquivos As String
        Dim readBuffer(8064) As Byte
        Dim contador As Integer
        Dim pbDownload As New ProgressBar
        Dim lbMensagem As New Label
        Dim request As FtpWebRequest
        Dim respostaFTP As FtpWebResponse
        Dim respostaStream As IO.Stream
        Dim arquivoSaida As IO.FileStream
        Dim pastaDestino As String


        'Recupera e versão atual do sistema e define o tamanho da progressbar
        intFinal = 101 'My.Application.Info.Version.Minor
        intInicio = 1

        'Define a pasta que será utilizada para armazenar os Scripts baixados
        Me.DownloadedPath = destinePath

        'Verifica se a pasta para donwload exite caso não exista ela será criada
        If Dir(DownloadedPath(), vbDirectory) = "" Then
            DownloadedPath() = "C:\Titanium\Scripts"
        Else
            'Se a pasta existe verifica se e quais scripts de versão já estão na pasta
            strArquivos = Dir("C:\Titanium\Scripts\")
            While strArquivos <> ""
                If InStr(strArquivos, ".sql") > 0 Then
                    intInicio = Replace(Mid(strArquivos, InStr(1, strArquivos, "v") + 1), ".sql", "")
                End If
                strArquivos = Dir()
            End While
        End If

        pbDownload.Maximum = (intFinal - intInicio) + 1
        pbDownload.Value = 0
        pbDownload.Visible = True
        pbDownload.Show()
        pbDownload.SuspendLayout()

        Dim form As New Form
        form.Show()

        Try  'Realiza o donwload dos arquivos 

            While intInicio <= intFinal

                lbMensagem.Text = "Baixando arquivo Script_v" & CStr(intInicio).PadLeft(4, "0") & ".sql"
                ' ----- Obtem local onde irá salvar o arquivo
                pastaDestino = My.Computer.FileSystem.CombinePath(DownloadedPath(), "Script_v" & My.Computer.FileSystem.GetName(ftpPath & CStr(intInicio).PadLeft(4, "0") & ".sql"))

                ' ----- Faz a conexao com o arquivo no site FTP
                request = CType(FtpWebRequest.Create(New Uri(ftpPath & "Script_v" & CStr(intInicio).PadLeft(4, "0") & ".sql")), FtpWebRequest)
                request.Credentials = _Credentials
                request.KeepAlive = False
                request.UseBinary = True
                request.Method = WebRequestMethods.Ftp.DownloadFile

                ' ----- Abre um canal de transmissão para o arquivo
                respostaFTP = CType(request.GetResponse, FtpWebResponse)
                respostaStream = respostaFTP.GetResponseStream
                arquivoSaida = New FileStream(pastaDestino, FileMode.Create)

                ' ----- Salva o conteúdo do arquivo de saida bloco a bloco
                Do
                    contador = respostaStream.Read(readBuffer, 0, readBuffer.Length)
                    arquivoSaida.Write(readBuffer, 0, contador)

                Loop Until contador = 0

                If FileLen(destinePath & "Script_v" & CStr(intInicio).PadLeft(4, "0") & ".sql") > 0 Then
                    intInicio = intInicio + 1
                    pbDownload.Value = pbDownload.Value + 1
                ElseIf (File.Exists(destinePath & "Script_v" & CStr(intInicio).PadLeft(4, "0") & ".sql")) Then
                    intInicio = intInicio + 1
                    pbDownload.Value = pbDownload.Value + 1
                End If

                ' ----- libera recursos.
                respostaStream.Close()
                arquivoSaida.Flush()
                arquivoSaida.Close()
                respostaFTP.Close()

            End While


        Catch ex As Exception

            MsgBox("Erro durante a realização do Download " + vbCrLf + ex.Message)

            Return

        End Try


    End Sub

    Public Overloads Sub DownloadScripts()

        'Utilizado na progressBar
        Dim intFinal As Integer
        Dim intInicio As Integer

        ' Faz o Download do arquivo definido via FTP e salva em uma pasta da aplicação
        Dim strArquivos As String
        Dim readBuffer(8064) As Byte
        Dim contador As Integer
        Dim form As New Form
        Dim pbDownload As New ProgressBar
        Dim lbMensagem As New Label
        Dim request As FtpWebRequest
        Dim respostaFTP As FtpWebResponse
        Dim respostaStream As IO.Stream
        Dim arquivoSaida As IO.FileStream
        Dim pastaDestino As String
        Dim formulario As New FormDonwload


        'Recupera e versão atual do sistema e define o tamanho da progressbar
        intFinal = 101 'My.Application.Info.Version.Minor
        intInicio = 1

        ''Define a pasta que será utilizada para armazenar os Scripts baixados
        'Me.downloadPath = destinePath

        'Verifica se a pasta para donwload exite caso não exista ela será criada
        If Dir(DownloadedPath(), vbDirectory) = "" Then
            DownloadedPath() = "C:\Titanium\Scripts"
        Else
            'Se a pasta existe verifica se e quais scripts de versão já estão na pasta
            strArquivos = Dir("C:\Titanium\Scripts\")
            While strArquivos <> ""
                If InStr(strArquivos, ".sql") > 0 Then
                    intInicio = Replace(Mid(strArquivos, InStr(1, strArquivos, "v") + 1), ".sql", "")
                End If
                strArquivos = Dir()
            End While
        End If

        progressbarScript(formulario)
        formulario._progress.Maximum = (intFinal - intInicio) + 1
        formulario._progress.Value = 0
        't1.Start()

        Try  'Realiza o donwload dos arquivos 

            While intInicio <= intFinal

                'lbMensagem.Text = "Baixando arquivo Script_v" & CStr(intInicio).PadLeft(4, "0") & ".sql"
                formulario.lbMensagem() = "Baixando arquivo Script_v" & CStr(intInicio).PadLeft(4, "0") & ".sql"

                ' ----- Obtem local onde irá salvar o arquivo
                pastaDestino = My.Computer.FileSystem.CombinePath(DownloadedPath(), "Script_v" & My.Computer.FileSystem.GetName(UploadedPath() & CStr(intInicio).PadLeft(4, "0") & ".sql"))

                ' ----- Faz a conexao com o arquivo no site FTP
                request = CType(FtpWebRequest.Create(New Uri(UploadedPath() & "Script_v" & CStr(intInicio).PadLeft(4, "0") & ".sql")), FtpWebRequest)
                request.Credentials = _Credentials
                request.KeepAlive = False
                request.UseBinary = True
                request.Method = WebRequestMethods.Ftp.DownloadFile

                ' ----- Abre um canal de transmissão para o arquivo
                respostaFTP = CType(request.GetResponse, FtpWebResponse)
                respostaStream = respostaFTP.GetResponseStream
                arquivoSaida = New FileStream(pastaDestino, FileMode.Create)

                ' ----- Salva o conteúdo do arquivo de saida bloco a bloco
                Do
                    contador = respostaStream.Read(readBuffer, 0, readBuffer.Length)
                    arquivoSaida.Write(readBuffer, 0, contador)

                Loop Until contador = 0

                If FileLen(DownloadedPath() & "Script_v" & CStr(intInicio).PadLeft(4, "0") & ".sql") > 0 Then
                    intInicio = intInicio + 1
                    formulario.progresBar = formulario.progresBar() + 1
                ElseIf (File.Exists(DownloadedPath() & "Script_v" & CStr(intInicio).PadLeft(4, "0") & ".sql")) Then
                    intInicio = intInicio + 1
                    formulario.progresBar = formulario.progresBar() + 1
                End If

                ' ----- libera recursos.
                respostaStream.Close()
                arquivoSaida.Flush()
                arquivoSaida.Close()
                respostaFTP.Close()

            End While


        Catch ex As Exception

            MsgBox("Erro durante a realização do Download " + vbCrLf + ex.Message)

            Return

        End Try


    End Sub

#End Region

#Region "ProgressBar"

    Public Sub progressbarScript(form As Object)

        Dim frm = DirectCast(form, FormDonwload)

        frm._form.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        frm._form.StartPosition = FormStartPosition.Manual
        frm._form.ClientSize = New System.Drawing.Size(450, 30)
        frm._form.FormBorderStyle = FormBorderStyle.FixedToolWindow
        frm._form.Location = New Point(My.Computer.Screen.Bounds.Width - frm._form.Size.Width, My.Computer.Screen.Bounds.Height - frm._form.Size.Height - 30)
        frm._form.Controls.Add(frm._lbmensagem)
        frm._form.Controls.Add(frm._progress)
        frm._form.Text = "Download de Scripts"

        frm._progress.Size = New System.Drawing.Size(frm._form.Size.Width - 18, 10)
        frm._progress.Anchor = AnchorStyles.Right
        frm._progress.ForeColor = Color.GreenYellow
        frm._progress.Location = New Point(0, 15)
        frm._progress.Show()

        frm._lbmensagem.Location = New Point(0, 0)
        frm._lbmensagem.BackColor = Color.FromArgb(6, 176, 37)
        frm._lbmensagem.BackColor = Color.Transparent
        frm._lbmensagem.AutoSize = True
        frm._lbmensagem.Visible = False

        frm._form.PerformLayout()
        frm._lbmensagem.Visible = True
        frm._form.Show()

    End Sub

    Public Class FormDonwload

        Public _form As Form
        Public _lbmensagem As Label
        Public _progress As ProgressBar


        Public Sub New()

            _form = New Form
            _lbmensagem = New Label
            _progress = New ProgressBar

        End Sub


        Public Property progresBar() As Integer

            Get

                Return _progress.Value

            End Get

            Set(value As Integer)

                _progress.Value = value

            End Set

        End Property

        Public Property lbMensagem() As String
            Get

                Return _lbmensagem.Text

            End Get

            Set(value As String)

                _lbmensagem.Text = value

            End Set

        End Property


    End Class

#End Region

#Region "Download"

    ''' <summary>
    ''' Realiza o download de Arquivos em um servidor via Ftp (File transfer Protocol)
    ''' O caminho Ftp deve ser setado na chamada da classe com o nome do arquivo de download
    ''' </summary>
    ''' <remarks></remarks>
    Public Overloads Sub DownloadViaFTP()

        ' ----- Faz o Download do arquivo definido via FTP e salva em uma pasta da aplicação
        Dim readBuffer(4095) As Byte
        Dim contador As Integer
        Dim _request As FtpWebRequest
        Dim respostaFTP As FtpWebResponse
        Dim respostaStream As IO.Stream
        Dim arquivoSaida As IO.FileStream
        Dim pastaDestino As String

        ' ----- Obtem local onde irá salvar o arquivo
        pastaDestino = My.Computer.FileSystem.CombinePath(DownloadedPath(), My.Computer.FileSystem.GetName(Me.UploadedPath()))

        Try
            ' ----- Faz a conexao com o arquivo no site FTP
            _request = CType(FtpWebRequest.Create(New Uri(Me.UploadedPath())), FtpWebRequest)

            _request.Credentials = _Credentials

            _request.KeepAlive = False
            _request.UseBinary = True

            _request.Method = WebRequestMethods.Ftp.DownloadFile

            ' ----- Abre um canal de transmissão para o arquivo
            respostaFTP = CType(_request.GetResponse, FtpWebResponse)
            respostaStream = respostaFTP.GetResponseStream
            arquivoSaida = New FileStream(pastaDestino, FileMode.Create)

            ' ----- Salva o conteúdo do arquivo de saida bloco a bloco
            Do
                contador = respostaStream.Read(readBuffer, 0, readBuffer.Length)
                arquivoSaida.Write(readBuffer, 0, contador)

            Loop Until contador = 0

        Catch ex As Exception

            MsgBox("Erro durante a realização do Download " + vbCrLf + ex.Message)

            Return

        End Try

        respostaStream.Close()
        arquivoSaida.Flush()
        arquivoSaida.Close()
        respostaFTP.Close()

    End Sub

    ''' <summary>
    ''' Realiza o download de Arquivos em um servidor via Ftp (File transfer Protocol)
    ''' o caminho é passado com o nome do arquivo a ser baixado
    ''' </summary>
    ''' <param name="caminhoArquivoFTP">diretório do arquivo que será baixado</param>
    ''' <remarks></remarks>
    Public Overloads Sub DownloadViaFTP(ByVal caminhoArquivoFTP As String)

        ' ----- Faz o Download do arquivo definido via FTP e salva em uma pasta da aplicação
        Dim readBuffer(4095) As Byte
        Dim contador As Integer

        Dim _request As FtpWebRequest
        Dim respostaFTP As FtpWebResponse
        Dim respostaStream As IO.Stream
        Dim arquivoSaida As IO.FileStream
        Dim pastaDestino As String

        ' ----- Obtem local onde irá salvar o arquivo
        pastaDestino = My.Computer.FileSystem.CombinePath(DownloadedPath, My.Computer.FileSystem.GetName(caminhoArquivoFTP))

        Try
            ' ----- Faz a conexao com o arquivo no site FTP
            _request = CType(FtpWebRequest.Create(New Uri(caminhoArquivoFTP)), FtpWebRequest)

            _request.Credentials = _Credentials

            _request.KeepAlive = False
            _request.UseBinary = True

            _request.Method = WebRequestMethods.Ftp.DownloadFile

            ' ----- Abre um canal de transmissão para o arquivo
            respostaFTP = CType(_request.GetResponse, FtpWebResponse)
            respostaStream = respostaFTP.GetResponseStream
            arquivoSaida = New FileStream(pastaDestino, FileMode.Create)

            ' ----- Salva o conteúdo do arquivo de saida bloco a bloco
            Do
                contador = respostaStream.Read(readBuffer, 0, readBuffer.Length)
                arquivoSaida.Write(readBuffer, 0, contador)

            Loop Until contador = 0

        Catch ex As Exception

            MsgBox("Erro durante a realização do Download " + vbCrLf + ex.Message)
            Return

        End Try

        ' ----- libera recursos.
        respostaStream.Close()
        arquivoSaida.Flush()
        arquivoSaida.Close()
        respostaFTP.Close()

    End Sub

    ''' <summary>
    ''' Realiza o download de Arquivos em um servidor via Ftp (File transfer Protocol)
    ''' </summary>
    ''' <param name="ftpPath">Diretório de onde o arquivo será baixado (deve ser passado o nome do arquivo junto com o diretórioFTP) </param>
    ''' <param name="destinePath">caminho onde o arquivo será salvo</param>
    ''' <remarks></remarks>
    Public Overloads Sub DownloadViaFTP(ByVal ftpPath As String, ByVal destinePath As String)

        ' ----- Faz o Download do arquivo definido via FTP e salva em uma pasta da aplicação
        Dim readBuffer(4095) As Byte
        Dim contador As Integer

        Dim _request As FtpWebRequest
        Dim respostaFTP As FtpWebResponse
        Dim respostaStream As IO.Stream
        Dim arquivoSaida As IO.FileStream
        Dim pastaDestino As String


        Me.DownloadedPath = destinePath

        ' ----- Obtem local onde irá salvar o arquivo
        pastaDestino = My.Computer.FileSystem.CombinePath(DownloadedPath(), My.Computer.FileSystem.GetName(ftpPath))

        Try
            ' ----- Faz a conexao com o arquivo no site FTP
            _request = CType(FtpWebRequest.Create(New Uri(ftpPath)), FtpWebRequest)

            _request.Credentials = _Credentials

            _request.KeepAlive = False
            _request.UseBinary = True

            _request.Method = WebRequestMethods.Ftp.DownloadFile

            ' ----- Abre um canal de transmissão para o arquivo
            respostaFTP = CType(_request.GetResponse, FtpWebResponse)
            respostaStream = respostaFTP.GetResponseStream
            arquivoSaida = New FileStream(pastaDestino, FileMode.Create)

            ' ----- Salva o conteúdo do arquivo de saida bloco a bloco
            Do
                contador = respostaStream.Read(readBuffer, 0, readBuffer.Length)
                arquivoSaida.Write(readBuffer, 0, contador)

            Loop Until contador = 0

        Catch ex As Exception

            MsgBox("Erro durante a realização do Download " + vbCrLf + ex.Message)

            Return

        End Try

        ' ----- libera recursos.
        respostaStream.Close()
        arquivoSaida.Flush()
        arquivoSaida.Close()
        respostaFTP.Close()

    End Sub

    ''' <summary>
    ''' Realiza o download de Arquivos em um servidor via Ftp (File transfer Protocol)
    ''' </summary>
    ''' <param name="ftpPath">Caminho de onde o arquivo será baixado</param>
    ''' <param name="destinePath">caminho onde o arquivo será salvo</param>
    ''' <param name="userFtp">nome de usuário para Login </param>
    ''' <param name="passFtp">Senha do usuário para Login </param>
    ''' <remarks></remarks>
    Public Overloads Sub DownloadViaFTP(ByVal ftpPath As String, ByVal destinePath As String, ByVal userFtp As String, ByVal passFtp As String)

        ' ----- Faz o Download do arquivo definido via FTP e salva em uma pasta da aplicação
        Dim readBuffer(4095) As Byte
        Dim contador As Integer

        Dim _request As FtpWebRequest
        Dim respostaFTP As FtpWebResponse
        Dim respostaStream As IO.Stream
        Dim arquivoSaida As IO.FileStream
        Dim pastaDestino As String


        Me.DownloadedPath = destinePath

        ' ----- Obtem local onde irá salvar o arquivo
        pastaDestino = My.Computer.FileSystem.CombinePath(DownloadedPath(), My.Computer.FileSystem.GetName(ftpPath))

        Try
            ' ----- Faz a conexao com o arquivo no site FTP
            _request = CType(FtpWebRequest.Create(New Uri(ftpPath)), FtpWebRequest)

            _request.Credentials = New NetworkCredential(userFtp, passFtp)

            _request.KeepAlive = False
            _request.UseBinary = True

            _request.Method = WebRequestMethods.Ftp.DownloadFile

            ' ----- Abre um canal de transmissão para o arquivo
            respostaFTP = CType(_request.GetResponse, FtpWebResponse)
            respostaStream = respostaFTP.GetResponseStream
            arquivoSaida = New FileStream(pastaDestino, FileMode.Create)

            ' ----- Salva o conteúdo do arquivo de saida bloco a bloco
            Do
                contador = respostaStream.Read(readBuffer, 0, readBuffer.Length)
                arquivoSaida.Write(readBuffer, 0, contador)

            Loop Until contador = 0

            'MsgBox("Download completo! " & vbNewLine & ftpPath & vbNewLine & " em " & pastaDestino)

        Catch ex As Exception

            MsgBox("Erro durante a realização do Download " + vbCrLf + ex.Message)
            Return

        End Try

        ' ----- libera recursos.
        respostaStream.Close()
        arquivoSaida.Flush()
        arquivoSaida.Close()
        respostaFTP.Close()

        'MsgBox("Download completo! " & vbNewLine & ftpPath)

    End Sub


#End Region

#Region "Diversos"

    ''' <summary>
    ''' Renomear arquivos Ftp
    ''' </summary>
    ''' <param name="OldFile">Fullpath do arquivo Ftp</param>
    ''' <param name="NewName">Apenas o novo nome do arquivo</param>
    ''' <remarks></remarks>
    Public Sub RenameFileOnServer(ByVal OldFile As String, ByVal NewName As String)

        Dim MyFtpWebRequest As System.Net.FtpWebRequest = CType(System.Net.FtpWebRequest.Create(OldFile), FtpWebRequest)

        MyFtpWebRequest.Credentials = _Credentials
        MyFtpWebRequest.Method = System.Net.WebRequestMethods.Ftp.Rename

        'novo nome do arquivo
        MyFtpWebRequest.RenameTo() = NewName
        Dim MyResponse As System.Net.FtpWebResponse

        Try
            MyResponse = CType(MyFtpWebRequest.GetResponse, FtpWebResponse)

            Dim MyStatusStr As String = MyResponse.StatusDescription
            Dim MyStatusCode As System.Net.FtpStatusCode = MyResponse.StatusCode

            If MyStatusCode <> Net.FtpStatusCode.FileActionOK Then

                MessageBox.Show("*** Rename " & OldFile & " para " & NewName & " Falhou.  Retornou status = " & MyStatusStr)

            Else

                MessageBox.Show("Rename " & OldFile & " para " & NewName & " ok em " & Now.ToString)

            End If

        Catch ex As Exception

            MessageBox.Show("*** Rename " & OldFile & " to " & NewName & " failed due to the following error: " & ex.Message)

        End Try

    End Sub

    ''' <summary>
    '''  Criar diretórios no FTP (Optional salva um arquivo no diretório criado)
    ''' </summary>
    ''' <param name="_CreatePath">Caminho onde será criado a nova pasta</param>
    ''' <param name="_fileName">OPTIONAL Nome para o arquivo Upado.</param>
    Public Sub MakeDir(ByVal _CreatePath As String, Optional ByVal _fileDirectory As String = "FILE", Optional ByVal _fileName As String = "NEW")

        Dim _Stream As Stream = Nothing

        Try
            _CreatePath = Me.trataUrlDirFtp(_CreatePath)

            If (DiretorioExiste(_CreatePath)) Then
                Console.Write("Diretório já existe." + vbCrLf + "Realizando próxima ação.")
            Else

                Dim _FtpWebRequest As System.Net.FtpWebRequest = CType(System.Net.FtpWebRequest.Create(New Uri(_CreatePath)), System.Net.FtpWebRequest)
                _FtpWebRequest.Method = WebRequestMethods.Ftp.MakeDirectory
                _FtpWebRequest.UseBinary = True
                _FtpWebRequest.Credentials = _Credentials  ''recupera o valor dos atributos da classe para senha e usuário

                Dim response As FtpWebResponse = DirectCast(_FtpWebRequest.GetResponse(), FtpWebResponse)
                _Stream = response.GetResponseStream()
                _Stream.Close()
                response.Close()
            End If

            ''Quando é passado o arquivo o upload é feito logo após a criação do diretório
            If _fileDirectory <> "FILE" Then
                If (_fileName <> "NEW") Then 'Salva o arquivo com o nome especificado no parametro _fileName
                    Me.uploadFile(_fileDirectory, _CreatePath + "/" + _fileName)
                Else
                    _fileName = Me.recuperaNomeArquivoInFullPath(_fileDirectory)
                    uploadFile(_fileDirectory, _CreatePath & "/" & _fileName)
                End If
            End If

        Catch ex As Exception

            If _Stream IsNot Nothing Then   'Close the file stream and the Request Stream
                _Stream.Close()
                _Stream.Dispose()
            End If

            Throw New Exception(ex.Message.ToString())

        End Try

    End Sub

    ''' <summary>
    ''' Gera um array list de String contendo os diretórios e arquivos no caminho informado
    ''' </summary>
    ''' <param name="_ftpPath">Caminho do path no  servidor FTP (File Transfer Protocol) </param>
    ''' <returns> ArrayList  de String </returns>
    ''' <remarks></remarks>
    Public Function GetDirectory(ByVal _ftpPath As String) As List(Of String)

        Dim dir As New List(Of String)

        Try

            'Cria requisição ftp recebendo o caminho do diretório
            Dim _request As System.Net.FtpWebRequest = System.Net.WebRequest.Create(_ftpPath)

            _request.KeepAlive = False
            'método que será utilizado ao realizar a conexão
            _request.Method = System.Net.WebRequestMethods.Ftp.ListDirectoryDetails

            ''Seta as credenciais do Login
            _request.Credentials = getCredentials

            Dim _response As System.Net.FtpWebResponse = _request.GetResponse()

            ''Stream que irá pegar o retorno
            Dim responseStream As System.IO.Stream = _response.GetResponseStream()

            Dim _reader As System.IO.StreamReader = New System.IO.StreamReader(responseStream)

            Dim FileData As String = _reader.ReadToEnd

            Dim Lines() As String = FileData.Split(New String() {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries)

            For Each l As String In Lines
                'Trata a string para retornar apenas o nome do arquivo
                dir.Add(Trim(Mid(l, InStrRev(l, " "))))
            Next

            _reader.Close()
            _response.Close()

        Catch ex As Exception

            MessageBox.Show(ex.Message, "Directory Fetch Error: ", MessageBoxButtons.OK, MessageBoxIcon.Error)

        End Try

        Return dir

    End Function

    ''' <summary>
    ''' Verifica a existencia de um caminho ftp informado.
    ''' </summary>
    ''' <param name="_ftpPath"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function DiretorioExiste(ByVal _ftpPath As String) As Boolean

        If (_ftpPath.Substring(0, 6) <> "ftp://") Then
            _ftpPath = "ftp://" + _ftpPath
        End If

        'Cria requisição ftp recebendo o caminho do diretório
        Dim _request As System.Net.FtpWebRequest = System.Net.WebRequest.Create(_ftpPath)

        _request.KeepAlive = False
        'método que será utilizado ao realizar a conexão
        _request.Method = System.Net.WebRequestMethods.Ftp.ListDirectoryDetails

        ''Seta as credenciais do Login
        _request.Credentials = getCredentials

        Try

            ' Dim response As FtpWebResponse = DirectCast(_request.GetResponse(), FtpWebResponse)
            Using _response As System.Net.FtpWebResponse = _request.GetResponse()
                _response.Close()
                Return True
            End Using

        Catch ex As UriFormatException
            Console.Write(ex.TargetSite.ToString() + ex.InnerException.ToString() + ex.Source + ex.StackTrace + ex.Message)
            Return False
        Catch ex As WebException
            Return False
        Catch ex As Exception
            Console.Write(ex.TargetSite.ToString() + ex.InnerException.ToString() + ex.Source + ex.StackTrace + ex.Message)
            Return False
        End Try

    End Function

    Public Function trataUrlDirFtp(ByVal _CreatePath As String) As String

        Try

            If Not (_CreatePath.Contains(Me._Credentials.Domain)) Then

                If Not (_CreatePath.Substring(0, 1) = "/") Then
                    _CreatePath = "/" + _CreatePath
                End If

                If (Me.DirWeb <> "" Or Me.DirWeb <> Nothing) Then
                    If Not (_CreatePath.Contains(Me.DirWeb)) Then
                        _CreatePath = Me.DirWeb + _CreatePath.Substring(1)
                    End If
                End If

                _CreatePath = Me._Credentials.Domain + _CreatePath

            End If

            If (_CreatePath.Substring(_CreatePath.Length - 1) = "/" Or _CreatePath.Substring(_CreatePath.Length - 1) = "\") Then
                _CreatePath = _CreatePath.Substring(0, _CreatePath.Length - 1)
            End If

            If Not (_CreatePath.StartsWith("ftp://")) Then
                _CreatePath = "ftp://" + _CreatePath
            End If

            trataUrlDirFtp = _CreatePath

        Catch ex As Exception
            Throw New Exception("Problemas ao definir string Ulr" + vbCrLf + "MÉTODO : trataUrlDirFtp ")
        End Try

    End Function

    ''' <summary>
    ''' Verifica se o arquivo existe no diretório
    ''' </summary>
    ''' <param name="pathToSearch"></param>
    ''' <param name="fileSearch"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function ifFileExists(ByVal pathToSearch As String, ByVal fileSearch As String) As Boolean

        Try

            If (DiretorioExiste(pathToSearch)) Then

                Dim list As String() = GetDirectory(pathToSearch).ToArray

                For Each l As String In list
                    If (l.Equals(fileSearch)) Then
                        Return True
                    End If
                Next
                Return False
            Else
                Throw New Exception("O caminho para o arquivo solicitado não pode ser encontrado!")
            End If

        Catch ex As Exception
            Throw ex
        End Try

    End Function

    ''' <summary>
    ''' Recupera o nome e a extensão do arquivo passado no Path do diretório
    ''' </summary>
    ''' <param name="filePath">Caminho do diretório do arquivo</param>
    ''' <returns>Nome.extensão do arquivo </returns>
    ''' <remarks></remarks>
    Public Function recuperaNomeArquivoInFullPath(filePath As String) As String

        Try

            If (InStr(StrReverse(filePath), "\") = 0 And InStr(StrReverse(filePath), "/") = 0) Then
                Throw New InvalidOperationException("String em formato inválido!")
            End If

            If filePath <> "" Then
                ''recupera apenas o nome e a extensão do arquivo que será Upado
                recuperaNomeArquivoInFullPath = StrReverse(Mid(StrReverse(filePath), 1, InStr(StrReverse(filePath), "\") - 1))
            Else
                recuperaNomeArquivoInFullPath = ""
            End If

        Catch ex As Exception
            recuperaNomeArquivoInFullPath = ""
            Throw ex
        End Try

    End Function

#End Region

#Region "Gets e Sets"

    Public Property dateFtp() As String
        Get
            Return _date
        End Get
        Set(value As String)
            _date = CStr(Date.Now.Day & "-" & Date.Now.Month & "-" & Date.Now.Year)
        End Set
    End Property

    Public Property hourFtp() As String
        Get
            Return _hour
        End Get
        Set(value As String)
            _hour = CStr(Date.Now.Hour & "-" & Date.Now.Minute & "-" & Date.Now.Second)
        End Set
    End Property

    Public Property dateAndHourFtp() As String
        Get
            Return _dateAndHour
        End Get
        Set(value As String)
            Me.hourFtp = Nothing
            Me.dateFtp = Nothing
            _dateAndHour = CStr(dateFtp & " " & hourFtp)
        End Set
    End Property

    Public Sub setPastaUpload(caminho As String)
        Me.UploadedPath = caminho
    End Sub

    Public Property UploadedPath() As String
        Get
            Return _UploadedPath
        End Get
        Private Set(UploadPath As String)
            _UploadedPath = UploadPath
        End Set
    End Property

    Public Property DeletedPath() As String
        Get
            Return _DeletedPath
        End Get
        Private Set(DeletedPath As String)
            _DeletedPath = DeletedPath
        End Set
    End Property

    Public Property DirWeb() As String
        Get
            Return _DirWeb
        End Get
        Private Set(DirWeb As String)
            _DirWeb = DirWeb
        End Set
    End Property

    Public Sub setPastaDownload(ByVal caminho As String)
        Me.DownloadedPath = caminho
    End Sub

    Public Property DownloadedPath() As String

        '++++=++=++=++=++=+==+=+=++=+=+=+=+=+=+=+=+=++=++++=+++++++++++++=++=++=+==+=++=+++++++=+=+++=+++=++===++==++==++==++=+++==+++
        'ESTE ´PROPRIEDADE PRECISA SER ATUALIZADA POIS HOUVERAM ALTERAÇÃOES EM 19-01-2016 ENTRE AS QUAIS ESTÁ AÇÃO NÃO FOI ATUALIZADA
        '++++=++=++=++=++=+==+=+=++=+=+=+=+=+=+=+=+=++=++++=+++++++++++++=++=++=+==+=++=+++++++=+=+++=+++=++===++==++==++==++=+++==+++
        Get
            Return _DownloadedPath
        End Get
        Set(diretorio As String)
            _DownloadedPath = diretorio
            ''caso o caminho informado ou o caminho da pasta titanium não exista ele é setado no diretótio do disco C
            If (My.Computer.FileSystem.DirectoryExists(diretorio)) Then
                _DownloadedPath = diretorio
            ElseIf (My.Computer.FileSystem.DirectoryExists("C:\Titanium\Arquivos")) Then
                _DownloadedPath = "C:\Titanium\Arquivos"
            Else
                _DownloadedPath = "C:\"
            End If
        End Set
    End Property

    Public ReadOnly Property SenhaFtp() As String
        Get
            Return Me._Credentials.Password.ToString()
        End Get
    End Property

    Public ReadOnly Property UsuarioFtp() As String
        Get
            Return Me._Credentials.UserName.ToString()
        End Get
    End Property

    Public ReadOnly Property DominioFtp() As String
        Get
            Return Me._Credentials.Domain.ToString()
        End Get
    End Property

    ''' <summary>
    ''' Define as credencias a serem utilizadas durante a conexão com o Ftp
    ''' </summary>
    ''' <param name="_ftpUser"> Usuário</param>
    ''' <param name="_ftpPass"> Senha</param>
    ''' <param name="_ftpDomain"> Dominio</param>
    ''' <remarks></remarks>
    Public Sub setCredentials(ByVal _ftpUser As String, ByVal _ftpPass As String, ByVal _ftpDomain As String)
        Me.DirWeb = ""
        trataStrDominio(_ftpDomain)
        _Credentials = New System.Net.NetworkCredential(_ftpUser, _ftpPass, _ftpDomain)
    End Sub

    ''' <summary>
    '''  Define as credencias a serem utilizadas durante a conexão com o Ftp
    ''' </summary>
    ''' <param name="_ftpUser"> Usuário</param>
    ''' <param name="_ftpPass"> Senha</param>
    ''' <param name="_ftpDomain"> Dominio</param>
    ''' <param name="_dirWeb">Diretório onde será salvo, deletado,renomeado um determinado arquivo</param> 
    Public Sub setCredentials(ByVal _ftpUser As String, ByVal _ftpPass As String, ByVal _ftpDomain As String, ByVal _dirWeb As String)
        Me.DirWeb = _dirWeb
        trataStrDominio(_ftpDomain)
        _Credentials = New System.Net.NetworkCredential(_ftpUser, _ftpPass, _ftpDomain)
    End Sub

    Public Sub trataStrDominio(ByRef _ftpDomain As String)

        If (_ftpDomain.StartsWith("http://")) Then
            _ftpDomain = _ftpDomain.Substring(7)
        End If

        If (_ftpDomain.StartsWith("ftp://")) Then
            _ftpDomain = _ftpDomain.Substring(6)
        End If

        If (_ftpDomain.Contains(".com/")) Then
            If (_ftpDomain.Substring(_ftpDomain.LastIndexOf(".com/") + 5).Length > 0) Then
                _ftpDomain = _ftpDomain.Substring(0, _ftpDomain.LastIndexOf(".com/") + 5)
            End If
        End If

        If (_ftpDomain.Contains(".br/")) Then
            If (_ftpDomain.Substring(_ftpDomain.LastIndexOf(".br/") + 4).Length > 0) Then
                _ftpDomain = _ftpDomain.Substring(0, _ftpDomain.LastIndexOf(".br/") + 4)
            End If
        End If

        If (_ftpDomain.EndsWith("/") Or _ftpDomain.EndsWith("\")) Then
            _ftpDomain = _ftpDomain.Substring(0, _ftpDomain.Length - 1)
        End If

    End Sub

    Public ReadOnly Property getCredentials() As System.Net.NetworkCredential
        Get
            Return _Credentials
        End Get
    End Property

#End Region

End Class


