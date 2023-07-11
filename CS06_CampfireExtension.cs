using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.Utils;
using NoMathExpectation.Celeste.Celestibility;

namespace Celeste.Mod.Celestibility.Extensions
{
    internal static class CS06_CampfireExtension
    {
        public static void InjectGetCurrentOption(this ILCursor cursor, bool prompt)
        {
            cursor.Emit(OpCodes.Ldloc_1);

            cursor.Emit(OpCodes.Ldloc_1);
            cursor.EmitDelegate(GetCurrentOption);

            cursor.EmitReference(prompt);

            cursor.EmitDelegate(SpeechSayOption);
        }

        public static object GetCurrentOption(this CS06_Campfire cutscene)
        {
            DynamicData data = DynamicData.For(cutscene);
            object list = data.Get("currentOptions");
            int index = data.Get<int>("currentOptionIndex");
            return DynamicData.For(list).Invoke("get_Item", index);
        }

        public static void SpeechSayOption(this CS06_Campfire cutscene, object option, bool prompt)
        {
            if (prompt)
            {
                string.Format(Dialog.Get("Celestibility_cutscene_select"), Input.MenuUp.SpeechRender(), Input.MenuDown.SpeechRender()).SpeechSay(true);
            }
            option.SpeechSayCS06_CampfireOption(!prompt);
        }
    }
}
