using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using NoMathExpectation.Celeste.Celestibility;
using System.Reflection;

namespace Celeste.Mod.Celestibility.Extensions
{
    internal static class HeartGemExtension
    {
        public static void Hook()
        {
            ModCollectRoutineHook = new ILHook(
                typeof(HeartGem).GetMethod("CollectRoutine", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget(),
                ModCollectRoutine
            );
        }

        public static void Unhook()
        {
            ModCollectRoutineHook.Dispose();
            ModCollectRoutineHook = null;
        }

        private static bool IsILInjectionPoint(Instruction inst)
        {
            if (!inst.MatchBrfalse(out _))
            {
                return false;
            }

            inst = inst.Next?.Next?.Next?.Next?.Next;
            if (inst is null || !inst.MatchCallvirt<HeartGem>("DoFakeRoutineWithBird"))
            {
                return false;
            }

            return true;
        }

        private static ILHook ModCollectRoutineHook = null;
        private static void ModCollectRoutine(ILContext context)
        {
            ILCursor cursor = new ILCursor(context);

            cursor.GotoNext(MoveType.Before, IsILInjectionPoint);
            cursor.Emit(OpCodes.Ldloc_1);
            cursor.EmitDelegate(HintCollected);
        }

        public static void HintCollected(this HeartGem gem)
        {
            if (gem is null)
            {
                return;
            }

            DynamicData data = DynamicData.For(gem);
            data.Get<Poem>("poem").SpeechSay();
        }
    }
}
