using Celeste;
using Celeste.Mod;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace NoMathExpectation.Celeste.Celestibility.Entities
{
    [Tracked(true)]
    [CustomEntity("Celestibility/AccessCamera")]
    internal class AccessCamera : Entity
    {
        public static AccessCamera Instance;
        public static bool Enabled => CelestibilityModule.Settings.Camera;

        public static bool MoveToPlayerPressed => CelestibilityModule.Settings.CameraMoveToPlayer.Pressed;
        public static bool MoveLeft => CelestibilityModule.Settings.CameraMoveLeft.Pressed;
        public static bool MoveRight => CelestibilityModule.Settings.CameraMoveRight.Pressed;
        public static bool MoveUp => CelestibilityModule.Settings.CameraMoveUp.Pressed;
        public static bool MoveDown => CelestibilityModule.Settings.CameraMoveDown.Pressed;
        public static bool MovePrecise => CelestibilityModule.Settings.CameraMovePrecise.Check;
        public static bool NarrateEntity => CelestibilityModule.Settings.CameraNarrate.Pressed;

        public AccessCamera()
        {
            Collider = new Hitbox(1, 1);
            Instance = this;
        }

        public void MoveToPlayer()
        {
            Player player = Scene.Tracker.GetEntity<Player>();
            X = player.X;
            Y = player.Y - 4;
            Narrate();
        }

        public void Move()
        {
            int dx = 0;
            int dy = 0;

            if (MoveLeft)
            {
                dx -= 1;
            }
            if (MoveRight)
            {
                dx += 1;
            }
            if (MoveUp)
            {
                dy -= 1;
            }
            if (MoveDown)
            {
                dy += 1;
            }

            if (!MovePrecise)
            {
                dx *= 8;
                dy *= 8;
            }

            X += dx;
            Y += dy;

            if (NarrateEntity || dx != 0 || dy != 0)
            {
                Narrate();
            }
        }

        public void Narrate()
        {
            LogUtil.Log($"Access camera at {X}, {Y}", LogLevel.Verbose);
            if (!CelestibilityModule.Settings.SpeechEnabled)
            {
                return;
            }

            EntityList entities = Scene.Entities;
            foreach (Entity entity in entities)
            {
                if (entity.Visible && Collide.Check(this, entity))
                {
                    UniversalSpeech.SpeechSay(entity);
                }
            }
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            MoveToPlayer();
        }

        public override void Update()
        {
            base.Update();

            if (MoveToPlayerPressed)
            {
                MoveToPlayer();
                return;
            }

            Move();
        }

        public override void Render()
        {
            base.Render();
            Draw.Point(Position, Color.Red);
            Draw.HollowRect(X - 2, Y - 2, 5, 5, Color.Red);
        }
    }
}
