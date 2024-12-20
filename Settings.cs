using System;
using SFS.IO;
using SFS.UI.ModGUI;
using SFS.Variables;
using TMPro;
using UITools;
using UnityEngine;
using Type = SFS.UI.ModGUI.Type;

namespace RandomFailuresLegacy
{
    public class Settings : ModSettings<Settings.RFSettings>
    {
        private static Settings? _instance;

        public static void Load()
        {
            _instance = new Settings();
            _instance.Initialize();
        }

        protected override void RegisterOnVariableChange(Action onChange)
        {
            settings.InitialStartFailureChance.OnChange += onChange;
            settings.SustainedFailureChance.OnChange += onChange;
            settings.IonMultiplier.OnChange += onChange;
            
            Application.quitting += onChange;
        }

        protected override FilePath SettingsFile => new($"{RFLegacyMod.Instance.ModFolder}/rfl_config.txt");

        public class RFSettings
        {
            public static int XInitialStartFailureChance => settings.InitialStartFailureChance.Value;
            public static int XSustainedFailureChance => settings.SustainedFailureChance.Value;
            public static int XIonMultiplier => settings.IonMultiplier.Value;

            public Int_Local InitialStartFailureChance = new() { Value = 100 }; // 1% fails at initial start
            public Int_Local SustainedFailureChance = new() { Value = 216000 }; // Fails after 1 hour of firing
            public Int_Local IonMultiplier = new() { Value = 8760 }; // Fails after 1 year of firing (given that a normal engine fails after an hour)
        }
    }

    public static class ConfigurationMenuTab
    {
        public static void Load()
        {
            ConfigurationMenu.Add(null, new (string, Func<Transform, GameObject>)[] { ("RFLegacy", CreateTab) });
        }

        private static GameObject CreateTab(Transform transform)
        {
            Vector2Int size = ConfigurationMenu.ContentSize;
            Box box = Builder.CreateBox(transform, size.x, size.y);
            
            box.CreateLayoutGroup(Type.Vertical, TextAnchor.UpperCenter, padding: new RectOffset(5, 5, 5, 5));

            CreateNumberInput(box, size.x - 50, 50, "Initial Start Failure Chance (1 in x)", Settings.settings.InitialStartFailureChance, 1, value => Settings.settings.InitialStartFailureChance.Value = value);
            CreateNumberInput(box, size.x - 50, 50, "Sustained Engine Failure Time (ticks)", Settings.settings.SustainedFailureChance, 1, value => Settings.settings.SustainedFailureChance.Value = value);
            CreateNumberInput(box, size.x - 50, 50, "Ion Multiplier (Sustained Engine Failure Time x this value)", Settings.settings.IonMultiplier, 1, value => Settings.settings.IonMultiplier.Value = value);

            return box.gameObject;
        }

        private static void CreateNumberInput(Transform transform, int width, int height, string label, int value,
            int step, Action<int> onChange)
        {
            Container container = Builder.CreateContainer(transform);
            container.CreateLayoutGroup(Type.Horizontal);

            Label title = Builder.CreateLabel(container, (int)(width * 0.55f), height, text: label);
            title.TextAlignment = TextAlignmentOptions.MidlineLeft;

            UIToolsBuilder.CreateNumberInput(container, (int)(width * 0.5f), height, value, step).OnValueChangedEvent += it => onChange((int) it);
        }
    }
}