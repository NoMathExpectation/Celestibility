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

        public override void Load()
        {
            LogUtil.log("Extracting dlls...");
            string cachePath = "Mods/Cache/Celestibility/nativebin";
            if (!Directory.Exists(cachePath))
            {
                Directory.CreateDirectory(cachePath);
            }
            string[] dlls = { "dolapi.dll", "jfwapi.dll", "nvdaControllerClient.dll", "SAAPI32.dll", "UniversalSpeech.dll" };
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
                    LogUtil.log("Dlls are currently at use, skipping.", LogLevel.Warn);
                    break;
                }
            }

            Hooks.hook();

            UniversalSpeech.speechSay("Celestibility mod loaded.");
            LogUtil.log("Mod loaded.");
        }

        public override void Unload()
        {
            Hooks.unhook();

            UniversalSpeech.speechSay("Celestibility mod unloaded.");
            LogUtil.log("Mod unloaded.");
        }
    }
}