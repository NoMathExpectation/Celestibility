using Celeste.Mod;
using System.IO;
using System.Runtime.InteropServices;

namespace NoMathExpectation.Celeste.Celestibility.Speech
{
    public class ZDSRSpeechProvider : ISpeechProvider
    {
        public string Name => "Celestibility_SpeechProvider_ZDSR";

        private const string dll = "ZDSRAPI";

        public ZDSRSpeechProvider()
        {
            string zdsrini = "ZDSRAPI.ini";
            if (!File.Exists(zdsrini))
            {
                try
                {
                    ModAsset asset = Everest.Content.Get($"{zdsrini}");
                    using Stream stream = asset.Stream;
                    using Stream destination = File.OpenWrite(zdsrini);
                    stream.CopyTo(destination);
                }
                catch (IOException)
                {
                    LogUtil.Log($"Failed to copy {zdsrini} to main folder.", LogLevel.Warn);
                }
            }

            InitTTS(1, "Celestibility", false);
        }

        public void Say(string text, bool interrupt = false)
        {
            Speak(text, interrupt);
        }

        public void Stop()
        {
            StopSpeak();
        }

        [DllImport(dll, CharSet = CharSet.Auto)]
        private static extern int InitTTS(int type, string channelName, bool bKeyDownInterrupt);

        [DllImport(dll, CharSet = CharSet.Auto)]
        private static extern int Speak(string text, bool bInterrupt);

        [DllImport(dll, CharSet = CharSet.Auto)]
        private static extern void StopSpeak();
    }
}
