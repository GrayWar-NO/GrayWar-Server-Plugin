using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Threading;
using System.Threading.Tasks;

namespace GW_server_plugin.Helpers;

/// <summary>
/// Looks up public Steam persona names through the Steam Web API.
/// </summary>
public static class SteamWebApi
{
    private static readonly HttpClient HttpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(10)
    };

    /// <summary>
    /// Gets the public Steam persona name for a SteamID.
    /// </summary>
    /// <param name="steamId">The 64-bit SteamID to look up.</param>
    /// <param name="cancellationToken">Token used to cancel the web request.</param>
    /// <returns>The persona name, or <see langword="null"/> when the key is unavailable or Steam has no result.</returns>
    public static async Task<string?> GetUsernameAsync(ulong steamId, CancellationToken cancellationToken = default)
    {
        var apiKey = PluginConfig.SteamWebApiKey?.Value;
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            GwServerPlugin.Logger.LogWarning("Steam Web API lookup skipped because no API key is configured.");
            return null;
        }

        var requestUri = new Uri(
            "https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key=" +
            Uri.EscapeDataString(apiKey!) +
            "&steamids=" + steamId);

        try
        {
            using var response = await HttpClient.GetAsync(requestUri, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                GwServerPlugin.Logger.LogWarning(
                    $"Steam Web API player lookup for {steamId} failed with HTTP {(int)response.StatusCode}.");
                return null;
            }

            using var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            var serializer = new DataContractJsonSerializer(typeof(PlayerSummariesResponse));
            var result = serializer.ReadObject(responseStream) as PlayerSummariesResponse;

            var username = result?.Response?.Players?
                .FirstOrDefault(player => player.SteamId == steamId.ToString())?
                .PersonaName;

            if (string.IsNullOrWhiteSpace(username))
                GwServerPlugin.Logger.LogWarning($"Steam Web API returned no persona name for {steamId}.");

            return username;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return null;
        }
        catch (Exception exception)
        {
            GwServerPlugin.Logger.LogWarning($"Steam Web API player lookup for {steamId} failed: {exception.Message}");
            return null;
        }
    }

    [DataContract]
    private sealed class PlayerSummariesResponse
    {
        [DataMember(Name = "response")]
        public PlayerSummariesPayload? Response { get; set; }
    }

    [DataContract]
    private sealed class PlayerSummariesPayload
    {
        [DataMember(Name = "players")]
        public List<PlayerSummary>? Players { get; set; }
    }

    [DataContract]
    private sealed class PlayerSummary
    {
        [DataMember(Name = "steamid")]
        public string? SteamId { get; set; }

        [DataMember(Name = "personaname")]
        public string? PersonaName { get; set; }
    }
}
