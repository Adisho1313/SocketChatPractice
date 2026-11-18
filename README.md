# Socket Chat Practice (C# / TCP / Windows Forms)

This starter project contains:
- `SocketChat.Server`: a console TCP server.
- `SocketChat.Client`: a Windows Forms desktop client.

Both applications communicate over TCP on your own computer (`127.0.0.1`), using port `5000`.

## Requirements

- Windows 10/11
- .NET 8 SDK
- Visual Studio Code
- VS Code C# extension (C# Dev Kit is recommended)

Windows Forms is Windows-only. Run the client on Windows.

## Open in VS Code

1. Extract the ZIP file.
2. In VS Code, choose **File > Open Folder** and select the extracted `SocketChatPractice` folder.
3. Open a terminal in VS Code.

## Run the server

In terminal 1:

```powershell
dotnet run --project .\SocketChat.Server\SocketChat.Server.csproj
```

Wait for: `Server listening on 127.0.0.1:5000`

## Run the Windows Forms client

Keep the server running. Open terminal 2 in VS Code:

```powershell
dotnet run --project .\SocketChat.Client\SocketChat.Client.csproj
```

A desktop window opens. Click **Connect**, type a message, and click **Send**.
The server replies with `Server received: ...`.

## How to stop

- Close the client window.
- Press `Ctrl+C` in the server terminal.

## Important learning notes

- `TcpListener` listens for incoming TCP connections.
- `TcpClient` connects to the server.
- `NetworkStream` carries bytes between the two applications.
- `StreamReader` and `StreamWriter` handle text.
- Each message ends with a newline (`WriteLineAsync` / `ReadLineAsync`) so the receiver knows where a message ends.

This is a learning demo, not production-ready networking. It has no authentication or encryption. Use it only on your own computer or a trusted test network.
