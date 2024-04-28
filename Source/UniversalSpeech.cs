using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using MonoMod.Utils;
using NoMathExpectation.Celeste.Celestibility.Speech;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using static Celeste.FancyText;
using static Celeste.TextMenu;

namespace NoMathExpectation.Celeste.Celestibility
{
    public static partial class UniversalSpeech
    {
        public static bool Enabled => CelestibilityModule.Settings.SpeechEnabled;

        public static bool SpeechSay(this string str, bool interrupt = false, string def = null, bool ignoreOff = false)
        {
            if (!(Enabled || ignoreOff))
            {
                return false;
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
                    string translated = str.DialogClean();
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

            SpeechEngine.Say(speech, interrupt);
            return true;
        }

        public static bool SpeechStop()
        {
            SpeechEngine.Stop();
            return true;
        }

        private static bool IsOption(this object obj)
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

        public static void SpeechSay(this TextMenu.Item item, bool update = false)
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

            if (item.IsOption())
            {
                DynamicData values = DynamicData.For(data.Get("Values"));
                int count = values.Get<int>("Count");
                if (count > 0) {
                    int index = data.Get<int>("Index");
                    text += ", " + DynamicData.For(values.Invoke("get_Item", index)).Get<string>("Item1");
                }
            }

            if (item is TextMenuExt.OptionSubMenu optionSubMenu)
            {
                text += ", " + optionSubMenu.Menus[optionSubMenu.MenuIndex].Item1;
                if (optionSubMenu.CurrentMenu.Count > 0)
                {
                    text += ", " + "Celestibility_OptionSubMenu_press_hint".DialogClean();
                }
            }

            if (item is TextMenu.Setting bindSetting)
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
                                    text += ", " + key.DialogClean();
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
                                    text += ", " + key.DialogClean();
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
                text.SpeechSay(true);
            }
            else
            {
                LogUtil.Log("Textmenu item text not found.", LogLevel.Verbose);
            }
        }

        public static void SpeechSay(this FancyText.Text text, int start = 0, int end = int.MaxValue, bool interrupt = true)
        {
            if (!Enabled || text is null)
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
            t.SpeechSay(interrupt);
        }

