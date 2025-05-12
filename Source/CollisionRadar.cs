using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using NoMathExpectation.Celeste.Celestibility.Entities;
using NoMathExpectation.Celeste.Celestibility.Extensions;
using System.Linq;

namespace NoMathExpectation.Celeste.Celestibility
{
    internal static class CollisionRadar
    {
        public static bool ToggleRadarPressed => CelestibilityModule.Settings.RadarToggle.Pressed;
        public static bool IncreaseRadarDistance => CelestibilityModule.Settings.RadarIncreaseDistance.Pressed;
        public static bool DecreaseRadarDistance => CelestibilityModule.Settings.RadarDecreaseDistance.Pressed;

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

            //add detectors
            scene.Add(new CollisionDetector(self, CollisionDetector.PlayerLeftCollisionDetectorName, new Vector2(-1, 0), 50));
            scene.Add(new CollisionDetector(self, CollisionDetector.PlayerRightCollisionDetectorName, new Vector2(1, 0), 100));
            scene.Add(new CollisionDetector(self, CollisionDetector.PlayerUpCollisionDetectorName, new Vector2(0, -1), 150));
            scene.Add(new CollisionDetector(self, CollisionDetector.PlayerDownCollisionDetectorName, new Vector2(0, 1), 200));
        }


        public static void ToggleRadar(bool? @bool = null)
        {
            bool enabled = @bool ?? !CelestibilityModule.Settings.RadarEnabled;
            CelestibilityModule.Settings.RadarEnabled = enabled;
            string s = enabled ? "on" : "off";
            string log = $"Celestibility_radar_{s}".DialogClean();
            LogUtil.Log(log);
            if (Engine.Commands.Open)
            {
                Engine.Commands.Log(log);
            }
            log.SpeechSay();
        }

        [Command("radar", "(Celestibility) Toggle radar.")]
        public static void ToggleRadar()
        {
            ToggleRadar(null);
        }

        [Command("radar_distance", "(Celestibility) Toggle radar distance.")]
        public static void SetRadarDistance(int distance)
        {
            if (distance < 1)
            {
                if (Engine.Commands.Open)
                {
                    Engine.Commands.Log($"Cannot set distance for less than 1.");
                }
                return;
            }

            CelestibilityModule.Settings.RadarMaxDistance = distance;
            CelestibilityModule.Settings.RadarMaxDistance.SpeechSay(true);

            if (Engine.Commands.Open)
            {
                Engine.Commands.Log($"Set radar distance to {distance}.");
            }
        }

        private static void PlayerUpdate(On.Celeste.Player.orig_Update orig, Player self)
        {
            orig(self);

            //deprecated
            //CheckCollision(self);

            //check keys
            if (ToggleRadarPressed)
            {
                ToggleRadar();
            }

            if (IncreaseRadarDistance)
            {
                if (CelestibilityModule.Settings.RadarMaxDistance < 128)
                {
                    CelestibilityModule.Settings.RadarMaxDistance++;
                }
                CelestibilityModule.Settings.RadarMaxDistance.SpeechSay(true);
            }

            if (DecreaseRadarDistance)
            {
                if (CelestibilityModule.Settings.RadarMaxDistance > 1)
                {
                    CelestibilityModule.Settings.RadarMaxDistance--;
                }
                CelestibilityModule.Settings.RadarMaxDistance.SpeechSay(true);
            }

            CheckGround(self);
        }

        public static bool ReadGround => CelestibilityModule.Settings.ReadGround;

        private static void CheckGround(this Player player)
        {
            const string previousGroundClassNameKey = "previousGroundClassName";

            var dynamicData = DynamicData.For(player);
            if (!ReadGround)
            {
                dynamicData.Set(previousGroundClassNameKey, null);
                return;
            }

            var previousGroundClassName = dynamicData.Get<string>(previousGroundClassNameKey);

            var collideGround = player.CollideFirst<Platform>(player.Position + Vector2.UnitY) ?? player.CollideFirstOutside<Platform>(player.Position + Vector2.UnitY);
            //LogUtil.Log($"Ground: {collideGround}", LogLevel.Debug);
            var groundClassName = collideGround?.GetType()?.Name;
            if (groundClassName == previousGroundClassName)
            {
                return;
            }

            LogUtil.Log($"Ground changed: {groundClassName}", LogLevel.Debug);
            dynamicData.Set(previousGroundClassNameKey, groundClassName);
            if (collideGround is null)
            {
                "Celestibility_read_ground_empty".SpeechSay(true);
                return;
            }
            collideGround.SpeechSay(true);
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
