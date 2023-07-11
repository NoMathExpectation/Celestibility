using MonoMod.Utils;
using NoMathExpectation.Celeste.Celestibility;

namespace Celeste.Mod.Celestibility.Extensions
{
    internal static class TextboxExtension
    {
        public static void MarkNewPage(this Textbox textbox)
        {
            DynamicData data = DynamicData.For(textbox);
            data.Set("NewPageMark", true);
        }

        public static void CharEncountered(this Textbox textbox)
        {
            DynamicData data = DynamicData.For(textbox);
            bool say = data.Get<bool?>("NewPageMark") ?? true;
            if (say)
            {
                data.Get<FancyText.Text>("text").SpeechSay(data.Get<int>("index"));
            }
            data.Set("NewPageMark", false);
        }
    }
}
