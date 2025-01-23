using Celeste;
using Celeste.Mod;
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
        private readonly string name;
        public const string PlayerLeftCollisionDetectorName = "leftCollisionDetector";
        public const string PlayerRightCollisionDetectorName = "rightCollisionDetector";
        public const string PlayerUpCollisionDetectorName = "upCollisionDetector";
        public const string PlayerDownCollisionDetectorName = "downCollisionDetector";

        public Vector2 Direction;
        public float MaxDistance;
        public float DistanceTravelled { get; private set; } = 0;

        private Player player;

        public CollisionDetector(Player player, string name, Vector2 direction, float maxDistance = 64)
        {
            this.player = player;
            this.name = name;
            Direction = direction;
            MaxDistance = maxDistance;
            Reset();
        }

        public void Reset()
        {
            LogUtil.Log($"{name} distance: {DistanceTravelled}", LogLevel.Verbose);
            Position = player.Position;
            DistanceTravelled = 0;
            Collider = player.Collider.Clone();
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

            if (player is null)
            {
                RemoveSelf();
                return;
            }

            Position += Direction;
            DistanceTravelled += Direction.Length();
            if (DistanceTravelled > MaxDistance)
            {
                Reset();
                return;
            }

            Collider = player.Collider.Clone();

            if (CollideCheck<Solid>() || Scene.Tracker.GetComponents<PlayerCollider>().Cast<PlayerCollider>().Any((component) => component.CheckWithoutAction(player, Position)))
            {
                Audio.Play("event:/Celestibility/rect_wave", Position);
                Reset();
                return;
            }
        }
    }
}
