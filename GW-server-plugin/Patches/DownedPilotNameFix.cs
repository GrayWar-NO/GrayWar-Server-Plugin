using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using GW_server_plugin.Helpers;
using NuclearOption.Networking;

namespace GW_server_plugin.Patches;

/// <summary>
///     Fixes downed pilot names being set to "player" in the map
/// </summary>
[HarmonyPatch(typeof(Aircraft))]
public class DownedPilotNameFix
{
    
    
    private static PilotDismounted SetupEjectingPilotName(PilotDismounted original, Player? owner)
    {
        if (owner != null) original.NetworkunitName = "[" + owner.GetDisplayName() + "] " + original.unitName;
        return original;
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="instructions"></param>
    /// <returns></returns>
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(Aircraft), nameof(Aircraft.SpawnEjectingPilot))]
    public static IEnumerable<CodeInstruction> PilotDismountedConstructorPostfix(IEnumerable<CodeInstruction> instructions)
    {
        
        var matcher = new CodeMatcher(instructions);
        var aircraftPlayerMethod = AccessTools.PropertyGetter(typeof(Aircraft), nameof(Aircraft.Player));
        var translatorMethod = AccessTools.Method(typeof(DownedPilotNameFix), nameof(SetupEjectingPilotName));
        
        matcher.MatchForward(false,
            new CodeMatch(new CodeInstruction(OpCodes.Ldarg_0)),
            new CodeMatch(instr => instr.Calls(aircraftPlayerMethod))
        );
        
        if (matcher.IsValid)
        {
            var instr0 = matcher.Instruction;
            matcher.Advance(1);
            var instr1 = matcher.Instruction;
            matcher.Advance(1);
            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Call, translatorMethod));
            matcher.InsertAndAdvance(instr0);
            matcher.InsertAndAdvance(instr1);
        }
        
        return matcher.InstructionEnumeration();
    }
}