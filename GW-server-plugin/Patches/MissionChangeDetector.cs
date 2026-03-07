using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using GW_server_plugin.Enums;
using GW_server_plugin.Features.IPC.Packets;
using HarmonyLib;
using Newtonsoft.Json;
using NuclearOption.DedicatedServer;
using NuclearOption.SavedMission;

namespace GW_server_plugin.Patches;

/// <summary>
/// Detects changes to the mission state
/// </summary>
[HarmonyPatch]
public class MissionChangeDetector
{
    // Target the MoveNext method of the async state machine
    // ReSharper disable once UnusedMember.Local
    private static IEnumerable<MethodBase> TargetMethods()
    {
        var asyncMethod = typeof(DedicatedServerManager).GetMethod(nameof(DedicatedServerManager.RunnerInner), BindingFlags.NonPublic | BindingFlags.Instance);
        if (asyncMethod == null) yield break;

        var stateMachineAttr = asyncMethod.GetCustomAttribute<AsyncStateMachineAttribute>();
        if (stateMachineAttr == null) yield break;

        var stateMachineType = stateMachineAttr.StateMachineType;
        var moveNextMethod = stateMachineType.GetMethod("MoveNext", BindingFlags.NonPublic | BindingFlags.Instance);

        if (moveNextMethod != null)
            yield return moveNextMethod;
    }

    [HarmonyTranspiler]
    // ReSharper disable once UnusedMember.Local
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    { // TODO change: this is broken
        var found = false;
        foreach (var ci in instructions)
        {
            // Look for stfld (field assignment)
            if (ci.opcode == OpCodes.Stfld && ci.operand is FieldInfo { Name: nameof(DedicatedServerManager.currentMission) } field)
            {
                yield return ci;

                yield return new CodeInstruction(OpCodes.Ldarg_0); // 'this' of state machine
                yield return new CodeInstruction(OpCodes.Ldfld, field); // Load the assigned value
                yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(MissionChangeDetector), nameof(OnMissionChanged))); // Call the mission change handler
                found = true;
            }
            else
            {
                yield return ci;
            }
        }

        if (!found)
            System.Console.WriteLine("Warning: Assignment to currentMission not found in IL.");
    }

    /// <summary>
    /// Behaviour to run whenever a mission changes.
    /// </summary>
    /// <param name="mission"></param>
    public static void OnMissionChanged(Mission? mission)
    {
        GwServerPlugin.Logger.LogDebug($"Mission changed: {mission?.Name ?? "null"}");
        var missionChangePacket = new LogEntryPacket
        {
            Channel = LogChannel.MissionStatus,
            LogText = mission?.Name ?? "null"
        };
        
        GwServerPlugin.SocketOutBox.Add(JsonConvert.SerializeObject(missionChangePacket));
        GwServerPlugin.WarnService.ClearWarns();
    }
}   

[HarmonyPatch(typeof(DedicatedServerManager), nameof(DedicatedServerManager.PreloadAndUpdateLobby))]
internal class LobbyPatch
{
    [HarmonyPostfix]
    static void Postfix(MissionOptions option, bool setStartTime, ref Mission mission)
    {
        MissionChangeDetector.OnMissionChanged(mission);
    }
}