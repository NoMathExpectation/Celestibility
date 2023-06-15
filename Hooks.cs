using Celeste;
using Microsoft.Xna.Framework;
using NoMathExpectation.Celeste.Celestibility.Entities;
using System;
using System.Linq;
using static Celeste.TextMenu;

namespace NoMathExpectation.Celeste.Celestibility
{
    internal class Hooks
    {
        internal static void Hook()
        {
            LogUtil.Log("Hooking.");

            //On.Celeste.TextMenu.Item.Enter += textMenuItemEnter;
            //On.Celeste.TextMenu.Item.ctor += textMenuItemCtor;
            On.Celeste.TextMenu.MoveSelection += TextMenuMoveSelection;
            On.Celeste.TextMenuExt.SubMenu.MoveSelection += SubMenuMoveSelection;
            On.Celeste.TextMenuExt.OptionSubMenu.MoveSelection += OptionSubMenuMoveSelection;

            On.Celeste.FancyText.Text.Draw += FancyTextTextDraw;

            On.Celeste.Level.LoadLevel += LevelLoadLevel;
        }

        internal static void Unhook()
        {
            LogUtil.Log("Unhooking.");

            //On.Celeste.TextMenu.Item.Enter -= textMenuItemEnter;
            //On.Celeste.TextMenu.Item.ctor -= textMenuItemCtor;
            On.Celeste.TextMenu.MoveSelection -= TextMenuMoveSelection;
            On.Celeste.TextMenuExt.SubMenu.MoveSelection -= SubMenuMoveSelection;
            On.Celeste.TextMenuExt.OptionSubMenu.MoveSelection -= OptionSubMenuMoveSelection;

            On.Celeste.FancyText.Text.Draw -= FancyTextTextDraw;

            On.Celeste.Level.LoadLevel -= LevelLoadLevel;
        }

        private static Item TextMenuItemEnter(On.Celeste.TextMenu.Item.orig_Enter orig, Item self, Action enter)
        {
            return orig(self, () =>
            {
                UniversalSpeech.SpeechSay(self);
                enter();
            });
        }

        private static void TextMenuItemCtor(On.Celeste.TextMenu.Item.orig_ctor orig, Item self)
        {
            self.Enter(() => { });
            orig(self);
        }

        private static void TextMenuMoveSelection(On.Celeste.TextMenu.orig_MoveSelection orig, TextMenu self, int direction, bool wiggle = false)
        {
            orig(self, direction, wiggle);
            UniversalSpeech.SpeechSay(self.Current);
        }

        private static void SubMenuMoveSelection(On.Celeste.TextMenuExt.SubMenu.orig_MoveSelection orig, TextMenuExt.SubMenu self, int direction, bool wiggle = false)
        {
            orig(self, direction, wiggle);
            UniversalSpeech.SpeechSay(self.Current);
        }

        private static void OptionSubMenuMoveSelection(On.Celeste.TextMenuExt.OptionSubMenu.orig_MoveSelection orig, TextMenuExt.OptionSubMenu self, int direction, bool wiggle = false)
        {
            orig(self, direction, wiggle);
            UniversalSpeech.SpeechSay(self.Current);
        }

        private static FancyText.Text CurrentText = null;
        private static int CurrentTextStart = 0;
        private static int CurrentTextEnd = int.MaxValue;

        private static void FancyTextTextDraw(On.Celeste.FancyText.Text.orig_Draw orig, FancyText.Text self, Vector2 position, Vector2 justify, Vector2 scale, float alpha, int start = 0, int end = int.MaxValue)
        {
            if (CurrentText != self || CurrentTextStart != start || CurrentTextEnd != end)
            {
                UniversalSpeech.SpeechSay(self, start, end);
                CurrentText = self;
                CurrentTextStart = start;
                CurrentTextEnd = end;
            }
            orig(self, position, justify, scale, alpha, start, end);
        }

        private static void LevelLoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes playerIntro, bool isFromLoader)
        {
            orig(self, playerIntro, isFromLoader);
            if (!self.Entities.Any(entity => entity is AccessCamera))
            {
                self.Add(new AccessCamera());
                self.Entities.UpdateLists();
            }
        }
    }
}
