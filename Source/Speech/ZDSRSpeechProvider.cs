using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NoMathExpectation.Celeste.Celestibility.Speech
{
    public class ZDSRSpeechProvider : ISpeechProvider
    {
        public string Name => "Celestibility_SpeechProvider_ZDSR";

        private const string dll = "Mods/Cache/Celestibility/nativebin/ZDSRAPI_x64";

        public ZDSRSpeechProvider() {
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
