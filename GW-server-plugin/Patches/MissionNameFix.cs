using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using NuclearOption.AddressableScripts;
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
        var newName = GetMissionName(new PublishedFileId_t(workshopID)) ?? originalName;
        __instance.keyValues["mi"] = newName;
        GwServerPlugin.Logger.LogDebug(newName);
    }

    private static string? GetMissionName(PublishedFileId_t workshopId)
    {
        SteamWorkshop.TryGetInstallFolder(workshopId, out var folder);
        var data = ModLoader.ReadMetaData<MissionGroup.MissionMetaData>(workshopId, folder!);
        return data?.FileName;
    }
    
    [SuppressMessage("Method Declaration", "Harmony003:Harmony non-ref patch parameters modified")]
    public static MissionKey TranslateWorkshopName(MissionKey key)
    {
        if (key.Group != MissionGroup.Workshop || key.WorkshopId == null) return key;
        return new MissionKey(key.Name, GetMissionName(key.WorkshopId.Value), key.WorkshopId, MissionGroup.Workshop);
    }
}
