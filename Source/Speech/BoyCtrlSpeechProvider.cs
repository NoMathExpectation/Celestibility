using System.Runtime.InteropServices;

namespace NoMathExpectation.Celeste.Celestibility.Speech
{
    public class BoyCtrlSpeechProvider : ISpeechProvider
    {
        string ISpeechProvider.Name => "Celestibility_SpeechProvider_BoyCtrl";

        private const string dll = "Mods/Cache/Celestibility/nativebin/BoyCtrl-x64";

        public void Say(string text, bool interrupt = false)
        {
            if (interrupt)
            {
                BoyCtrlStopSpeaking(true);
            }
            BoyCtrlSpeakU8(text, true, !interrupt, false, (value) => { });
        }

        public void Stop()
        {
            BoyCtrlStopSpeaking(true);
        }

        public BoyCtrlSpeechProvider()
        {
            BoyCtrlInitializeU8(null);
        }

        ~BoyCtrlSpeechProvider()
        {
            BoyCtrlUninitialize();
        }

        private enum BoyCtrlError
        {
            // 成功
            e_bcerr_success = 0,
            // 一般错误
            e_bcerr_fail = 1,
            // 参数错误（如字符串为空或过长）
            e_bcerr_arg = 2,
            // 功能不可用
            e_bcerr_unavailable = 3,
        }

        [DllImport(dll, CharSet = CharSet.Auto)]
        private static extern int BoyCtrlInitializeU8(string logPath);

        private delegate void BoyCtrlSpeakCompleteFunc(int reason);

        [DllImport(dll, CharSet = CharSet.Auto)]
        private static extern int BoyCtrlSpeakU8(string text, bool withSlave, bool append, bool allowBreak, [MarshalAs(UnmanagedType.FunctionPtr)] BoyCtrlSpeakCompleteFunc onCompletion);

        [DllImport(dll, CharSet = CharSet.Auto)]
        private static extern int BoyCtrlStopSpeaking(bool withSlave);

        [DllImport(dll, CharSet = CharSet.Auto)]
        private static extern int BoyCtrlUninitialize();
    }
}
