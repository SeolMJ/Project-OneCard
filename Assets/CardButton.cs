using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardButton : CardSelectable
{

    [Header("Event")]
    public UnityEvent onClick;

    public override void OnPress(BaseEventData eventData)
    {
        if (IsActive() && interactable)
        {
            UISystemProfilerApi.AddMarker("Button.onClick", this);
            onClick.Invoke();
        }
    }

}
