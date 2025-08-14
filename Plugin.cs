using BaboonAPI.Hooks.Initializer;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.IO;
using TootTallyCore.Utils.TootTallyModules;
using TootTallySettings;
using UnityEngine;

namespace TootTallyReplayViewerBot
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency("TootTallyCore", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("TootTallySettings", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("TootTallyLeaderboard", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("TootTallyAutoToot", BepInDependency.DependencyFlags.HardDependency)]
    public class Plugin : BaseUnityPlugin, ITootTallyModule
    {
        public static Plugin Instance;

        private const string CONFIG_NAME = "TootTallyReplayViewerBot.cfg";
        private Harmony _harmony;
        public ConfigEntry<bool> ModuleConfigEnabled { get; set; }
        public bool IsConfigInitialized { get; set; }

        //Change this name to whatever you want
        public string Name { get => PluginInfo.PLUGIN_NAME; set => Name = value; }

        public static TootTallySettingPage settingPage;

        public static void LogInfo(string msg) => Instance.Logger.LogInfo(msg);
        public static void LogError(string msg) => Instance.Logger.LogError(msg);

        private void Awake()
        {
            if (Instance != null) return;
            Instance = this;
            _harmony = new Harmony(Info.Metadata.GUID);

            GameInitializationEvent.Register(Info, TryInitialize);
        }

        private void TryInitialize()
        {
            // Bind to the TTModules Config for TootTally
            ModuleConfigEnabled = TootTallyCore.Plugin.Instance.Config.Bind("Modules", "TootTallyReplayViewerBot", true, "Module that automatically watches replays.");
            TootTallyModuleManager.AddModule(this);
            TootTallySettings.Plugin.Instance.AddModuleToSettingPage(this);
        }

        public void LoadModule()
        {
            string configPath = Path.Combine(Paths.BepInExRootPath, "config/");
            ConfigFile config = new ConfigFile(configPath + CONFIG_NAME, true) { SaveOnConfigSet = true };
            ToggleKey = config.Bind("General", nameof(ToggleKey), KeyCode.F10, "Enable / Disable AutoToot.");
            DebugMode = config.Bind("General", nameof(DebugMode), false, "Show additional logs.");
            UseAutoToot = config.Bind("General", nameof(UseAutoToot), false, "Use AutoToot instead of replays.");

            settingPage = TootTallySettingsManager.AddNewPage("TootTallyReplayViewerBot", "TootTally Replay Viewer Bot", 40f, new Color(0,0,0,0));
            settingPage.AddLabel("Toggle Key");
            settingPage.AddDropdown("Toggle Key", ToggleKey);
            settingPage.AddToggle("Debug Mode", DebugMode);
            settingPage.AddToggle("UseAutoToot", UseAutoToot);

            _harmony.PatchAll(typeof(ReplayBotManager));
            LogInfo($"Module loaded!");
        }

        public void UnloadModule()
        {
            _harmony.UnpatchSelf();
            settingPage.Remove();
            LogInfo($"Module unloaded!");
        }

        public ConfigEntry<KeyCode> ToggleKey { get; set; }
        public ConfigEntry<bool> DebugMode { get; set; }
        public ConfigEntry<bool> UseAutoToot { get; set; }

    }
}