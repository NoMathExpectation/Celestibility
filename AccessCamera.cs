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
        public static bool Toggled => CelestibilityModule.Settings.CameraBind.Pressed;

        public static bool MoveToPlayerPressed => CelestibilityModule.Settings.CameraMoveToPlayer.Pressed;
        public static bool MoveLeft => CelestibilityModule.Settings.CameraMoveLeft.Pressed;
        public static bool MoveRight => CelestibilityModule.Settings.CameraMoveRight.Pressed;
        public static bool MoveUp => CelestibilityModule.Settings.CameraMoveUp.Pressed;
        public static bool MoveDown => CelestibilityModule.Settings.CameraMoveDown.Pressed;
        public static bool MovePrecise => CelestibilityModule.Settings.CameraMovePrecise.Check;
        public static bool NarrateEntity => CelestibilityModule.Settings.CameraNarrate.Pressed;
        public static bool CameraPosition => CelestibilityModule.Settings.CameraPosition.Pressed;
        public static bool PlayerPosition => CelestibilityModule.Settings.PlayerPosition.Pressed;

        public AccessCamera()
        {
            Collider = new Hitbox(1, 1);

            //Tag |= TagsExt.SubHUD;
            Tag |= Tags.Persistent;
            Tag |= Tags.TransitionUpdate;
            Tag |= Tags.FrozenUpdate;
            Tag |= Tags.PauseUpdate;

            Depth = -999999;
        }

        public void Toggle(bool? @bool = null)
        {
            if (@bool ?? !Enabled)
            {
                Visible = true;
                MoveToPlayer();
            }
            else
            {
                Visible = false;
            }
        }

        public void MoveToPlayer()
        {
            Player player = Scene.Tracker.GetEntity<Player>();
            if (player is null)
            {
                return;
            }

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
            UniversalSpeech.SpeechStop();
            if (!(CelestibilityModule.Settings.SpeechEnabled && Enabled))
            {
                return;
            }

            EntityList entities = Scene.Entities;
            foreach (Entity entity in entities)
            {
                if (entity.Visible && Collide.Check(this, entity))
                {
                    entity.SpeechSay();
                }
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Instance = this;
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Toggle(Enabled);
        }

        public override void Update()
        {
            base.Update();

            if (Toggled)
            {
                CelestibilityModuleSettings.ToggleCamera();
                return;
            }
            if (!Enabled)
            {
                return;
            }

            if (CameraPosition)
            {
                $"{X}, {Y}".SpeechSay(true);
            }

            if (PlayerPosition)
            {
                Player player = Scene.Tracker.GetEntity<Player>();
                if (player is not null)
                {
                    $"{player.X}, {player.Y}".SpeechSay(true);
                }
            }

            if (Toggled || MoveToPlayerPressed)
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

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Instance = null;
        }
    }
}
