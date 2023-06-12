using Celeste;
using System;
using System.Runtime.InteropServices;

namespace NoMathExpectation.Celeste.Celestibility
{
    internal class UniversalSpeech
    {
        private class Internal
        {
            [DllImport("Celestibility/nativebin/UniversalSpeech", CharSet = CharSet.Auto)]
            public static extern int speechSay(string str, int interrput);

            [DllImport("Celestibility/nativebin/UniversalSpeech")]
            public static extern int speechStop();
        }

        public static int speechSay(string str, bool interrupt = false)
        {
            try
            {
                if (Dialog.Has(str))
                {
                    return Internal.speechSay(Dialog.Clean(str), interrupt ? 1 : 0);
                }
                else
                {
                    return Internal.speechSay(str, interrupt ? 1 : 0);
                }
            }
            catch (Exception)
            {
                return Internal.speechSay(str, interrupt ? 1 : 0);
            }
        }

        public static int speechStop()
        {
            return Internal.speechStop();
        }
    }
}
