using Celeste.Mod;
using System;
using System.IO;

namespace NoMathExpectation.Celeste.Celestibility
{
    public class CelestibilityModule : EverestModule
    {
        public static CelestibilityModule Instance { get; private set; }

        public override Type SettingsType => typeof(CelestibilityModuleSettings);
        public static CelestibilityModuleSettings Settings => (CelestibilityModuleSettings)Instance._Settings;

        public override Type SessionType => typeof(CelestibilityModuleSession);
        public static CelestibilityModuleSession Session => (CelestibilityModuleSession)Instance._Session;

        public CelestibilityModule()
        {
            Instance = this;
#if DEBUG
            // debug builds use verbose logging
            Logger.SetLogLevel("Celestibility", LogLevel.Verbose);
#else
            // release builds use info logging to reduce spam in log files
            Logger.SetLogLevel("Celestibility", LogLevel.Info);
#endif
        }

        private static void ExtractDlls()
        {
            LogUtil.Log("Extracting dlls...");
            string cachePath = "Mods/Cache/Celestibility/nativebin";
            if (!Directory.Exists(cachePath))
            {
                Directory.CreateDirectory(cachePath);
            }
            string[] dlls = { "dolapi.dll", "jfwapi.dll", "nvdaControllerClient.dll", "SAAPI32.dll", "UniversalSpeech.dll", "ZDSRAPI.dll" };
            foreach (string dll in dlls)
            {
                try
                {
                    ModAsset asset = Everest.Content.Get($"nativebin/{dll}");
                    using Stream stream = asset.Stream;
                    using Stream destination = File.OpenWrite(Path.Combine(cachePath, dll));
                    stream.CopyTo(destination);
                }
                catch (IOException)
                {
                    LogUtil.Log("Dlls are currently at use, skipping.", LogLevel.Warn);
                    break;
                }
            }
        }

        public override void Load()
        {
            ExtractDlls();

            Hooks.Hook();

            UniversalSpeech.SpeechSay("Celestibility_loaded", def: "Celestibility mod loaded.");
            LogUtil.Log("Mod loaded.");
        }

        public override void Unload()
        {
            Hooks.Unhook();

            UniversalSpeech.SpeechSay("Celestibility_unloaded", def: "Celestibility mod unloaded.");
            LogUtil.Log("Mod unloaded.");
        }
    }
}