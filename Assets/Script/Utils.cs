using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using System.Threading;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SeolMJ
{
    public static class Utils
    {

        public static float Vec2Deg(Vector2 vector)
            => Mathf.Atan2(vector.y, vector.x) * Mathf.Rad2Deg;

        public static bool IsZero(this Vector2 vector)
            => vector.x == 0 && vector.y == 0;

        public static float AbsDeltaAngle(float one, float two)
            => Mathf.Abs(Mathf.DeltaAngle(one, two));

        public static bool IsInt(this Vector2 vector)
            => vector == Vector2Int.RoundToInt(vector);

        public static Vector2 Clamp(this Vector2 vector)
            => vector.normalized * Mathf.Clamp(vector.magnitude, 0, 1);

        public static void EditAlpha(this Image image, float alpha)
        {
            image.color = new Color(image.color.r, image.color.g, image.color.b, alpha);
        }

        public static async void TimeOut(Action action, TimeSpan timeout, Action onTimeOut = null, Action onSuccess = null)
        {
            Task task = Task.Run(action);
            using (var cancel = new CancellationTokenSource())
            {
                Task completed = await Task.WhenAny(task, Task.Delay(timeout, cancel.Token));
                if (completed == task)
                {
                    if (completed.IsFaulted) throw completed.Exception;
                    cancel.Cancel();
                    onSuccess?.Invoke();
                }
                else
                {
                    onTimeOut?.Invoke();
                }
            }
        }

    }
}