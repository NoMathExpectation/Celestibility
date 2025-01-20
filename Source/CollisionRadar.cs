using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Monocle;

namespace NoMathExpectation.Celeste.Celestibility
{
    internal static class CollisionRadar
    {
        public static void CheckCollision(this Player player)
        {
            CheckCollisionLeft(player);
        }

        public static void CheckCollisionLeft(this Player player)
        {
            int maxDistance = 64;
            int distance = maxDistance;
            for (int i = 0; i < maxDistance; i++)
            {
                Vector2 pos = player.Position + new Vector2(-i, 0);
                bool broken = false;
                foreach (var entity in player.Scene.Entities)
                {
                    if (entity != player && Collide.Check(player, entity, pos))
                    {
                        distance = i;
                        broken = true;
                        break;
                    }
                }
                if (broken)
                {
                    break;
                }
            }

            LogUtil.Log($"Left distance: {distance}", LogLevel.Verbose);
        }

        private static void PlayerUpdate(On.Celeste.Player.orig_Update orig, Player self)
        {
            orig(self);
            //CheckCollision(self);
        }

        internal static void Hook()
        {
            On.Celeste.Player.Update += PlayerUpdate;
        }

        internal static void Unhook()
        {
            On.Celeste.Player.Update -= PlayerUpdate;
        }
    }
}
