using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorManager : MonoBehaviour
{

    public static CursorManager instance;

    [Header("Textures")]
    public Texture2D[] textures;

    [Header("Settings")]
    public Vector2 hotspot;

    private static bool hover, active;

    void Awake()
    {
        instance = this;
    }

    public static void Hover(bool on)
    {
        hover = on;
    }

    public static void Active(bool on)
    {
        active = on;
    }

    public static void Refresh()
    {
        if (hover)
        {
            if (active)
            {

            }
            else
            {

            }
        }
        else
        {

        }
    }

    public static void Change(CursorType type)
    {
        Cursor.SetCursor(instance.textures[(int)type], instance.hotspot, CursorMode.Auto);
    }

}

public enum CursorType
{
    Normal = 0,
    Hover = 1,
    Active = 2
}