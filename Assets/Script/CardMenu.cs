using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

using SeolMJ;

public class CardMenu : MonoBehaviour
{

    [Header("References")]
    public RectTransform canvas;
    public RectTransform parent;
    public RectTransform select;
    public RectTransform finish;

    [Header("Elements")]
    public RectTransform[] cards;
    public UnityEvent[] events;

    [Header("Settings")]
    public AnimationCurve cardHeightCurve;
    public float delay = 0.2f;
    public float speed = 150f;
    public float damp = 0.07f;

    private Vector2 mousePos;
    private Vector2 velocity, velvel;
    private RectTransform selected;
    private bool pressed;
    private bool finished;

    void Start()
    {
        
    }

    void Update()
    {
        if (finished)
        {
            for (int i = 0; i < cards.Length; i++)
            {
                if (cards[i] == selected)
                {
                    selected.anchoredPosition = Utils.SpringDamp(selected.anchoredPosition, finish.anchoredPosition, ref velocity, ref velvel, speed, damp, Time.unscaledDeltaTime);
                    selected.localRotation = Quaternion.Lerp(selected.localRotation, Quaternion.identity, Time.unscaledDeltaTime * 10f);
                    continue;
                }

                cards[i].anchoredPosition = Vector2.Lerp(cards[i].anchoredPosition, new Vector2(0f, -1000f), Time.unscaledDeltaTime * 10f);
            }
            return;
        }

        Camera camera = Camera.main;
        if (!camera) return;

        mousePos = new Vector2((Input.mousePosition.x / Screen.width - 0.5f) * canvas.sizeDelta.x, (Input.mousePosition.y / Screen.height - 0.5f) * canvas.sizeDelta.y);

        if (selected && Input.GetMouseButtonDown(0))
        {
            pressed = true;
            select.gameObject.SetActive(false);
        }

        if (pressed && Input.GetMouseButtonUp(0))
        {
            pressed = false;
            if (Vector2.Distance(mousePos, finish.anchoredPosition) <= 200f || Vector2.Distance(selected.anchoredPosition, finish.anchoredPosition) <= 200f)
            {
                finished = true;
                for(int i = 0; i < cards.Length; i++)
                {
                    if (cards[i] == selected)
                    {
                        DelayedInvoke(i);
                        break;
                    }
                }
                return;
            }
            else
            {
                select.gameObject.SetActive(true);
            }
        }

        if (pressed)
        {
            Vector2 position = Vector2.Lerp(selected.anchoredPosition, mousePos, Time.unscaledDeltaTime * 10f);
            velocity = (position - selected.anchoredPosition) / Time.unscaledDeltaTime;
            selected.anchoredPosition = position;
            selected.localRotation = Quaternion.Lerp(selected.localRotation, Quaternion.identity, Time.unscaledDeltaTime * 10f);
            if (Vector2.Distance(selected.anchoredPosition, finish.anchoredPosition) <= 150f)
            {
                select.position = finish.position;
                select.rotation = finish.rotation;
                if (!select.gameObject.activeSelf) select.gameObject.SetActive(true);
            }
            else if (select.gameObject.activeSelf) select.gameObject.SetActive(false);
        }

        Move();
    }

    void Move()
    {
        int count = cards.Length;
        float canvasWidth = canvas.sizeDelta.x;
        float width = count * 400f + (count - 1) * 25f;
        float halfWidth = width / 2f;
        float bottom = -canvas.sizeDelta.y / 2f;
        float deltaTime = Time.unscaledDeltaTime * 10f;
        float mouseHeight = (Input.mousePosition.y / Screen.height * 4f - 0.25f) * (pressed ? 0.1f : 1f);
        float mul = 1f;
        float mousePosHalf = mousePos.y + 540f;
        if (count < 4 && count > 1) mul /= count switch
        {
            2 => 5 - count,
            3 => 4.5f - count,
            _ => 1f
        };
        float minDist = Mathf.Infinity;
        int closest = 0;
        for (int i = 0; i < count; i++)
        {
            if (pressed && cards[i] == selected) continue;
            float index = count == 1 ? 0f : ((float)i / (count - 1) - 0.5f) * mul;
            float pos = 200f + i * 425f - mousePos.x / canvasWidth * width;
            float offset = (pos - halfWidth - mousePos.x) / canvasWidth;
            float posX = Mathf.Abs(cards[i].anchoredPosition.x - mousePos.x);
            float perlinPlusIndex = index * 10f + Time.unscaledTime * 0.2f;
            float perlinMinusIndex = index * 10f - Time.unscaledTime * 0.2f;
            if (posX < minDist)
            {
                minDist = posX;
                closest = i;
            }
            cards[i].anchoredPosition = Vector2.LerpUnclamped(cards[i].anchoredPosition
                , Vector2.Lerp(new Vector2(960f * index, bottom + index * index * -240f)
                , new Vector2(pos - halfWidth
                , mousePosHalf * cardHeightCurve.Evaluate(mousePosHalf / 1080f) + (1 - Mathf.Abs(offset)) * 320f - 700f)
                , mouseHeight) + new Vector2(Mathf.PerlinNoise(perlinPlusIndex, perlinMinusIndex) * 32f
                , Mathf.PerlinNoise(perlinMinusIndex, perlinPlusIndex) * 32f)
                , deltaTime);
            cards[i].localRotation = Quaternion.LerpUnclamped(cards[i].localRotation
                , Quaternion.Euler(0, 0, Mathf.Lerp(index * -30f, offset * -10f, mouseHeight)), deltaTime);
        }
        if (pressed) return;
        selected = cards[closest];
        select.SetPositionAndRotation(selected.position, selected.rotation);
        if (selected.GetSiblingIndex() != parent.childCount - 2) selected.SetSiblingIndex(parent.childCount - 2);
    }

    void DelayedInvoke(int index)
    {
        StartCoroutine(DoDelayedInvoke(index));
    }

    IEnumerator DoDelayedInvoke(int index)
    {
        yield return new WaitForSecondsRealtime(delay);
        events[index].Invoke();
    }

}
