using System.Runtime.InteropServices;

namespace NoMathExpectation.Celeste.Celestibility.Speech
{
    public class NVDASpeechProvider : ISpeechProvider
    {
        string ISpeechProvider.Name => "Celestibility_SpeechProvider_NVDA";

        private const string dll = "nvdaControllerClient";

        public void Say(string text, bool interrupt = false)
        {
            if (interrupt)
            {
                nvdaController_cancelSpeech();
            }
            nvdaController_speakText(text);
        }

        public void Stop()
        {
            nvdaController_cancelSpeech();
        }

        [DllImport(dll, CharSet = CharSet.Auto)]
        private static extern int nvdaController_cancelSpeech();

        [DllImport(dll, CharSet = CharSet.Auto)]
        private static extern int nvdaController_speakText(string text);
    }
}
