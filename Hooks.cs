using Celeste;
using System;
using static Celeste.TextMenu;

namespace NoMathExpectation.Celeste.Celestibility
{
    internal class Hooks
    {
        internal static void hook()
        {
            LogUtil.log("Hooking.");

            //On.Celeste.TextMenu.Item.Enter += textMenuItemEnter;
            //On.Celeste.TextMenu.Item.ctor += textMenuItemCtor;
            On.Celeste.TextMenu.MoveSelection += textMenuMoveSelection;
            On.Celeste.TextMenuExt.SubMenu.MoveSelection += subMenuMoveSelection;
            On.Celeste.TextMenuExt.OptionSubMenu.MoveSelection += optionSubMenuMoveSelection;
        }

        internal static void unhook()
        {
            LogUtil.log("Unhooking.");

            //On.Celeste.TextMenu.Item.Enter -= textMenuItemEnter;
            //On.Celeste.TextMenu.Item.ctor -= textMenuItemCtor;
            On.Celeste.TextMenu.MoveSelection -= textMenuMoveSelection;
            On.Celeste.TextMenuExt.SubMenu.MoveSelection -= subMenuMoveSelection;
            On.Celeste.TextMenuExt.OptionSubMenu.MoveSelection -= optionSubMenuMoveSelection;
        }

        internal static Item textMenuItemEnter(On.Celeste.TextMenu.Item.orig_Enter orig, Item self, Action enter)
        {
            return orig(self, () =>
            {
                UniversalSpeech.speechSay(self);
                enter();
            });
        }

        internal static void textMenuItemCtor(On.Celeste.TextMenu.Item.orig_ctor orig, Item self)
        {
            self.Enter(() => { });
            orig(self);
        }

        internal static void textMenuMoveSelection(On.Celeste.TextMenu.orig_MoveSelection orig, TextMenu self, int direction, bool wiggle = false)
        {
            orig(self, direction, wiggle);
            UniversalSpeech.speechSay(self.Current);
        }

        internal static void subMenuMoveSelection(On.Celeste.TextMenuExt.SubMenu.orig_MoveSelection orig, TextMenuExt.SubMenu self, int direction, bool wiggle = false)
        {
            orig(self, direction, wiggle);
            UniversalSpeech.speechSay(self.Current);
        }

        internal static void optionSubMenuMoveSelection(On.Celeste.TextMenuExt.OptionSubMenu.orig_MoveSelection orig, TextMenuExt.OptionSubMenu self, int direction, bool wiggle = false)
        {
            orig(self, direction, wiggle);
            UniversalSpeech.speechSay(self.Current);
        }
    }
}
