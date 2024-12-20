using System.Collections.Generic;
using ModLoader;
using HarmonyLib;
using ModLoader.Helpers;

namespace RandomFailuresLegacy
{
    public class RFLegacyMod : Mod
    {
        public const string RfModId = "random_failures_legacy";
        public const string RfModName = "Random Failures Legacy";
        public const string RfAuthor = "TheAlgorithm476";
        
        private static Harmony? _patcher;

        public static RFLegacyMod Instance { get; private set; } = null!;
        
        public override string ModNameID => RfModId;
        public override string DisplayName => RfModName;
        public override string Author => RfAuthor;
        public override string MinimumGameVersionNecessary => "1.5.10.2";
        public override string ModVersion => "1.0.0";
        public override Dictionary<string, string> Dependencies { get; } = new() { { "UITools", "1.1.5" } };
        public override string Description =>
            "Adds a random small chance of an engine failing to ignite, or having an in-flight failure.";

        public RFLegacyMod() => Instance = this;

        public override void Early_Load()
        {
            _patcher = new Harmony($"me.thealgorithm476.{RfModId}");
            _patcher.PatchAll();
        }

        public override void Load()
        {
            Settings.Load();
            ConfigurationMenuTab.Load();

            SceneHelper.OnHubSceneLoaded += () =>
            {
                Data.FailureTable.Clear();
            };
        }
    }
}