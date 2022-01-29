using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SeolMJ
{
    public static class VectorUtils
    {

        #region SmoothDamp
        public static void SmoothDamp(ref this Vector3 current, Vector3 to, ref Vector3 velocity, float time)
        {
            current = Vector3.SmoothDamp(current, to, ref velocity, time);
        }

        public static void SmoothDamp(ref this Vector2 current, Vector2 to, ref Vector2 velocity, float time)
        {
            current = Vector2.SmoothDamp(current, to, ref velocity, time);
        }

        public static void SmoothDamp(ref this float current, float target, ref float velocity, float time)
        {
            current = Mathf.SmoothDamp(current, target, ref velocity, time);
        }
        #endregion

    }
}