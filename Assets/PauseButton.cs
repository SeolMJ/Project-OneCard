using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class PauseButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{

    [Header("References")]
    public RectTransform thisRect;
    public PauseManager manager;

    [Header("Settings")]
    public UnityEvent onClick;

    void Reset()
    {
        if (!thisRect) thisRect = GetComponent<RectTransform>();
        if (!manager) manager = GetComponentInParent<PauseManager>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        manager.Click(onClick);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        thisRect.anchoredPosition = new Vector2(thisRect.anchoredPosition.x, manager.positionY.y);
        manager.selectedButton = this;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (manager.selectedButton == this) manager.selectedButton = null;
    }

}
