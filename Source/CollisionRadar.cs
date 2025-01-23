using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Monocle;
using NoMathExpectation.Celeste.Celestibility.Entities;
using NoMathExpectation.Celeste.Celestibility.Extensions;
using System.Linq;

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
            int distance;

            var soilds = player.Scene.Tracker.GetEntities<Solid>().Cast<Solid>().ToList();
            var playerColliderComponents = player.Scene.Tracker.GetComponents<PlayerCollider>().Cast<PlayerCollider>().ToList();

            if (player.Scene.OnInterval(1))
            {
                LogUtil.Log($"Soilds: {soilds.Count}", LogLevel.Verbose);
                LogUtil.Log($"PlayerColliderComponents: {playerColliderComponents.Count}", LogLevel.Verbose);
            }

            for (distance = 0; distance < maxDistance; distance++)
            {
                Vector2 pos = player.Position + new Vector2(-distance, 0);

                if (Collide.Check(player, soilds, pos))
                {
                    break;
                }

                if (playerColliderComponents.Any((component) => component.CheckWithoutAction(player)))
                {
                    break;
                }
            }

            if (player.Scene.OnInterval(1))
            {
                LogUtil.Log($"Left distance: {distance}", LogLevel.Verbose);
            }
        }

        private static void PlayerAdded(On.Celeste.Player.orig_Added orig, Player self, Scene scene)
        {
            orig(self, scene);
            scene.Add(new CollisionDetector(self, CollisionDetector.PlayerLeftCollisionDetectorName, new Vector2(-1, 0), 64));
        }

        private static void PlayerUpdate(On.Celeste.Player.orig_Update orig, Player self)
        {
            orig(self);
            //CheckCollision(self);
        }

        internal static void Hook()
        {
            On.Celeste.Player.Added += PlayerAdded;
            On.Celeste.Player.Update += PlayerUpdate;
        }

        internal static void Unhook()
        {
            On.Celeste.Player.Added -= PlayerAdded;
            On.Celeste.Player.Update -= PlayerUpdate;
        }
    }
}
