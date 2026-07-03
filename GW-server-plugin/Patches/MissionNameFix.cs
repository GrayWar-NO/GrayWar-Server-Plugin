using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.Emit;
using HarmonyLib;
using NuclearOption.AddressableScripts;
using NuclearOption.DedicatedServer;
using NuclearOption.SavedMission;
using NuclearOption.Workshop;
using Steamworks;

namespace GW_server_plugin.Patches;

[HarmonyPatch]
internal class MissionNameFix
{
    [HarmonyPatch(
        typeof(MissionGroup.WorkshopGroup),
        nameof(MissionGroup.WorkshopGroup.TryGetJson),
        [typeof(MissionKey), typeof(string)],
        [ArgumentType.Normal, ArgumentType.Out]
    )]
    [HarmonyPostfix]
    public static void GetJsonPostfix(
        MissionGroup.WorkshopGroup __instance,
        ref MissionKey key,
        ref string json,
        ref bool __result)
    {
        if (!__result) return;
        GwServerPlugin.Logger.LogDebug(GetMissionName(key.WorkshopId!.Value));
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
    
    [HarmonyPatch(typeof(DedicatedServerManager), nameof(DedicatedServerManager.PreLoadMission))]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> PreLoadMissionTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        var matcher = new CodeMatcher(instructions)
            .MatchForward(
                false,
                new CodeMatch(OpCodes.Ldloc_0),
                new CodeMatch(OpCodes.Ldarg_1)
            );

        if (matcher.IsValid)
        {
            GwServerPlugin.Logger.LogDebug("Found preloadmission transpile target.");
            matcher.Advance(1);
            matcher.Insert(new CodeInstruction(OpCodes.Call, AccessTools.Method(
                typeof(MissionNameFix), nameof(TranslateWorkshopName))));
        }

        return matcher.InstructionEnumeration();
    }
    
}