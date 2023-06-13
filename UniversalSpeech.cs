using Celeste;
using Celeste.Mod;
using MonoMod.Utils;
using System;
using System.Runtime.InteropServices;
using static Celeste.TextMenu;

namespace NoMathExpectation.Celeste.Celestibility
{
    internal class UniversalSpeech
    {
        private static class Internal
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

        public static void speechSay(Item item)
        {
            if (item is null)
            {
                return;
            }

            DynamicData data = DynamicData.For(item);
            string text = data.Get<string>("Label");
            if (string.IsNullOrEmpty(text))
            {
                text = data.Get<string>("Title");
            }

            if (!string.IsNullOrEmpty(text))
            {
                LogUtil.log($"Textmenu item text found: {text}", LogLevel.Verbose);
                speechSay(text, true);
            }
            else
            {
                LogUtil.log("Textmenu item text not found.", LogLevel.Verbose);
            }
        }
    }
}
