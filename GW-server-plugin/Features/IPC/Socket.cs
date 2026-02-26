using System;
using System.Drawing.Printing;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GW_server_plugin.Features.IPC;

/// <summary>
/// Inter‑Process Communication TCP socket for the plugin (server side).
/// </summary>
public class Socket : IDisposable
{
    /* ------------------------------------------------------------------ */
    /*  Fields & Properties                                              */
    /* ------------------------------------------------------------------ */

    private TcpListener? _listener;     // listens for incoming connections
    private TcpClient?   _client;        // the currently connected client
    private NetworkStream? _stream;
    private CancellationTokenSource? _cts;

    private bool Connected => _client?.Connected == true;

    /// <summary>Raised when a complete JSON line has been received.</summary>
    public event Action<string>? OnJson;

    /* ------------------------------------------------------------------ */
    /*  Public API                                                        */
    /* ------------------------------------------------------------------ */

    /// <summary>
    /// Start listening on *host*:*port*. The call is non‑blocking.
    /// </summary>
    public void Start(string host, int port)
    {
        try
        {
            // Create a TCP listener and start it
            _cts = new CancellationTokenSource();
            var ip = IPAddress.Parse(host);
            _listener = new TcpListener(ip, port);
            _listener.Start();

            // Kick off the accept loop in the background
            _ = AcceptLoop(_cts.Token);
        }
        catch (Exception ex)
        {
            GwServerPlugin.Logger.LogError($"[IPC]: failed to open socket: {ex.Message}");
        }
    }

    /// <summary>
    /// Send a JSON string to the connected client (if any).
    /// </summary>
    public async Task SendJson(string json)
    {
        if (!Connected) return;

        var data = Encoding.UTF8.GetBytes(json + "\n");
        try
        {
            await _stream!.WriteAsync(data, 0, data.Length);
        }
        catch (Exception ex)
        {
            GwServerPlugin.Logger.LogWarning(
                $"[IPC] send json failed: {json} ({ex.Message})");
        }
    }

    /// <summary>
    /// Stop listening and clean up all sockets.
    /// </summary>
    public void Dispose()
    {
        _cts?.Cancel();
        _listener?.Stop();          // stops AcceptTcpClientAsync
        Cleanup();
    }

    /* ------------------------------------------------------------------ */
    /*  Internal helpers                                                 */
    /* ------------------------------------------------------------------ */

    private async Task AcceptLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            TcpClient? newClient = null;
            try
            {
                // Wait for a client to connect
                newClient = await _listener!.AcceptTcpClientAsync();
                GwServerPlugin.Logger.LogDebug("[IPC] client connected");

                // Close any previous connection (if we only support 1)
                Cleanup();

                // Store the new connection and start receiving data
                _client = newClient;
                _stream = _client.GetStream();
                await ReceiveLoop(token);      // will return when disconnected
                GwServerPlugin.Logger.LogDebug("[IPC] client disconnected");
            }
            catch (OperationCanceledException) { /* graceful exit */ }
            catch (SocketException ex)
            {
                GwServerPlugin.Logger.LogWarning($"[IPC] accept failed: {ex.Message}");
            }
            catch (Exception ex)
            {
                GwServerPlugin.Logger.LogError("[IPC]  " + ex);
            }

        }
    }

    private async Task ReceiveLoop(CancellationToken token)
    {
        var buffer = new byte[4096];
        var sb = new StringBuilder();

        while (!token.IsCancellationRequested && Connected)
        {
            int read;
            try
            {
                read = await _stream!.ReadAsync(buffer, 0, buffer.Length, token);
            }
            catch (Exception) { break; }          // network error / cancel

            if (read == 0)                         // remote closed the socket
                break;

            sb.Append(Encoding.UTF8.GetString(buffer, 0, read));

            while (true)
            {
                var newline = sb.ToString().IndexOf('\n');
                if (newline == -1)
                    break;                       // no full line yet

                var line = sb.ToString(0, newline).Trim();
                sb.Remove(0, newline + 1);

                if (!string.IsNullOrEmpty(line))
                    OnJson?.Invoke(line);
            }
        }

        // The client has disconnected – clean up and return to the accept loop
        Cleanup();
    }

    private void Cleanup()
    {
        _stream?.Close();
        _client?.Close();
        _stream = null;
        _client = null;
    }
}