using MonoMod.Utils;
using NoMathExpectation.Celeste.Celestibility;
using System.Collections;

namespace Celeste.Mod.Celestibility.Extensions
{
    internal static class ChapterExtension
    {
        public static AreaKey GetCurrentAreaKey() => SaveData.Instance.LastArea_Safe;

        public static AreaData GetCurrentAreaData() => AreaData.Areas[GetCurrentAreaKey().ID];

        public static string GetCurrentLevelSetName() => Dialog.CleanLevelSet(SaveData.Instance?.LevelSet ?? "Celeste");

        public static string GetChapter(this OuiChapterPanel panel) => DynamicData.For(panel).Get<string>("chapter");

        public static string GetChapter(this AreaKey key) => Dialog.Get("area_chapter").Replace("{x}", key.ChapterIndex.ToString().PadLeft(2));

        public static string GetChapterName(this AreaKey key) => Dialog.Clean(AreaData.Get(key).Name);

        public static string GetChapterName(this AreaData data) => Dialog.Clean(data.Name);

        public static void SpeechSayCurrentChapter(bool levelSet = false)
        {
            AreaKey key = GetCurrentAreaKey();
            string text = "";

            if (levelSet)
            {
                text += ", " + GetCurrentLevelSetName();
            }

            if (SaveData.Instance.AssistMode && key.ID >= SaveData.Instance.UnlockedAreas_Safe + 1)
            {
                text += ", " + "ASSIST_SKIP".DialogClean();
            }
            else
            {
                if (!GetCurrentAreaData().Interlude_Safe)
                {
                    text += ", " + key.GetChapter();
                }

                text += ", " + key.GetChapterName();
            }

            text.SpeechSay(true);
        }

        public static void SpeechSay(this OuiChapterPanel panel, bool name = false)
        {
            if (!UniversalSpeech.Enabled || panel is null)
            {
                return;
            }

            if (name)
            {
                SpeechSayCurrentChapter();
            }

            panel.SpeechSayOuiChapterPanelOption(!name);
        }

        public static void SpeechSayOuiChapterPanelOption(this OuiChapterPanel panel, bool interrupt = true)
        {
            if (!UniversalSpeech.Enabled || panel is null)
            {
                return;
            }

            DynamicData data = DynamicData.For(panel);

            DynamicData optionsListData = DynamicData.For(data.Get("options"));
            int optionSelecting = data.Get<int>("option");
            object option = optionsListData.Invoke("get_Item", optionSelecting);
            option.SpeechSayOuiChapterPanelOption(interrupt);
        }

        public static void SpeechSayOuiChapterPanelOption(this object option, bool interrupt = true)
        {
            if (!UniversalSpeech.Enabled || option is null)
            {
                return;
            }

            DynamicData data = DynamicData.For(option);
            data.Get<string>("Label").SpeechSay(interrupt);
        }

        public static void Hook()
        {
            On.Celeste.OuiChapterSelect.Enter += OuiChapterSelectEnter;
            On.Celeste.OuiChapterSelect.Update += OuiChapterSelectUpdate;

            On.Celeste.OuiChapterPanel.Enter += OuiChapterPanelEnter;
            On.Celeste.OuiChapterPanel.Update += OuiChapterPanelUpdate;
        }

        public static void Unhook()
        {
            On.Celeste.OuiChapterSelect.Enter -= OuiChapterSelectEnter;
            On.Celeste.OuiChapterSelect.Update -= OuiChapterSelectUpdate;

            On.Celeste.OuiChapterPanel.Enter -= OuiChapterPanelEnter;
            On.Celeste.OuiChapterPanel.Update -= OuiChapterPanelUpdate;
        }

        public static IEnumerator OuiChapterSelectEnter(On.Celeste.OuiChapterSelect.orig_Enter orig, OuiChapterSelect self, Oui from)
        {
            yield return new SwapImmediately(orig(self, from));

            if (!UniversalSpeech.Enabled)
            {
                yield break;
            }

            DynamicData data = DynamicData.For(self);
            DynamicData iconListData = DynamicData.For(data.Get("icons"));
            int area = data.Get<int>("area");
            data.Set("origArea", area);

            OuiChapterSelectIcon currentIcon = iconListData.Invoke<OuiChapterSelectIcon>("get_Item", area);
            data.Set("origCurrentAssistModeUnlockable", currentIcon.AssistModeUnlockable);

            if (from is not OuiChapterPanel)
            {
                SpeechSayCurrentChapter(true);
            }
        }

        public static void OuiChapterSelectUpdate(On.Celeste.OuiChapterSelect.orig_Update orig, OuiChapterSelect self)
        {
            orig(self);

            DynamicData data = DynamicData.For(self);
            if (!UniversalSpeech.Enabled || !data.Get<bool>("display"))
            {
                return;
            }

            int origArea = data.Get<int?>("origArea") ?? data.Get<int>("area");
            int area = data.Get<int>("area");

            DynamicData iconListData = DynamicData.For(data.Get("icons"));
            OuiChapterSelectIcon currentIcon = iconListData.Invoke<OuiChapterSelectIcon>("get_Item", area);
            bool origCurrentAssistModeUnlockable = data.Get<bool?>("origCurrentAssistModeUnlockable") ?? currentIcon.AssistModeUnlockable;
            bool currentAssistModeUnlockable = currentIcon.AssistModeUnlockable;

            if (origArea != area || origCurrentAssistModeUnlockable != currentAssistModeUnlockable)
            {
                SpeechSayCurrentChapter();
            }

            data.Set("origArea", area);
            data.Set("origCurrentAssistModeUnlockable", currentAssistModeUnlockable);
        }

        public static IEnumerator OuiChapterPanelEnter(On.Celeste.OuiChapterPanel.orig_Enter orig, OuiChapterPanel self, Oui from)
        {
            yield return new SwapImmediately(orig(self, from));

            DynamicData data = DynamicData.For(self);

            if (!UniversalSpeech.Enabled || data.Get<bool>("instantClose"))
            {
                yield break;
            }

            self.SpeechSay(from is not OuiChapterSelect);
        }

        public static void OuiChapterPanelUpdate(On.Celeste.OuiChapterPanel.orig_Update orig, OuiChapterPanel self)
        {
            orig(self);

            DynamicData data = DynamicData.For(self);
            if (!UniversalSpeech.Enabled)
            {
                return;
            }

            bool origSelectingMode = data.Get<bool?>("origSelectingMode") ?? data.Get<bool>("selectingMode");
            bool selectingMode = data.Get<bool>("selectingMode");
            int origOption = data.Get<int?>("origOption") ?? data.Get<int>("option");
            int option = data.Get<int>("option");

            if (origSelectingMode != selectingMode || origOption != option)
            {
                self.SpeechSayOuiChapterPanelOption();
            }

            data.Set("origSelectingMode", selectingMode);
            data.Set("origOption", option);
        }

    }
}
