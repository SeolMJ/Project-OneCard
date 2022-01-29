using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class PauseButton : MonoBehaviour, IPointerClickHandler
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
        
    }

}
