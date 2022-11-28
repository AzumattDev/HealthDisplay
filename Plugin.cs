using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using ServerSync;
using UnityEngine;

namespace HealthDisplay
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class HealthDisplayPlugin : BaseUnityPlugin
    {
        internal const string ModName = "HealthDisplay";
        internal const string ModVersion = "1.0.3";
        internal const string Author = "Azumatt";
        private const string ModGUID = Author + "." + ModName;
        private static string ConfigFileName = ModGUID + ".cfg";
        private static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;
        internal static bool GroupsIsInstalled;
        internal static string ConnectionError = "";

        private readonly Harmony _harmony = new(ModGUID);

        public static readonly ManualLogSource HealthDisplayLogger =
            BepInEx.Logging.Logger.CreateLogSource(ModName);

        private static readonly ConfigSync ConfigSync = new(ModGUID)
            { DisplayName = ModName, CurrentVersion = ModVersion, MinimumRequiredVersion = ModVersion };

        public enum Toggle
        {
            On = 1,
            Off = 0
        }

        public void Awake()
        {
            _serverConfigLocked = config("1 - General", "Lock Configuration", Toggle.On,
                "If on, the configuration is locked and can be changed by server admins only.");
            _ = ConfigSync.AddLockingConfigEntry(_serverConfigLocked);

            HealthString = config("1 - General", "Health String Format", "{0}/{1} (<color>{2}%</color>)",
                "Creature health format\n'{0}' is current health value\n'{1}' is total health value\n'{2}' is health percentage value");
            TamedColor = config("2 - Colors", "Tamed HB Color", new Color(0.2f, 0.62f, 0.4f, 1.0f), //"#339E66FF"
                "Color of the health bar for tamed creatures. This is the bar that is on top.");
            EnemyHbColor = config("2 - Colors", "Enemy HB Color", new Color(0.2f, 0.62f, 0.4f, 1.0f), //"#339E66FF"
                "Color of the health bar for tamed creatures. This is the bar that is under the top bar.");
            HighPercentColor = config("2 - Colors", "High Percent Color",
                new Color(0.2f, 0.62f, 0.4f, 1.0f), //"#339E66FF"
                "Color of the health bar's percentage text for creatures with high health percentage. 75% or higher.");
            HurtPercentColor = config("2 - Colors", "Hurt Percent Color",
                new Color(0.8f, 0.8f, 0.2f, 1.0f), //"#CC6633FF"
                "Color of the health bar's percentage text for creatures with relatively high health percentage. 50% or higher.");
            LowPercentColor = config("2 - Colors", "Low Percent Color", new Color(0.8f, 0.4f, 0.2f, 1.0f), //"#CC3333FF"
                "Color of the health bar's percentage text for creatures with low health percentage. 25% or higher.");
            CriticalPercentColor = config("2 - Colors", "Critical Percent Color",
                new Color(0.8f, 0.2f, 0.2f, 1.0f), //"#CC3333FF"
                "Color of the health bar's percentage text for creatures with critical health percentage. 0% or higher.");
            HealthbarScaleTamed = config("3 - Scaling", "Tamed Healthbar Scale", new Vector3(1f, 1f, 1f),
                "Scale of the health bar for tamed creatures.");
            HealthbarScaleEnemy = config("3 - Scaling", "Enemy Healthbar Scale", new Vector3(1f, 1f, 1f),
                "Scale of the health bar for creatures.");

            Assembly assembly = Assembly.GetExecutingAssembly();
            _harmony.PatchAll(assembly);
            SetupWatcher();
#if DEBUG
            AutoDoc();
#endif
        }

        private void Start()
        {
            if (Chainloader.PluginInfos.ContainsKey("org.bepinex.plugins.groups"))
            {
                GroupsIsInstalled = true;
            }

            Game.isModded = true;
        }

        private void OnDestroy()
        {
            Config.Save();
        }

        private void SetupWatcher()
        {
            FileSystemWatcher watcher = new(Paths.ConfigPath, ConfigFileName);
            watcher.Changed += ReadConfigValues;
            watcher.Created += ReadConfigValues;
            watcher.Renamed += ReadConfigValues;
            watcher.IncludeSubdirectories = true;
            watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
            watcher.EnableRaisingEvents = true;
        }

        private void ReadConfigValues(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(ConfigFileFullPath)) return;
            try
            {
                HealthDisplayLogger.LogDebug("ReadConfigValues called");
                Config.Reload();
            }
            catch
            {
                HealthDisplayLogger.LogError($"There was an issue loading your {ConfigFileName}");
                HealthDisplayLogger.LogError("Please check your config entries for spelling and format!");
            }
        }

        private void AutoDoc()
        {
            // Store Regex to get all characters after a [
            Regex regex = new(@"\[(.*?)\]");

            // Strip using the regex above from Config[x].Description.Description
            string Strip(string x) => regex.Match(x).Groups[1].Value;
            StringBuilder sb = new();
            string lastSection = "";
            foreach (ConfigDefinition x in Config.Keys)
            {
                // skip first line
                if (x.Section != lastSection)
                {
                    lastSection = x.Section;
                    sb.Append($"{Environment.NewLine}`{x.Section}`{Environment.NewLine}");
                }

                sb.Append($"\n{x.Key} [{Strip(Config[x].Description.Description)}]" +
                          $"{Environment.NewLine}   * {Config[x].Description.Description.Replace("[Synced with Server]", "").Replace("[Not Synced with Server]", "")}" +
                          $"{Environment.NewLine}     * Default Value: {Config[x].GetSerializedValue()}{Environment.NewLine}");
            }

            File.WriteAllText(
                Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
                    $"{ModName}_AutoDoc.md"), sb.ToString());
        }


        #region ConfigOptions

        private static ConfigEntry<Toggle> _serverConfigLocked = null!;
        internal static ConfigEntry<string> HealthString = null!;
        internal static ConfigEntry<Color> TamedColor = null!;
        internal static ConfigEntry<Color> EnemyHbColor = null!;
        internal static ConfigEntry<Color> HighPercentColor = null!;
        internal static ConfigEntry<Color> HurtPercentColor = null!;
        internal static ConfigEntry<Color> LowPercentColor = null!;
        internal static ConfigEntry<Color> CriticalPercentColor = null!;
        internal static ConfigEntry<Vector3> HealthbarScaleTamed = null!;
        internal static ConfigEntry<Vector3> HealthbarScaleEnemy = null!;

        private ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description,
            bool synchronizedSetting = true)
        {
            ConfigDescription extendedDescription =
                new(
                    description.Description +
                    (synchronizedSetting ? " [Synced with Server]" : " [Not Synced with Server]"),
                    description.AcceptableValues, description.Tags);
            ConfigEntry<T> configEntry = Config.Bind(group, name, value, extendedDescription);
            //var configEntry = Config.Bind(group, name, value, description);

            SyncedConfigEntry<T> syncedConfigEntry = ConfigSync.AddConfigEntry(configEntry);
            syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

            return configEntry;
        }

        private ConfigEntry<T> config<T>(string group, string name, T value, string description,
            bool synchronizedSetting = true)
        {
            return config(group, name, value, new ConfigDescription(description), synchronizedSetting);
        }

        private class ConfigurationManagerAttributes
        {
            public bool? Browsable = false;
        }

        class AcceptableShortcuts : AcceptableValueBase
        {
            public AcceptableShortcuts() : base(typeof(KeyboardShortcut))
            {
            }

            public override object Clamp(object value) => value;
            public override bool IsValid(object value) => true;

            public override string ToDescriptionString() =>
                "# Acceptable values: " + string.Join(", ", KeyboardShortcut.AllKeyCodes);
        }

        #endregion
    }
}