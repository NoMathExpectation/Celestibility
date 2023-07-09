using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using NoMathExpectation.Celeste.Celestibility.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static Celeste.TextMenu;

namespace NoMathExpectation.Celeste.Celestibility
{
    internal class Hooks
    {
        internal static void Hook()
        {
            LogUtil.Log("Hooking.");

            On.Celeste.TextMenu.Update += TextMenuUpdate;
            On.Celeste.TextMenu.MoveSelection += TextMenuMoveSelection;
            On.Celeste.TextMenuExt.SubMenu.MoveSelection += SubMenuMoveSelection;
            On.Celeste.TextMenuExt.OptionSubMenu.MoveSelection += OptionSubMenuMoveSelection;
            ModTextMenuUpdateHook = new ILHook(typeof(TextMenu).GetMethod("orig_Update"), ModTextMenuUpdate<TextMenu>);
            IL.Celeste.TextMenuExt.SubMenu.Update += ModTextMenuUpdate<TextMenuExt.SubMenu>;
            IL.Celeste.TextMenuExt.OptionSubMenu.Update += ModTextMenuUpdate<TextMenuExt.OptionSubMenu>;

            On.Celeste.FancyText.Text.Draw += FancyTextTextDraw;

            On.Celeste.Level.LoadLevel += LevelLoadLevel;

            On.Celeste.OuiMainMenu.Enter += OuiMainMenuEnter;
            ModMenuButtonSetSelectedHook = new ILHook(typeof(MenuButton).GetMethod("set_Selected"), ModMenuButtonSetSelected);

            On.Celeste.OuiFileSelect.Update += OuiFileSelectUpdate;
            On.Celeste.OuiFileSelectSlot.Select += OuiFileSelectSlotSelect;
            On.Celeste.OuiFileSelectSlot.OnDeleteSelected += OuiFileSelectSlotOnDeleteSelected;
            ModOuiFileSelectSlotOrigUpdateHook = new ILHook(typeof(OuiFileSelectSlot).GetMethod("orig_Update"), ModOuiFileSelectSlotOrigUpdate);

            On.Celeste.SaveLoadIcon.Routine += SaveLoadIconRoutine;

            On.Celeste.OuiAssistMode.Enter += OuiAssistModeEnter;
            ModOuiAssistModeInputRoutineHook = new ILHook(typeof(OuiAssistMode).GetMethod("InputRoutine", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetStateMachineTarget(), ModOuiAssistModeInputRoutine);

            On.Celeste.BirdTutorialGui.Update += BirdTutorialGuiUpdate;
        }

        internal static void Unhook()
        {
            LogUtil.Log("Unhooking.");

            On.Celeste.TextMenu.Update -= TextMenuUpdate;
            On.Celeste.TextMenu.MoveSelection -= TextMenuMoveSelection;
            On.Celeste.TextMenuExt.SubMenu.MoveSelection -= SubMenuMoveSelection;
            On.Celeste.TextMenuExt.OptionSubMenu.MoveSelection -= OptionSubMenuMoveSelection;
            ModTextMenuUpdateHook.Dispose();
            ModTextMenuUpdateHook = null;
            IL.Celeste.TextMenuExt.SubMenu.Update -= ModTextMenuUpdate<TextMenuExt.SubMenu>;
            IL.Celeste.TextMenuExt.OptionSubMenu.Update -= ModTextMenuUpdate<TextMenuExt.OptionSubMenu>;

            On.Celeste.FancyText.Text.Draw -= FancyTextTextDraw;

            On.Celeste.Level.LoadLevel -= LevelLoadLevel;

            On.Celeste.OuiMainMenu.Enter -= OuiMainMenuEnter;
            ModMenuButtonSetSelectedHook.Dispose();
            ModMenuButtonSetSelectedHook = null;

            On.Celeste.OuiFileSelect.Update -= OuiFileSelectUpdate;
            On.Celeste.OuiFileSelectSlot.Select -= OuiFileSelectSlotSelect;
            On.Celeste.OuiFileSelectSlot.OnDeleteSelected -= OuiFileSelectSlotOnDeleteSelected;
            ModOuiFileSelectSlotOrigUpdateHook.Dispose();
            ModOuiFileSelectSlotOrigUpdateHook = null;

            On.Celeste.SaveLoadIcon.Routine -= SaveLoadIconRoutine;

            On.Celeste.OuiAssistMode.Enter -= OuiAssistModeEnter;
            ModOuiAssistModeInputRoutineHook.Dispose();
            ModOuiAssistModeInputRoutineHook = null;

            On.Celeste.BirdTutorialGui.Update -= BirdTutorialGuiUpdate;
        }


        private static void TextMenuUpdate(On.Celeste.TextMenu.orig_Update orig, TextMenu self)
        {
            orig(self);

            DynamicData data = DynamicData.For(self);

            if (data.TryGet<bool?>("origFocused", out var origFocused))
            {
                if (origFocused != self.Focused && self.Focused)
                {
                    self.Current.SpeechSay();
                }
            }

            data.Set("origFocused", self.Focused);
        }

        private static void TextMenuMoveSelection(On.Celeste.TextMenu.orig_MoveSelection orig, TextMenu self, int direction, bool wiggle = false)
        {
            orig(self, direction, wiggle);
            self.Current.SpeechSay();
        }

        private static void SubMenuMoveSelection(On.Celeste.TextMenuExt.SubMenu.orig_MoveSelection orig, TextMenuExt.SubMenu self, int direction, bool wiggle = false)
        {
            orig(self, direction, wiggle);
            self.Current.SpeechSay();
        }

        private static void OptionSubMenuMoveSelection(On.Celeste.TextMenuExt.OptionSubMenu.orig_MoveSelection orig, TextMenuExt.OptionSubMenu self, int direction, bool wiggle = false)
        {
            orig(self, direction, wiggle);
            self.Current.SpeechSay();
        }

        private static ILHook ModTextMenuUpdateHook = null;
        private static void ModTextMenuUpdate<T>(ILContext context)
        {
            ILCursor cursor = new ILCursor(context);
            while (cursor.TryGotoNext(MoveType.After, inst => inst.MatchCallvirt<Item>("LeftPressed") || inst.MatchCallvirt<Item>("RightPressed")))
            {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit<T>(OpCodes.Callvirt, "get_Current");
                cursor.EmitReference(true);
                cursor.EmitDelegate<Action<Item, bool>>(UniversalSpeech.SpeechSay);
            }
        }

        private static FancyText.Text CurrentText = null;
        private static int CurrentTextStart = 0;
        private static int CurrentTextEnd = int.MaxValue;

        private static void FancyTextTextDraw(On.Celeste.FancyText.Text.orig_Draw orig, FancyText.Text self, Vector2 position, Vector2 justify, Vector2 scale, float alpha, int start = 0, int end = int.MaxValue)
        {
            if (CurrentText != self || CurrentTextStart != start || CurrentTextEnd != end)
            {
                self.SpeechSay(start, end);
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


        private static IEnumerator OuiMainMenuEnter(On.Celeste.OuiMainMenu.orig_Enter orig, OuiMainMenu self, Oui from)
        {
            yield return new SwapImmediately(orig(self, from));
            MenuButton.GetSelection(self.Scene).SpeechSay();
        }

        private static ILHook ModMenuButtonSetSelectedHook = null;
        private static void ModMenuButtonSetSelected(ILContext context)
        {
            ILCursor cursor = new ILCursor(context);

            while (cursor.TryGotoNext(MoveType.After, inst => inst.MatchCallvirt<MenuButton>("OnSelect")))
            {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Action<MenuButton>>(UniversalSpeech.SpeechSay);
            }
        }


        private static int fileSelectSlotIndex = 0;
        private static bool fileSelectFocused = false;
        private static void OuiFileSelectUpdate(On.Celeste.OuiFileSelect.orig_Update orig, OuiFileSelect self)
        {
            orig(self);
            if ((fileSelectFocused != self.Focused && self.Focused == true) || fileSelectSlotIndex != self.SlotIndex)
            {
                self.Slots[self.SlotIndex].SpeechSay();
            }
            fileSelectSlotIndex = self.SlotIndex;
            fileSelectFocused = self.Focused;
        }

        private static void OuiFileSelectSlotSelect(On.Celeste.OuiFileSelectSlot.orig_Select orig, OuiFileSelectSlot self, bool resetButtonIndex)
        {
            orig(self, resetButtonIndex);
            DynamicData data = DynamicData.For(self);
            List<OuiFileSelectSlot.Button> buttons = data.Get<List<OuiFileSelectSlot.Button>>("buttons");
            int index = data.Get<int>("buttonIndex");
            buttons[index].SpeechSay();
        }

        private static void OuiFileSelectSlotOnDeleteSelected(On.Celeste.OuiFileSelectSlot.orig_OnDeleteSelected orig, OuiFileSelectSlot self)
        {
            orig(self);
            "FILE_DELETE_REALLY".SpeechSay(true);
            "FILE_DELETE_NO".SpeechSay();
        }

        private static ILHook ModOuiFileSelectSlotOrigUpdateHook = null;
        private static void ModOuiFileSelectSlotOrigUpdate(ILContext context)
        {
            ILCursor cursor = new ILCursor(context);

            cursor.GotoNext(MoveType.After, inst => inst.MatchStfld<OuiFileSelectSlot>("deleteIndex"));
            cursor.EmitReference("FILE_DELETE_YES");
            cursor.EmitReference(true);
            cursor.EmitReference<string>(null);
            cursor.EmitReference(false);
            cursor.EmitDelegate<Func<string, bool, string, bool, int>>(UniversalSpeech.SpeechSay);
            cursor.Emit(OpCodes.Pop);

            cursor.GotoNext(MoveType.After, inst => inst.MatchStfld<OuiFileSelectSlot>("deleteIndex"));
            cursor.EmitReference("FILE_DELETE_NO");
            cursor.EmitReference(true);
            cursor.EmitReference<string>(null);
            cursor.EmitReference(false);
            cursor.EmitDelegate<Func<string, bool, string, bool, int>>(UniversalSpeech.SpeechSay);
            cursor.Emit(OpCodes.Pop);

            cursor.GotoNext(MoveType.After, inst => inst.MatchStfld<OuiFileSelectSlot>("buttonIndex"));
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit<OuiFileSelectSlot>(OpCodes.Ldfld, "buttons");
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit<OuiFileSelectSlot>(OpCodes.Ldfld, "buttonIndex");
            cursor.Emit(OpCodes.Callvirt, typeof(List<OuiFileSelectSlot.Button>).GetMethod("get_Item"));
            cursor.EmitDelegate<Action<OuiFileSelectSlot.Button>>(UniversalSpeech.SpeechSay);

            cursor.GotoNext(MoveType.After, inst => inst.MatchStfld<OuiFileSelectSlot>("buttonIndex"));
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit<OuiFileSelectSlot>(OpCodes.Ldfld, "buttons");
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit<OuiFileSelectSlot>(OpCodes.Ldfld, "buttonIndex");
            cursor.Emit(OpCodes.Callvirt, typeof(List<OuiFileSelectSlot.Button>).GetMethod("get_Item"));
            cursor.EmitDelegate<Action<OuiFileSelectSlot.Button>>(UniversalSpeech.SpeechSay);
        }


        private static IEnumerator SaveLoadIconRoutine(On.Celeste.SaveLoadIcon.orig_Routine orig, SaveLoadIcon self)
        {
            "Celestibility_saving".DialogClean().SpeechSay();
            yield return new SwapImmediately(orig(self));
            "Celestibility_saved".DialogClean().SpeechSay();
        }


        private static IEnumerator OuiAssistModeEnter(On.Celeste.OuiAssistMode.orig_Enter orig, OuiAssistMode self, Oui from)
        {
            yield return new SwapImmediately(orig(self, from));
            self.SpeechSay();
        }

        private static bool IsModOuiAssistModeInputRoutineHookPoint(Instruction inst)
        {
            if (!inst.MatchBeq(out _))
            {
                return false;
            }

            inst = inst.Previous;
            if (inst is null || !inst.MatchLdfld<OuiAssistMode>("pageIndex"))
            {
                return false;
            }

            inst = inst.Previous;
            if (inst is null || !inst.MatchLdloc(1))
            {
                return false;
            }

            inst = inst.Previous;
            if (inst is null || !inst.MatchLdfld(out _))
            {
                return false;
            }

            inst = inst.Previous;
            if (inst is null || !inst.MatchLdarg(0))
            {
                return false;
            }

            return true;
        }

        private static ILHook ModOuiAssistModeInputRoutineHook = null;
        private static void ModOuiAssistModeInputRoutine(ILContext context)
        {
            ILCursor cursor = new ILCursor(context);

            cursor.GotoNext(MoveType.After, IsModOuiAssistModeInputRoutineHookPoint);
            cursor.Emit(OpCodes.Ldloc_1);
            cursor.Emit(OpCodes.Ldloc_1);
            cursor.Emit<OuiAssistMode>(OpCodes.Ldfld, "pageIndex");
            cursor.Emit(OpCodes.Ldloc_1);
            cursor.Emit<OuiAssistMode>(OpCodes.Ldfld, "questionIndex");
            cursor.EmitDelegate<Action<OuiAssistMode, int, int>>(UniversalSpeech.SpeechSay);

            while (cursor.TryGotoNext(MoveType.After, inst => inst.MatchStfld<OuiAssistMode>("questionIndex")))
            {
                cursor.Emit(OpCodes.Ldloc_1);
                cursor.Emit<OuiAssistMode>(OpCodes.Ldfld, "questionIndex");
                cursor.EmitDelegate(UniversalSpeech.SpeechSayAssistUpdate);
            }
        }


        private static void BirdTutorialGuiUpdate(On.Celeste.BirdTutorialGui.orig_Update orig, BirdTutorialGui self)
        {
            orig(self);

            DynamicData data = DynamicData.For(self);
            bool? origOpen = data.Get<bool?>("origOpen");
            if ((CelestibilityModule.Settings.Tutorial.Pressed || origOpen != self.Open) && self.Open)
            {
                self.SpeechSay();
            }

            data.Set("origOpen", self.Open);
        }
    }
}
