using Celeste.Mod;
using NoMathExpectation.Celeste.Celestibility.Speech;
using System;

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
            // ExtractDlls();
            SpeechEngine.Init();

            Hooks.Hook();

            "Celestibility_loaded".SpeechSay(def: "Celestibility mod loaded.");
            LogUtil.Log("Mod loaded.");
        }

        public override void Initialize()
        {
            base.Initialize();
            Settings.SpeechProvider = SpeechEngine.TrySetCurrentDefault();
        }

        public override void Unload()
        {
            Hooks.Unhook();

            "Celestibility_unloaded".SpeechSay(def: "Celestibility mod unloaded.");
            LogUtil.Log("Mod unloaded.");
        }
    }
}