        public static void SpeechSay(this Entity entity, bool ignoreOff = false)
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
                string method = methodKey.DialogClean().Replace('_', '.');
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
                string sound = soundKey.DialogClean();
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
                string text = textKey.DialogClean();
                text.SpeechSay();
                return;
            }

            LogUtil.Log($"Unable to find speech way for entity speech {entityType.FullName}.", LogLevel.Verbose);
            entityType.ToString().SpeechSay();
        }

        public static void SpeechSay(this MenuButton button)
        {
            if (!Enabled || button is null)
            {
                return;
            }

            LogUtil.Log($"MenuButton: {button}", LogLevel.Verbose);
            DynamicData data = DynamicData.For(button);
            if (data.TryGet<string>("label", out var label))
            {
                label.SpeechSay(true);
            }
        }

        public static void SpeechSay(this Strawberry berry)
        {
            if (!Enabled || berry is null)
            {
                return;
            }

            bool collected = SaveData.Instance.CheckStrawberry(berry.ID);
            string text = berry.Winged ? Dialog.Clean("Celestibility_speech_entity_text_Celeste_Strawberry_winged") : "";
            text += " " + Dialog.Clean($"Celestibility_speech_entity_text_Celeste_Strawberry_{(collected ? "" : "un")}collected");
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
            text.SpeechSay();
        }

        public static string SpeechRender(this StrawberriesCounter counter)
        {
            if (counter is null)
            {
                return "";
            }

            string result = "" + counter.Amount;
            if (counter.ShowOutOf && counter.OutOf > -1)
            {
                result += "/" + counter.OutOf;
            }

            return result;
        }

        public static void SpeechSay(this OuiFileSelectSlot slot)
        {
            if (!Enabled || slot is null)
            {
                return;
            }

            if (!slot.Exists)
            {
                "file_newgame".SpeechSay(true);
                return;
            }

            if (slot.Corrupted)
            {
                "file_corrupted".SpeechSay(true);
                return;
            }

            string.Format(Dialog.Get("Celestibility_speech_fileslot"), slot.Name, slot.Time, slot.Deaths.Amount, slot.Strawberries.SpeechRender()).SpeechSay(true);
        }

        public static void SpeechSay(this OuiFileSelectSlot.Button button)
        {
            if (!Enabled || button is null)
            {
                return;
            }

            button.Label.SpeechSay(true);
        }

        public static void SpeechSay(this OuiAssistMode assist, int index = 0, int enable = 1)
        {
            if (!Enabled || assist is null)
            {
                return;
            }

            object pages = DynamicData.For(assist).Get("pages");
            int max = DynamicData.For(pages).Get<int>("Count");
            if (index == max)
            {
                "ASSIST_ASK".SpeechSay(true);
                if (enable == 0)
                {
                    "ASSIST_YES".SpeechSay();
                }
                else
                {
                    "ASSIST_NO".SpeechSay();
                }
                return;
            }
            else if (index > max)
            {
                return;
            }

            object page = DynamicData.For(pages).Invoke("get_Item", index);
            FancyText.Text text = DynamicData.For(page).Get<FancyText.Text>("Text");
            text.SpeechSay();
        }

        public static void SpeechSayAssistUpdate(int enable)
        {
            if (!Enabled)
            {
                return;
            }

            if (enable == 0)
            {
                "ASSIST_YES".SpeechSay(true);
            }
            else
            {
                "ASSIST_NO".SpeechSay(true);
            }
        }

        public static string SpeechRender(this VirtualButton button)
        {
            return button.Binding.SpeechRender();
        }

        public static string SpeechRender(this Binding binding)
        {
            if (binding is null)
            {
                return "";
            }

            if (Input.GuiInputController())
            {
                if (binding.Controller is not null)
                {
                    foreach (Buttons b in binding.Controller)
                    {
                        string key = $"Celestibility_speech_Buttons_{b}";
                        if (Dialog.Has(key))
                        {
                            return key.DialogClean();
                        }
                        else
                        {
                            return b.ToString();
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
                            return key.DialogClean();
                        }
                        else
                        {
                            return k.ToString();
                        }
                    }
                }
            }

            return "Celestibility_Binding_unbound".DialogClean();
        }

        public static string SpeechRender(this Vector2 vec0)
        {
            Vector2 vec = new Vector2(vec0.X, vec0.Y);
            if (SaveData.Instance is not null && SaveData.Instance.Assists.MirrorMode)
            {
                vec.X = -vec.X;
            }

            string xd = vec.X switch
            {
                < 0 => "left",
                > 0 => "right",
                _ => "zero"
            };
            string yd = vec.Y switch
            {
                < 0 => "up",
                > 0 => "down",
                _ => "zero"
            };

            return $"Celestibility_BirdTutorialGui_Vector2_{yd}_{xd}".DialogClean();
        }

        private static string SpeechRenderBirdTutorialGuiContent(this object obj)
        {
            LogUtil.Log($"BirdTutorialGui content: {obj}", LogLevel.Verbose);
            return obj switch
            {
                null => "",
                string str => str,
                BirdTutorialGui.ButtonPrompt prompt => BirdTutorialGui.ButtonPromptToVirtualButton(prompt).SpeechRender(),
                Vector2 direction => direction.SpeechRender(),
                MTexture => "Celestibility_BirdTutorialGui_MTexture".DialogClean(),
                _ => "Celestibility_BirdTutorialGui_unknown".DialogClean()
            };
        }

        public static void SpeechSay(this BirdTutorialGui gui)
        {
            LogUtil.Log($"BirdTutorialGui: {gui}", LogLevel.Verbose);
            if (!Enabled || gui is null)
            {
                return;
            }

            StringBuilder sb = new StringBuilder();
            DynamicData data = DynamicData.For(gui);

            object info = data.Get("info");
            List<object> controls = data.Get<List<object>>("controls");

            sb.Append(info.SpeechRenderBirdTutorialGuiContent());

            foreach (object control in controls)
            {
                sb.Append(", ").Append(control.SpeechRenderBirdTutorialGuiContent());
            }

            sb.ToString().SpeechSay();
        }

        internal static void SpeechSayCS06_CampfireOption(this object option, bool interrupt)
        {
            if (!Enabled || option is null)
            {
                return;
            }

            object question = DynamicData.For(option).Get("Question");
            FancyText.Text text = DynamicData.For(question).Get<FancyText.Text>("AskText");
            text.SpeechSay(interrupt: interrupt);
        }

        public static void SpeechSay(this MiniTextbox box)
        {
            if (!Enabled || box is null)
            {
                return;
            }

            DynamicData.For(box).Get<FancyText.Text>("text").SpeechSay();
        }

        internal static void SpeechSayVignette<T>(this T intro)
        {
            if (!Enabled || intro is null)
            {
                return;
            }

            DynamicData data = DynamicData.For(intro);

            FancyText.Text text = data.Get<FancyText.Text>("text");
            int textStart = data.Get<int>("textStart");
            text.SpeechSay(textStart);
        }

        public static void SpeechSay(this Poem poem)
        {
            if (!Enabled || poem is null)
            {
                return;
            }

            DynamicData data = DynamicData.For(poem);
            data.Get<string>("text").SpeechSay(true);
        }

        public static void SpeechSay(this MemorialText text)
        {
            if (!Enabled || text is null)
            {
                return;
            }

            DynamicData data = DynamicData.For(text);
            string message = data.Get<string>("message");

            if (!text.Dreamy)
            {
                message.SpeechSay(true);
                return;
            }

            List<char> chars = message.ToList();
            Calc.PushRandom();
            chars.Shuffle();
            Calc.PopRandom();

            StringBuilder sb = new StringBuilder();
            foreach (char c in chars)
            {
                sb.Append(c);
            }
            sb.ToString().SpeechSay(true);
        }

        public static void SpeechSay(this AutoSavingNotice notice) {
            "autosaving_title_PC".SpeechSay();
            "autosaving_desc_PC".SpeechSay();
        }
    }
}
