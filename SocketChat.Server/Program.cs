using System.Net;
using System.Net.Sockets;
using System.Text;

const int port = 5000;
var listener = new TcpListener(IPAddress.Loopback, port);

listener.Start();
Console.WriteLine($"Server listening on 127.0.0.1:{port}");
Console.WriteLine("Waiting for a client... (Press Ctrl+C to stop)");

try
{
    while (true)
    {
        using TcpClient client = await listener.AcceptTcpClientAsync();
        Console.WriteLine("Client connected.");

        await using NetworkStream stream = client.GetStream();
        using var reader = new StreamReader(stream, Encoding.UTF8);
        await using var writer = new StreamWriter(stream, new UTF8Encoding(false))
        {
            AutoFlush = true
        };

        string? message;
        while ((message = await reader.ReadLineAsync()) is not null)
        {
            Console.WriteLine($"Client says: {message}");
            await writer.WriteLineAsync($"Server received: {message}");
        }

        Console.WriteLine("Client disconnected. Waiting for another client...");
    }
}
catch (SocketException ex)
{
    Console.WriteLine($"Socket error: {ex.Message}");
}
finally
{
    listener.Stop();
}
