namespace JoonaUtils
{
    using UnityEngine;

    public static class Convenience
    {
        /// <summary>
        /// Returns direction vector pointing from one point to another
        /// </summary>
        /// <param name="start">start point</param>
        /// <param name="end">target point</param>
        /// <returns>Direction as Vector2. Not normalized.</returns>
        public static Vector2 Direction2D(Vector2 start, Vector2 end)
        {
            return (end - start).normalized;
        }
    }
}