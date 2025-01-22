using Celeste;
using Microsoft.Xna.Framework;

namespace NoMathExpectation.Celeste.Celestibility.Extensions
{
    public static class PlayerColliderExtension
    {
        public static bool CheckWithoutAction(this PlayerCollider playerCollider, Player player)
        {
            var collider = playerCollider.Collider;
            if (playerCollider.FeatherCollider != null && player.StateMachine.State == Player.StStarFly)
            {
                collider = playerCollider.FeatherCollider;
            }

            if (collider == null)
            {
                if (player.CollideCheck(playerCollider.Entity))
                {
                    return true;
                }
                return false;
            }

            var origCollider = playerCollider.Entity.Collider;
            playerCollider.Entity.Collider = collider;
            bool collided = player.CollideCheck(playerCollider.Entity);
            playerCollider.Entity.Collider = origCollider;
            return collided;
        }

        public static bool CheckWithoutAction(this PlayerCollider playerCollider, Player player, Vector2 at)
        {
            var collider = playerCollider.Collider;
            if (playerCollider.FeatherCollider != null && player.StateMachine.State == Player.StStarFly)
            {
                collider = playerCollider.FeatherCollider;
            }

            if (collider == null)
            {
                if (player.CollideCheck(playerCollider.Entity, at))
                {
                    return true;
                }
                return false;
            }

            var origCollider = playerCollider.Entity.Collider;
            playerCollider.Entity.Collider = collider;
            bool collided = player.CollideCheck(playerCollider.Entity, at);
            playerCollider.Entity.Collider = origCollider;
            return collided;
        }
    }
}
