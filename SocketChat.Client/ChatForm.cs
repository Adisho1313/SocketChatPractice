using System.Net.Sockets;
using System.Text;

namespace SocketChat.Client;

public sealed class ChatForm : Form
{
    private readonly TextBox _messages = new()
    {
        Multiline = true,
        ReadOnly = true,
        ScrollBars = ScrollBars.Vertical,
        Dock = DockStyle.Fill
    };

    private readonly TextBox _messageInput = new()
    {
        Dock = DockStyle.Fill
    };

    private readonly Button _connectButton = new()
    {
        Text = "Connect",
        AutoSize = true
    };

    private readonly Button _sendButton = new()
    {
        Text = "Send",
        AutoSize = true,
        Enabled = false
    };

    private TcpClient? _client;
    private StreamReader? _reader;
    private StreamWriter? _writer;
    private CancellationTokenSource? _receiveCts;

    public ChatForm()
    {
        Text = "TCP Socket Chat Client";
        Width = 650;
        Height = 430;
        StartPosition = FormStartPosition.CenterScreen;

        var bottomPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 42,
            ColumnCount = 3,
            Padding = new Padding(6)
        };
        bottomPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        bottomPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        bottomPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        bottomPanel.Controls.Add(_messageInput, 0, 0);
        bottomPanel.Controls.Add(_sendButton, 1, 0);
        bottomPanel.Controls.Add(_connectButton, 2, 0);

        Controls.Add(_messages);
        Controls.Add(bottomPanel);

        _connectButton.Click += async (_, _) => await ConnectAsync();
        _sendButton.Click += async (_, _) => await SendAsync();
        _messageInput.KeyDown += async (_, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                await SendAsync();
            }
        };

        FormClosing += (_, _) => Disconnect();
    }

    private async Task ConnectAsync()
    {
        if (_client is not null)
        {
            Disconnect();
            return;
        }

        try
        {
            _client = new TcpClient();
            await _client.ConnectAsync("127.0.0.1", 5000);

            NetworkStream stream = _client.GetStream();
            _reader = new StreamReader(stream, Encoding.UTF8);
            _writer = new StreamWriter(stream, new UTF8Encoding(false))
            {
                AutoFlush = true
            };

            _receiveCts = new CancellationTokenSource();
            _connectButton.Text = "Disconnect";
            _sendButton.Enabled = true;
            AddMessage("Connected to server.");

            _ = ReceiveLoopAsync(_receiveCts.Token);
        }
        catch (Exception ex)
        {
            AddMessage($"Connection failed: {ex.Message}");
            Disconnect();
        }
    }

    private async Task SendAsync()
    {
        string message = _messageInput.Text.Trim();

        if (string.IsNullOrWhiteSpace(message) || _writer is null)
            return;

        try
        {
            await _writer.WriteLineAsync(message);
            AddMessage($"You: {message}");
            _messageInput.Clear();
        }
        catch (Exception ex)
        {
            AddMessage($"Send failed: {ex.Message}");
            Disconnect();
        }
    }

    private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested && _reader is not null)
            {
                string? response = await _reader.ReadLineAsync(cancellationToken);
                if (response is null)
                    break;

                AddMessage(response);
            }

            if (!cancellationToken.IsCancellationRequested)
                AddMessage("Server disconnected.");
        }
        catch (OperationCanceledException)
        {
            // Expected when the user disconnects or closes the window.
        }
        catch (Exception ex)
        {
            if (!cancellationToken.IsCancellationRequested)
                AddMessage($"Receive failed: {ex.Message}");
        }
        finally
        {
            if (!IsDisposed && !cancellationToken.IsCancellationRequested)
                BeginInvoke(Disconnect);
        }
    }

    private void AddMessage(string message)
    {
        if (IsDisposed)
            return;

        if (InvokeRequired)
        {
            BeginInvoke(() => AddMessage(message));
            return;
        }

        _messages.AppendText(message + Environment.NewLine);
    }

    private void Disconnect()
    {
        _receiveCts?.Cancel();
        _receiveCts?.Dispose();
        _receiveCts = null;

        _reader?.Dispose();
        _writer?.Dispose();
        _client?.Close();

        _reader = null;
        _writer = null;
        _client = null;

        if (!IsDisposed)
        {
            _connectButton.Text = "Connect";
            _sendButton.Enabled = false;
        }
    }
}
