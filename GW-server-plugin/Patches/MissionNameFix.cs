using System.Diagnostics.CodeAnalysis;
using Cysharp.Threading.Tasks;
using HarmonyLib;
using NuclearOption.AddressableScripts;
using NuclearOption.DedicatedServer;
using NuclearOption.Networking.Lobbies;
using NuclearOption.SavedMission;
using NuclearOption.Workshop;
using Steamworks;

namespace GW_server_plugin.Patches;

[HarmonyPatch]
internal class MissionNameFix
{
    [HarmonyPatch(typeof(DedicatedServerKeyValues), nameof(DedicatedServerKeyValues.ApplyValuesToSteam))]
    [HarmonyPrefix]
    public static void TransformMissionName(DedicatedServerKeyValues __instance)
    {
        if (!__instance.keyValues.TryGetValue("mi", out var originalName)) return;
        if (!ulong.TryParse(originalName, out var workshopID)) return;
        GetMissionName(workshopID, out var newName);
        newName ??= originalName;
        __instance.keyValues["mi"] = newName;
        GwServerPlugin.Logger.LogDebug(newName);
    }

    internal static bool GetMissionName(ulong workshopId, out string? name) => GetMissionName(new PublishedFileId_t(workshopId), out name);
    
    private static bool GetMissionName(PublishedFileId_t workshopId, out string? name)
    {
        name = null;
        if (!SteamWorkshop.TryGetInstallFolder(workshopId, out var folder))
        {
            GwServerPlugin.Logger.LogDebug($"returning: {folder}");
            return false;
        }        
        GwServerPlugin.Logger.LogDebug($"1: {folder}");
        var data = ModLoader.ReadMetaData<MissionGroup.MissionMetaData>(workshopId, folder!);
        name = data?.FileName;
        return name != null;
    }

    [SuppressMessage("Method Declaration", "Harmony003:Harmony non-ref patch parameters modified")]
    internal static async UniTask<(bool success, string? name)> GetMissionNameAsync(ulong workshopId)
    {
        if (!await DedicatedServerManager.EnsureWorkshopItemDownloaded(
                new MissionKeySaveable { Group = "Workshop", Name = workshopId.ToString() }))
            return (false, null);

        var workshopIDTyped = new PublishedFileId_t(workshopId);
        
        if (!SteamWorkshop.TryGetInstallFolder(workshopIDTyped, out var folder))
        {
            return (false, null);
        }        
        GwServerPlugin.Logger.LogDebug($"1: {folder}");
        var data = ModLoader.ReadMetaData<MissionGroup.MissionMetaData>(workshopIDTyped, folder!);
        var name = data?.FileName;
        return (name != null, name);
    }
    
    [SuppressMessage("Method Declaration", "Harmony003:Harmony non-ref patch parameters modified")]
    public static MissionKey TranslateWorkshopName(MissionKey key)
    {
        if (key.Group != MissionGroup.Workshop || key.WorkshopId == null ||
            !GetMissionName(key.WorkshopId.Value, out var name)) return key;
        return new MissionKey(key.Name, name, key.WorkshopId, MissionGroup.Workshop);
    }
}
