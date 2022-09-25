using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tween : MonoBehaviour
{

    public static Tween instance;

    public delegate void Action();
    private static List<ITweenable> updates;
    public static int count;

    [Header("Graphs")]
    public AnimationCurve sticky;
    public AnimationCurve sa_Diamond;
    public AnimationCurve sa_Spade;
    public AnimationCurve pa_Pop;

    [Header("Debug")]
    public int currentCount;

    void Awake()
    {
        instance = this;
        updates = new List<ITweenable>(100);
    }

    void Update()
    {
#if UNITY_EDITOR
        currentCount = count;
#endif
        for (int i = 0; i < count; ++i)
        {
            if (updates[i] == null)
            {
                count--;
                updates.RemoveAt(i);
                i--;
                continue;
            }
            Action action = updates[i].TweenAction;
            if (action == null)
            {
                count--;
                updates.RemoveAt(i);
                i--;
                continue;
            }
            action.Invoke();
            if (action == null)
            {
                count--;
                updates.RemoveAt(i);
                i--;
                continue;
            }
        }
    }

    public static void Run(ITweenable tweenable, Action method, float delay = 0f)
    {
        bool running = tweenable.TweenAction != null;
        tweenable.TweenAction = method;
        tweenable.OnTween(-delay);
        if (running) return;
        updates.Add(tweenable);
        count++;
    }

}

public interface ITweenable
{
    public Tween.Action TweenAction { get; set; }
    public void OnTween(float progress);
}