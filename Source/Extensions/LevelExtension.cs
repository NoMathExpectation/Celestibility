using Celeste;
using Microsoft.Xna.Framework;

namespace NoMathExpectation.Celeste.Celestibility.Extensions
{
    public static class LevelExtension
    {
        public static Vector2 GetPlayerBasedSoundPosition(this Level level, Vector2 position, float scale = 1.0f)
        {
            var cameraCenter = level.Camera.Position + new Vector2(320f, 180f) / 2f;
            var displacement = position - level.Tracker.GetEntity<Player>().Position;
            displacement *= scale;
            return cameraCenter + displacement;
        }
    }
}
