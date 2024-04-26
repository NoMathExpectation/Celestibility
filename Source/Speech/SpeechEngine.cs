using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace NoMathExpectation.Celeste.Celestibility.Speech
{
    public static class SpeechEngine
    {
        private static readonly Dictionary<string, ISpeechProvider> providers = [];

        public static void RegisterProvider(ISpeechProvider provider)
        {
            providers[provider.Name] = provider;
        }

        public static string CurrentName { get; private set; }
        public static ISpeechProvider Current
        {
            get
            {
                providers.TryGetValue(CurrentName, out var provider);
                return provider;
            }
        }

        public static void SetCurrent(string name)
        {
            CurrentName = name;
        }

        public static string TrySetCurrentDefault()
        {
            if (CurrentName is not null && providers.ContainsKey(CurrentName))
            {
                return CurrentName;
            }

            CurrentName = providers.Keys.FirstOrDefault(CelestibilityModuleSettings.speechProviderNone);
            return CurrentName;
        }

        public static ISpeechProvider GetProvider(string name)
        {
            providers.TryGetValue(name, out var provider);
            return provider;
        }

        public static ReadOnlyDictionary<string, ISpeechProvider> GetProviders()
        {
            return providers.AsReadOnly();
        }

        public static void Say(string text, bool interrupt = false)
        {
            if (CurrentName is null || !providers.ContainsKey(CurrentName))
            {
                return;
            }

            Current.Say(text, interrupt);
        }

        public static void Stop()
        {
            if (CurrentName is null || !providers.ContainsKey(CurrentName))
            {
                return;
            }

            Current.Stop();
        }

        internal static void Init()
        {
            RegisterProvider(new UniversalSpeechProvider());
            RegisterProvider(new NVDASpeechProvider());
            RegisterProvider(new ZDSRSpeechProvider());
            RegisterProvider(new BoyCtrlSpeechProvider());
            CurrentName = CelestibilityModule.Settings.SpeechProvider;
        }
    }
}
