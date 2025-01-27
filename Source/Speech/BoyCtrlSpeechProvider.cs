using Celeste.Mod;
using System.Runtime.InteropServices;

namespace NoMathExpectation.Celeste.Celestibility.Speech
{
    public class BoyCtrlSpeechProvider : ISpeechProvider
    {
        string ISpeechProvider.Name => "Celestibility_SpeechProvider_BoyCtrl";

        private const string dll = "BoyCtrl";

        public bool UseSlave;

        public bool Initialized { get; private set; } = false;

        public void Say(string text, bool interrupt = false)
        {
            if (!Initialized)
            {
                LogUtil.Log("Initializing BoyCtrl...", LogLevel.Info);
                if (BoyCtrlInitializeAnsi(null) != BoyCtrlError.e_bcerr_success)
                {
                    LogUtil.Log("Cannot initialize BoyCtrl!", LogLevel.Warn);
                    return;
                }
                Initialized = true;
            }
            if (!BoyCtrlIsReaderRunning())
            {
                LogUtil.Log("BoyCtrl reader is not running!", LogLevel.Warn);
            }
            if (interrupt)
            {
                BoyCtrlStopSpeaking(UseSlave);
            }
            BoyCtrlSpeakAnsi(text, UseSlave, !interrupt, false, (value) => { });
        }

        public void Stop()
        {
            BoyCtrlStopSpeaking(UseSlave);
        }

        public BoyCtrlSpeechProvider(bool useSlave = true)
        {
            UseSlave = useSlave;
        }

        ~BoyCtrlSpeechProvider()
        {
            Initialized = false;
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

        [DllImport(dll, CharSet = CharSet.Ansi)]
        private static extern BoyCtrlError BoyCtrlInitializeAnsi(string logPath);

        private delegate void BoyCtrlSpeakCompleteFunc(int reason);

        [DllImport(dll, CharSet = CharSet.Ansi)]
        private static extern BoyCtrlError BoyCtrlSpeakAnsi(string text, bool withSlave, bool append, bool allowBreak, [MarshalAs(UnmanagedType.FunctionPtr)] BoyCtrlSpeakCompleteFunc onCompletion);

        [DllImport(dll, CharSet = CharSet.Ansi)]
        private static extern BoyCtrlError BoyCtrlStopSpeaking(bool withSlave);

        [DllImport(dll, CharSet = CharSet.Ansi)]
        private static extern void BoyCtrlUninitialize();

        [DllImport(dll, CharSet = CharSet.Ansi)]
        private static extern bool BoyCtrlIsReaderRunning();
    }
}
