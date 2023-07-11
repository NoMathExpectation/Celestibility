using NoMathExpectation.Celeste.Celestibility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.Celestibility.Extensions
{
    internal static class TalkComponentExtension
    {
        public static void HintInteract() {
            if (!UniversalSpeech.Enabled) {
                return;
            }

            string.Format(Dialog.Get("Celestibility_interact_hint"), Input.Talk.SpeechRender()).SpeechSay();
        }
    }
}
