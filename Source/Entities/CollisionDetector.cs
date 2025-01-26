using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using NoMathExpectation.Celeste.Celestibility.Extensions;
using System.Linq;

namespace NoMathExpectation.Celeste.Celestibility.Entities
{
    [Tracked(true)]
    [CustomEntity("Celestibility/CollisionDetector")]
    internal class CollisionDetector : Entity
    {
        public static bool Enabled => CelestibilityModule.Settings.RadarEnabled;

        private readonly string name;
        public const string PlayerLeftCollisionDetectorName = "leftCollisionDetector";
        public const string PlayerRightCollisionDetectorName = "rightCollisionDetector";
        public const string PlayerUpCollisionDetectorName = "upCollisionDetector";
        public const string PlayerDownCollisionDetectorName = "downCollisionDetector";

        public Vector2 Direction;
        public static float MaxDistance => CelestibilityModule.Settings.RadarMaxDistance;
        public Vector2 Displacement = Vector2.Zero;

        private Player player;

        public float Pitch;

        public CollisionDetector(Player player, string name, Vector2 direction, float pitch = 100)
        {
            Visible = false;
            Tag |= Tags.FrozenUpdate | Tags.TransitionUpdate | Tags.Persistent;

            this.player = player;
            this.name = name;
            Direction = direction;
            Pitch = pitch;
            Reset();
        }

        public void Reset()
        {
            //LogUtil.Log($"{name} distance: {Displacement.Length()}", LogLevel.Verbose);
            Position = player.Position;
            Displacement = Vector2.Zero;
            Collider = player.Collider.Clone();
        }

        private float playCooldown = 0;

        public void PlaySound()
        {
            if (!Enabled)
            {
                return;
            }

            if (playCooldown > 0)
            {
                return;
            }

            if (Direction.Y > 0 && !player.Dead && player.OnGround())
            {
                return;
            }

            Audio.Play("event:/Celestibility/rect_wave", Position, "pitch", Pitch);
            playCooldown = 0.125f;
        }

        public override void Awake(Scene scene)
        {
            player = Scene.Tracker.GetEntity<Player>();
            if (player is null)
            {
                base.Awake(scene);
                return;
            }

            DynamicData playerData = DynamicData.For(player);
            playerData.Set(name, this);
            base.Awake(scene);
        }

        public override void Removed(Scene scene)
        {
            if (player is null)
            {
                base.Removed(scene);
                return;
            }

            DynamicData playerData = DynamicData.For(player);
            playerData.Set(name, null);
            base.Removed(scene);
        }

        public override void Update()
        {
            base.Update();

            if (player.Dead)
            {
                RemoveSelf();
                return;
            }

            playCooldown -= Engine.DeltaTime;

            Displacement += Direction;
            Position = player.Position + Displacement;
            if (Displacement.Length() > MaxDistance)
            {
                Reset();
                return;
            }

            Collider = player.Collider.Clone();

            bool checkPlatform = Direction.Y > 0 && CollideCheck<Platform>();
            bool checkSoild = CollideCheck<Solid>();
            bool checkPlayerColliders = Scene.Tracker.GetComponents<PlayerCollider>().Cast<PlayerCollider>().Any((component) => component.CheckWithoutAction(player, Position));
            if (checkPlatform || checkSoild || checkPlayerColliders)
            {
                PlaySound();
                Reset();
                return;
            }
        }
    }
}
