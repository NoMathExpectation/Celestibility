using Celeste;
using Celeste.Mod;
using Monocle;
using MonoMod.Utils;
using System;
using System.Runtime.InteropServices;
using System.Text;
using static Celeste.FancyText;
using static Celeste.TextMenu;

namespace NoMathExpectation.Celeste.Celestibility
{
    public class UniversalSpeech
    {
        private static class Internal
        {
            [DllImport("Celestibility/nativebin/UniversalSpeech", CharSet = CharSet.Auto)]
            public static extern int speechSay(string str, int interrput);

            [DllImport("Celestibility/nativebin/UniversalSpeech")]
            public static extern int speechStop();
        }

        public static bool Enabled => CelestibilityModule.Settings.SpeechEnabled;

        public static int SpeechSay(string str, bool interrupt = false, string def = null, bool ignoreOff = false)
        {
            if (!(Enabled || ignoreOff))
            {
                return 2;
            }

            if (def is null)
            {
                def = str;
            }

            try
            {
                if (Dialog.Has(str))
                {
                    string translated = Dialog.Clean(str);
                    LogUtil.Log($"Speech: {translated}", LogLevel.Verbose);
                    return Internal.speechSay(translated, interrupt ? 1 : 0);
                }
                else
                {
                    LogUtil.Log($"Speech: {def}", LogLevel.Verbose);
                    return Internal.speechSay(def, interrupt ? 1 : 0);
                }
            }
            catch (Exception)
            {
                LogUtil.Log($"Speech: {def}", LogLevel.Verbose);
                return Internal.speechSay(def, interrupt ? 1 : 0);
            }
        }

        public static int SpeechStop()
        {
            return Internal.speechStop();
        }

        public static void SpeechSay(Item item, bool ignoreOff = false)
        {
            if (!(Enabled || ignoreOff) || item is null)
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
                LogUtil.Log($"Textmenu item text found: {text}", LogLevel.Verbose);
                SpeechSay(text, true);
            }
            else
            {
                LogUtil.Log("Textmenu item text not found.", LogLevel.Verbose);
            }
        }

        public static void SpeechSay(FancyText.Text text, int start = 0, int end = int.MaxValue, bool ignoreOff = false)
        {
            if (!(Enabled || ignoreOff) || text is null)
            {
                return;
            }

            end = Math.Min(text.Count, end);
            StringBuilder sb = new StringBuilder();
            for (int i = start; i < end; i++)
            {
                Node node = text[i];
                if (node is NewLine)
                {
                    sb.AppendLine();
                }
                else if (node is FancyText.Char c)
                {
                    sb.Append((char)c.Character);
                }
                else if (node is NewPage)
                {
                    break;
                }
            }

            string t = sb.ToString();
            LogUtil.Log($"FancyText Text: {t}", LogLevel.Verbose);
            SpeechSay(t, true);
        }

        public static void SpeechSay(Entity entity, bool ignoreOff = false)
        {
            if (!(Enabled || ignoreOff) || entity is null)
            {
                return;
            }

            LogUtil.Log($"Entity: {entity}", LogLevel.Verbose);
            SpeechSay(entity.GetType().ToString());
        }

        public static void SpeechSay(MenuButton button, bool ignoreOff = false)
        {
            if (!(Enabled || ignoreOff) || button is null)
            {
                return;
            }

            LogUtil.Log($"MenuButton: {button}", LogLevel.Verbose);
            DynamicData data = DynamicData.For(button);
            SpeechSay(data.Get<string>("label"), true);
        }
    }
}
