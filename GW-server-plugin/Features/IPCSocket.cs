using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GW_server_plugin.Features;

/// <summary>
/// InterProcessCommunication TCP socket for the plugin
/// </summary>
public class IpcSocket
{
    private TcpClient client;
    private NetworkStream stream;
    private CancellationTokenSource cts;

    private string Host;
    private int Port;

    /// <summary>
    /// Bool for if the socket is connected
    /// </summary>
    public bool Connected => client?.Connected == true;
    /// <summary>
    /// Event listener when Json command is recieved via socket
    /// </summary>
    public event Action<string>? OnJson;
    
  /// <summary>
  /// Starts the IPC socket
  /// </summary>
  public void Start(string host, int port)
    {
        Host = host;
        Port = port;
        cts = new CancellationTokenSource();
        // _ = Task.Run(() => ConnectionLoop(cts.Token));
        _ = ConnectionLoop(cts.Token);
    }

    async Task ConnectionLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                client = new TcpClient();
                client.NoDelay = true;
                await client.ConnectAsync(Host, Port);
                stream = client.GetStream();

                GwServerPlugin.Logger?.LogDebug("[IPC] socket connected");

                await ReceiveLoop(token);
            }
            catch (SocketException)
            {
                GwServerPlugin.Logger?.LogWarning("[IPC] socket failed to connect or disconnected.");
                GwServerPlugin.Logger?.LogWarning($"[IPC] Retrying connect in {PluginConfig.IpcRetryDelayMs!.Value / 1000} seconds...");
            }
            catch (Exception ex)
            {
                GwServerPlugin.Logger?.LogDebug("[IPC] " + ex.Message);
            }

            Cleanup();
            await Task.Delay(PluginConfig.IpcRetryDelayMs!.Value, token);
        }
    }

    async Task ReceiveLoop(CancellationToken token)
    {
        var buffer = new byte[4096];
        var sb = new StringBuilder();

        while (!token.IsCancellationRequested)
        {
            int read = await stream.ReadAsync(buffer, 0, buffer.Length, token);
            if (read == 0)
                throw new Exception("Disconnected");

            sb.Append(Encoding.UTF8.GetString(buffer, 0, read));

            while (true)
            {
                int newline = sb.ToString().IndexOf('\n');
                if (newline == -1)
                    break;

                string line = sb.ToString(0, newline).Trim();
                sb.Remove(0, newline + 1);

                if (!string.IsNullOrEmpty(line))
                {
                    OnJson?.Invoke(line);
                }
            }
        }
    }

    /// <summary>
    /// Sends a Json string to the process.
    /// </summary>
    /// <param name="json"></param>
    public async Task SendJson(string json)
    {
        if (!Connected) return;

        byte[] data = Encoding.UTF8.GetBytes(json + "\n");
        try
        {
            await stream.WriteAsync(data, 0, data.Length);
        }
        catch
        {
            GwServerPlugin.Logger?.LogWarning($"[IPC] send json failed: {json}");
        }
    }

    void Cleanup()
    {
        stream?.Close();
        client?.Close();
        stream = null;
        client = null;
    }

    /// <summary>
    /// Deletes the stream and cleans it up
    /// </summary>
    public void Dispose()
    {
        cts?.Cancel();
        Cleanup();
    }    
}