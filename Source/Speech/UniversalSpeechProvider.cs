using System.Runtime.InteropServices;

namespace NoMathExpectation.Celeste.Celestibility.Speech
{
    internal class UniversalSpeechProvider : ISpeechProvider
    {
        public string Name => "Celestibility_SpeechProvider_UniversalSpeech";
        
        private const string dll = "UniversalSpeech";

        public void Say(string text, bool interrupt = false)
        {
            speechSay(text, interrupt ? 1 : 0);
        }

        public void Stop()
        {
            speechStop();
        }

        [DllImport(dll, CharSet = CharSet.Auto)]
        private static extern int speechSay(string str, int interrput);

        [DllImport(dll, CharSet = CharSet.Auto)]
        private static extern void speechStop();
    }
}
