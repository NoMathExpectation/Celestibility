using Celeste;
using FMOD.Studio;
using MonoMod.Utils;

namespace NoMathExpectation.Celeste.Celestibility.Extensions
{
    internal static class PlayerJumpAssist
    {
        public static bool Enabled => CelestibilityModule.Settings.JumpHeightAssist;

        private static EventInstance soundInstance = null;

        private static void StopSound()
        {
            soundInstance?.stop(STOP_MODE.IMMEDIATE);
            soundInstance = null;
        }

        private static void PlayerUpdate(On.Celeste.Player.orig_Update orig, Player self)
        {
            orig(self);

            if (soundInstance is null)
            {
                return;
            }

            var data = DynamicData.For(self);
            var varJumpTimer = data.Get<float>("varJumpTimer");
            if (!Enabled || varJumpTimer <= 0 || self.Dead)
            {
                StopSound();
                return;
            }

            float pitch = 1f + (0.2f - varJumpTimer) / 0.2f * 5f;
            soundInstance.setPitch(pitch);
        }

        private static void PlayerJump(On.Celeste.Player.orig_Jump orig, Player self, bool particles = true, bool playSfx = true)
        {
            orig(self, particles, playSfx);

            if (!Enabled || soundInstance is not null)
            {
                return;
            }

            var data = DynamicData.For(self);
            var varJumpTimer = data.Get<float>("varJumpTimer");
            if (varJumpTimer > 0)
            {
                soundInstance = Audio.Play("event:/Celestibility/jump");
                soundInstance.setVolume(2);
            }
        }

        public static void Hook()
        {
            On.Celeste.Player.Update += PlayerUpdate;
            On.Celeste.Player.Jump += PlayerJump;
        }

        public static void Unhook()
        {
            On.Celeste.Player.Update -= PlayerUpdate;
            On.Celeste.Player.Jump -= PlayerJump;
        }
    }
}
