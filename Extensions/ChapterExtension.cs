﻿using MonoMod.Utils;
using NoMathExpectation.Celeste.Celestibility;
using System.Collections;

namespace Celeste.Mod.Celestibility.Extensions
{
    internal static class ChapterExtension
    {
        public static AreaKey GetCurrentAreaKey() => SaveData.Instance.LastArea_Safe;

        public static AreaData GetCurrentAreaData() => AreaData.Areas[GetCurrentAreaKey().ID];

        public static string GetCurrentLevelSetName() => DialogExt.CleanLevelSet(SaveData.Instance?.GetLevelSet() ?? "Celeste");

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

            if (!GetCurrentAreaData().Interlude_Safe)
            {
                text += ", " + key.GetChapter();
            }

            text += ", " + key.GetChapterName();

            text.SpeechSay(true);
        }

        public static void SpeechSay(this OuiChapterPanel panel)
        {
            if (!UniversalSpeech.Enabled || panel is null)
            {
                return;
            }

            DynamicData data = DynamicData.For(panel);

            DynamicData optionsListData = DynamicData.For(data.Get("options"));
            int optionSelecting = data.Get<int>("option");
            object option = optionsListData.Invoke("get_Item", optionSelecting);
            option.SpeechSayOuiChapterPanelOption();
        }

        public static void SpeechSayOuiChapterPanelOption(this object option)
        {
            if (!UniversalSpeech.Enabled || option is null)
            {
                return;
            }

            DynamicData data = DynamicData.For(option);
            data.Get<string>("Label").SpeechSay(true);
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

            DynamicData data = DynamicData.For(self);
            data.Set("origArea", data.Get<int>("area"));

            if (!UniversalSpeech.Enabled)
            {
                yield break;
            }

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

            if (origArea != area)
            {
                SpeechSayCurrentChapter();
            }

            data.Set("origArea", area);
        }

        public static IEnumerator OuiChapterPanelEnter(On.Celeste.OuiChapterPanel.orig_Enter orig, OuiChapterPanel self, Oui from)
        {
            yield return new SwapImmediately(orig(self, from));

            DynamicData data = DynamicData.For(self);
            data.Set("origSelectingMode", data.Get<bool>("selectingMode"));
            data.Set("origOption", data.Get<int>("option"));

            if (!UniversalSpeech.Enabled || data.Get<bool>("instantClose"))
            {
                yield break;
            }

            self.SpeechSay();
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
                self.SpeechSay();
            }

            data.Set("origSelectingMode", selectingMode);
            data.Set("origOption", option);
        }

    }
}
