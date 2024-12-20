using System.Collections.Generic;
using HarmonyLib;
using SFS;
using SFS.Parts.Modules;
using SFS.UI;
using Random = System.Random;

// ReSharper disable InconsistentNaming // Harmony double underscore

namespace RandomFailuresLegacy
{
    internal static class Data
    {
        // Key: EngineModule::GetInstanceID
        public static readonly Dictionary<int, FailureData> FailureTable = new();
    }

    internal class FailureData
    {
        private static readonly Random _random = new();

        private readonly bool _failedAtStart = _random.Next(0, Settings.RFSettings.XInitialStartFailureChance) == 0;
        private bool _failedInFlight;
        private int _failureTicks;

        public bool CanStart => !_failedAtStart && !_failedInFlight;
        
        public FailureData(bool isIonEngine = false)
        {
            _failureTicks = Settings.RFSettings.XSustainedFailureChance *
                            (isIonEngine ? Settings.RFSettings.XIonMultiplier : 1);
        }

        public bool EngineTick()
        {
            if (_failureTicks == 0) return true;
            
            _failureTicks--;
            return _random.Next(0, _failureTicks) == 0;
        }

        public void DisablePermanent()
        {
            _failedInFlight = true;
            _failureTicks = 0;
        }
    }

    [HarmonyPatch(typeof(EngineModule), "EnableEngine")]
    public class EnableEngineEngineModulePatch
    {
        [HarmonyPriority(Priority.First)]
        private static bool Prefix(EngineModule __instance, I_MsgLogger logger)
        {
            if (!Data.FailureTable.ContainsKey(__instance.GetInstanceID()))
            {
                Data.FailureTable.Add(__instance.GetInstanceID(), new FailureData());
            }
            
            FailureData data = Data.FailureTable[__instance.GetInstanceID()];
            
            if (!data.CanStart) MsgDrawer.main.Log("Engine failed to start!");
            return data.CanStart;
        }
    }

    [HarmonyPatch(typeof(EngineModule), "FixedUpdate")]
    public class FixedUpdateEngineModulePatch
    {
        [HarmonyPriority(Priority.First)]
        private static void Prefix(EngineModule __instance)
        {
            if (!__instance.engineOn.Value) return;
            
            if (!Data.FailureTable.ContainsKey(__instance.GetInstanceID()))
            {
                Data.FailureTable.Add(__instance.GetInstanceID(), new FailureData());
            }
            
            FailureData data = Data.FailureTable[__instance.GetInstanceID()];

            if (!data.CanStart) return;
            if (!data.EngineTick()) return;
            
            MsgDrawer.main.Log("An engine failed mid-flight!");
            data.DisablePermanent();
            __instance.ToggleEngine();
        }
    }
}