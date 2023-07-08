using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework.Input;
using Monocle;
using MonoMod.Utils;
using System;
using System.Reflection;
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

            public static class Zdsr
            {
                [DllImport("Celestibility/nativebin/ZDSRAPI", CharSet = CharSet.Auto)]
                public static extern int InitTTS(int type, string channelName, bool keyDownInterrupt);

                [DllImport("Celestibility/nativebin/ZDSRAPI", CharSet = CharSet.Auto)]
                public static extern int Speak(string text, bool interrput);

                [DllImport("Celestibility/nativebin/ZDSRAPI")]
                public static extern int GetSpeakState();

                [DllImport("Celestibility/nativebin/ZDSRAPI")]
                public static extern void StopSpeak();

                public static bool Inited = false;
            }
        }

        public static bool Enabled => CelestibilityModule.Settings.SpeechEnabled;

        public static void TryInitZdsr()
        {
            if (Internal.Zdsr.InitTTS(0, null, false) == 0)
            {
                Internal.Zdsr.Inited = true;
            }
        }

        public static int SpeechSay(string str, bool interrupt = false, string def = null, bool ignoreOff = false)
        {
            if (!(Enabled || ignoreOff))
            {
                return -1;
            }

            if (def is null)
            {
                def = str;
            }

            string speech = "";
            try
            {
                if (Dialog.Has(str))
                {
                    string translated = Dialog.Clean(str);
                    LogUtil.Log($"Speech: {translated}", LogLevel.Verbose);
                    speech = translated;
                }
                else
                {
                    LogUtil.Log($"Speech: {def}", LogLevel.Verbose);
                    speech = def;
                }
            }
            catch (Exception)
            {
                LogUtil.Log($"Speech: {def}", LogLevel.Verbose);
                speech = def;
            }

            if (!Internal.Zdsr.Inited)
            {
                TryInitZdsr();
            }

            if (Internal.Zdsr.Inited && Internal.Zdsr.Speak(speech, interrupt) == 0)
            {
                return 0;
            }
            return Internal.speechSay(speech, interrupt ? 1 : 0);
        }

        public static int SpeechStop()
        {
            Internal.Zdsr.StopSpeak();
            return Internal.speechStop();
        }

        private static bool IsOption(object obj)
        {
            Type t = obj.GetType();
            do
            {
                if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Option<>))
                {
                    return true;
                }
                t = t.BaseType;
            } while (t != null);
            return false;
        }

        public static void SpeechSay(Item item, bool update = false)
        {
            if (!Enabled || item is null)
            {
                return;
            }

            LogUtil.Log($"Item: {item}", LogLevel.Verbose);

            DynamicData data = DynamicData.For(item);
            string text = "";

            if (!update)
            {
                text = data.Get<string>("Label");
                if (string.IsNullOrEmpty(text))
                {
                    text = data.Get<string>("Title");
                }
            }

            if (item is TextMenuExt.IntSlider slider)
            {
                text += ", " + slider.Index;
            }

            if (IsOption(item))
            {
                DynamicData values = DynamicData.For(data.Get("Values"));
                int index = data.Get<int>("Index");
                text += ", " + DynamicData.For(values.Invoke("get_Item", index)).Get<string>("Item1");
            }

            if (item is Setting bindSetting)
            {
                Binding binding = bindSetting.Binding;
                if (binding is not null)
                {
                    if (bindSetting.BindingController)
                    {
                        if (binding.Controller is not null)
                        {
                            foreach (Buttons b in binding.Controller)
                            {
                                string key = $"Celestibility_speech_Buttons_{b}";
                                if (Dialog.Has(key))
                                {
                                    text += ", " + Dialog.Clean(key);
                                }
                                else
                                {
                                    text += ", " + b;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (binding.Keyboard is not null)
                        {
                            foreach (Keys k in binding.Keyboard)
                            {
                                string key = $"Celestibility_speech_Keys_{k}";
                                if (Dialog.Has(key))
                                {
                                    text += ", " + Dialog.Clean(key);
                                }
                                else
                                {
                                    text += ", " + k;
                                }
                            }
                        }
                    }
                }
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
            Type entityType = entity.GetType();
            string fullNameKey = entityType.FullName.Replace('.', '_');

            string methodKey = $"Celestibility_speech_entity_invoke_{fullNameKey}";
            if (Dialog.Has(methodKey))
            {
                string method = Dialog.Clean(methodKey).Replace('_', '.');
                try
                {
                    int split = method.LastIndexOf('.');
                    Type invokeType = Type.GetType(method.Substring(0, split));
                    MethodInfo invokeMethod = invokeType.GetMethod(method.Substring(split + 1), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { entityType }, null);
                    invokeMethod.Invoke(null, new object[] { entity });
                    return;
                }
                catch (Exception e)
                {
                    LogUtil.Log($"Unable to invoke method {method} for entity speech {entityType.FullName}.", LogLevel.Error);
                    LogUtil.Log(e);
                }
            }

            string soundKey = $"Celestibility_speech_entity_sound_{fullNameKey}";
            if (Dialog.Has(soundKey))
            {
                string sound = Dialog.Clean(soundKey);
                try
                {
                    SoundEmitter.Play(sound);
                    return;
                }
                catch (Exception e)
                {
                    LogUtil.Log($"Unable to play sound {sound} for entity speech {entityType.FullName}.", LogLevel.Error);
                    LogUtil.Log(e);
                }
            }

            string textKey = $"Celestibility_speech_entity_text_{fullNameKey}";
            if (Dialog.Has(textKey))
            {
                string text = Dialog.Clean(textKey);
                SpeechSay(text);
                return;
            }

            LogUtil.Log($"Unable to find speech way for entity speech {entityType.FullName}.", LogLevel.Verbose);
            SpeechSay(entityType.ToString());
        }

        public static void SpeechSay(MenuButton button)
        {
            if (!Enabled || button is null)
            {
                return;
            }

            LogUtil.Log($"MenuButton: {button}", LogLevel.Verbose);
            DynamicData data = DynamicData.For(button);
            if (data.TryGet<string>("label", out var label))
            {
                SpeechSay(label, true);
            }
        }

        public static void SpeechSay(Strawberry berry)
        {
            bool collected = SaveData.Instance.CheckStrawberry(berry.ID);
            string text = Dialog.Clean($"Celestibility_speech_entity_text_Celeste_Strawberry_{(collected ? "" : "un")}collected");
            if (berry.Moon)
            {
                text += " " + Dialog.Clean("Celestibility_speech_entity_text_Celeste_Strawberry_moon");
            }
            else if (berry.Golden)
            {
                text += " " + Dialog.Clean("Celestibility_speech_entity_text_Celeste_Strawberry_golden");
            }
            else
            {
                text += " " + Dialog.Clean("Celestibility_speech_entity_text_Celeste_Strawberry_normal");
            }
            SpeechSay(text);
        }

        public static void SpeechSay(OuiFileSelectSlot slot)
        {
            if (slot is null)
            {
                return;
            }

            if (!slot.Exists)
            {
                SpeechSay("file_newgame", true);
                return;
            }

            if (slot.Corrupted)
            {
                SpeechSay("file_corrupted", true);
                return;
            }

            SpeechSay(string.Format(Dialog.Get("Celestibility_speech_fileslot"), slot.Name, slot.Time, slot.Deaths.Amount, slot.SaveData.TotalStrawberries_Safe), true);
        }

        public static void SpeechSay(OuiFileSelectSlot.Button button)
        {
            SpeechSay(button.Label, true);
        }

        public static void SpeechSay(OuiAssistMode assist, int index = 0, int enable = 1)
        {
            object pages = DynamicData.For(assist).Get("pages");
            int max = DynamicData.For(pages).Get<int>("Count");
            if (index == max)
            {
                SpeechSay("ASSIST_ASK", true);
                if (enable == 0)
                {
                    SpeechSay("ASSIST_YES");
                }
                else
                {
                    SpeechSay("ASSIST_NO");
                }
                return;
            }
            else if (index > max)
            {
                return;
            }

            object page = DynamicData.For(pages).Invoke("get_Item", index);
            FancyText.Text text = DynamicData.For(page).Get<FancyText.Text>("Text");
            SpeechSay(text);
        }

        public static void SpeechSayAssistUpdate(int enable)
        {
            if (enable == 0)
            {
                SpeechSay("ASSIST_YES", true);
            }
            else
            {
                SpeechSay("ASSIST_NO", true);
            }
        }
    }
}
