using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ChatManager : MonoBehaviour
{

    public static ChatManager instance;

    public RectTransform thisRect;

    [Header("Settings")]
    public float activeY;
    public float inactiveY;
    public float speedY;
    public float spacing;

    [Header("Debug")]
    public float curY;

    private List<ChatElement> elements;
    private Coroutine coroutine;

    void Awake()
    {
        elements = new();
        instance = this;
    }

    void Start()
    {
        
    }

    IEnumerator DoUpdate()
    {
        float vel = 0;
        while (thisRect.anchoredPosition.y != curY)
        {
            thisRect.anchoredPosition = new Vector2(thisRect.anchoredPosition.x, Mathf.SmoothDamp(thisRect.anchoredPosition.y, curY, ref vel, speedY));
            yield return null;
        }
        yield break;
    }

    public void Create()
    {

    }

    public void Mode(bool active)
    {
        curY = active ? activeY : inactiveY;
        if (coroutine == null) coroutine = StartCoroutine(DoUpdate());
    }

    public static void Layout()
    {
        int count = instance.transform.childCount;
        float totalY = 0;
        while (count > 8)
        {
#if UNITY_EDITOR
            DestroyImmediate(instance.transform.GetChild(1).gameObject);
#else
            Destroy(transform.GetChild(1).gameObject);
#endif
            count--;
        }
        instance.ApplyElements();
        for (int i = count - 1, t = 0; i > 0; i--, t++)
        {
            Transform child = instance.transform.GetChild(i);
            RectTransform rect = child.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, totalY);
            totalY += rect.sizeDelta.y + instance.spacing;
        }
    }

    public void ApplyElements()
    {
        foreach (ChatElement element in elements)
        {
            element.Apply();
        }
    }

    public static void AddElement(ChatElement element)
    {
        if (!instance.elements.Contains(element)) instance.elements.Add(element);
    }

    public static void RemoveElement(ChatElement element)
    {
        if (instance.elements.Contains(element)) instance.elements.Remove(element);
    }

}
