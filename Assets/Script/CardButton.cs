using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

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

#if UNITY_EDITOR
    [MenuItem("GameObject/Card UI/Button", false)]
    public static void CreateDefault(MenuCommand menuCommand)
    {
        GameObject obj = new GameObject("Button", typeof(CanvasRenderer), typeof(Image), typeof(CardButton));
        Image image = obj.GetComponent<Image>();
        image.sprite = GameManager.EditorResource.selectableDefaultSprite;
        image.pixelsPerUnitMultiplier = 1.25f;
        GameObjectUtility.SetParentAndAlign(obj, menuCommand.context as GameObject);
        Undo.RegisterCreatedObjectUndo(obj, "Create " + obj.name);
        Selection.activeObject = obj;
    }
#endif

}
