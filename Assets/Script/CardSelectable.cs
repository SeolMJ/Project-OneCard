using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using SeolMJ;

[SelectionBase]
public abstract class CardSelectable : UIBehaviour, IMoveHandler, IEventSystemHandler, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler, ISubmitHandler
{

    [Header("Reference")]
    public RectTransform targetRect;
    public Graphic targetGraphic;

    [Header("Settings")]
    public bool interactable = true;
    public bool selectable = false;

    [Header("Navigation")]
    public Navigation navigation;
    public ExplicitNavigation explicitNavigation;

    [HideInInspector]
    public bool isPointerInside, isPointerDown, isSelected;

    // Private Variables
    private bool isUpdating;
    private Color currentColor;

    // Static Variables
    public static ColorBlock color;
    public static List<CardSelectable> cardSelectables = new(64);

    protected override void Reset()
    {
        targetRect = GetComponent<RectTransform>();
        targetGraphic = GetComponent<Graphic>();
    }

    protected override void Awake()
    {
        color = GameManager.Resource.selectableColor;
    }

    protected override void OnEnable()
    {
        cardSelectables.Add(this);

        currentColor = color.normalColor;
        if (GameManager.instance.eventSystem && GameManager.instance.eventSystem.currentSelectedGameObject == gameObject && selectable)
        {
            isSelected = true;
            currentColor = color.selectedColor;
        }

        isPointerDown = false;
        isUpdating = true;
    }

    protected override void OnDisable()
    {
        int index = cardSelectables.IndexOf(this);
        if (index != -1) cardSelectables.RemoveAt(index);

        isPointerInside = false;
        isPointerDown = false;
        isSelected = false;
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus && isPointerDown)
        {
            isPointerInside = false;
            isPointerDown = false;
            isSelected = false;
        }
    }

    void Update()
    {
        if (!isUpdating) return;

        targetGraphic.color = Color.Lerp(targetGraphic.color, currentColor, Time.unscaledDeltaTime * color.fadeDuration);
    }

    // Private Methods

    public void ApplyColor()
    {
        targetGraphic.color = currentColor;
    }

    // Events

    public virtual void OnMove(AxisEventData eventData)
    {
        switch (eventData.moveDir)
        {
            case MoveDirection.Right:
                Navigate(eventData, FindSelectableOnRight());
                break;
            case MoveDirection.Up:
                Navigate(eventData, FindSelectableOnUp());
                break;
            case MoveDirection.Left:
                Navigate(eventData, FindSelectableOnLeft());
                break;
            case MoveDirection.Down:
                Navigate(eventData, FindSelectableOnDown());
                break;
        }
    }

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        if (!interactable) return;
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (navigation.mode != 0 && GameManager.instance.eventSystem)
            {
                GameManager.instance.eventSystem.SetSelectedGameObject(gameObject, eventData);
            }

            isPointerDown = true;

            currentColor = color.pressedColor;
            ApplyColor();
            OnPress(eventData);
        }
    }

    public virtual void OnPointerUp(PointerEventData eventData)
    {
        if (!interactable) return;
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (selectable && isPointerInside)
            {
                isSelected = true;
                currentColor = color.selectedColor;
            }
            else currentColor = isPointerInside ? color.highlightedColor : color.normalColor;

            isPointerDown = false;
        }
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        if (!interactable) return;
        if (eventData != null && !(eventData.pointerEnter == null) && !(eventData.pointerEnter.GetComponentInParent<CardSelectable>() != this))
        {
            if (!isPointerDown)
            {
                currentColor = color.highlightedColor;
                ApplyColor();
            }

            isPointerInside = true;
        }
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        if (!interactable) return;
        if (eventData != null && !(eventData.pointerEnter == null) && !(eventData.pointerEnter.GetComponentInParent<CardSelectable>() != this))
        {
            if (!isPointerDown && !isSelected) currentColor = color.normalColor;

            isPointerInside = false;
        }
    }

    public virtual void OnSelect(BaseEventData eventData)
    {
        if (!interactable) return;

        currentColor = color.highlightedColor;
        ApplyColor();

        if (selectable)
        {
            isSelected = true;
        }
    }

    public virtual void OnDeselect(BaseEventData eventData)
    {
        if (!interactable) return;

        currentColor = color.normalColor;

        isSelected = false;
    }

    public virtual void Select()
    {
        if (!interactable) return;
        if (GameManager.instance.eventSystem != null && !GameManager.instance.eventSystem.alreadySelecting)
        {
            GameManager.instance.eventSystem.SetSelectedGameObject(gameObject);
        }
    }

    public virtual void OnSubmit(BaseEventData eventData)
    {
        if (!interactable) return;

        currentColor = color.pressedColor;
        ApplyColor();
        currentColor = isSelected ? color.selectedColor : color.highlightedColor;

        OnPress(eventData);
    }

    public abstract void OnPress(BaseEventData eventData);

    // Public Methods

    public void Navigate(AxisEventData eventData, CardSelectable sel)
    {
        if (sel != null && sel.IsActive())
        {
            eventData.selectedObject = sel.gameObject;
        }
    }

    public CardSelectable FindSelectableOnRight()
    {
        if (navigation.mode == Navigation.Mode.Explicit)
        {
            return explicitNavigation.right;
        }
        return FindSelectable(Vector3.right);
    }

    public CardSelectable FindSelectableOnUp()
    {
        if (navigation.mode == Navigation.Mode.Explicit)
        {
            return explicitNavigation.up;
        }
        return FindSelectable(Vector3.up);
    }

    public CardSelectable FindSelectableOnLeft()
    {
        if (navigation.mode == Navigation.Mode.Explicit)
        {
            return explicitNavigation.left;
        }
        return FindSelectable(Vector3.left);
    }

    public CardSelectable FindSelectableOnDown()
    {
        if (navigation.mode == Navigation.Mode.Explicit)
        {
            return explicitNavigation.down;
        }
        return FindSelectable(Vector3.down);
    }

    public CardSelectable FindSelectable(Vector3 dir)
    {
        dir = dir.normalized;
        Vector3 v = Quaternion.Inverse(transform.rotation) * dir;
        Vector3 b = targetRect.TransformPoint(GetPointOnRectEdge(targetRect, v));
        float num = float.NegativeInfinity;
        float num2 = float.NegativeInfinity;
        float num3;
        bool flag = navigation.wrapAround && (navigation.mode == Navigation.Mode.Vertical || navigation.mode == Navigation.Mode.Horizontal);
        CardSelectable selectable = null;
        CardSelectable result = null;
        for (int i = 0; i < cardSelectables.Count; i++)
        {
            CardSelectable selectable2 = cardSelectables[i];
            if (selectable2 == this || !selectable2.interactable || selectable2.navigation.mode == Navigation.Mode.None || (Camera.current != null && !StageUtility.IsGameObjectRenderedByCamera(selectable2.gameObject, Camera.current)))
            {
                continue;
            }

            RectTransform rectTransform = selectable2.transform as RectTransform;
            Vector3 position = (rectTransform != null) ? rectTransform.rect.center : Vector3.zero;
            Vector3 rhs = selectable2.transform.TransformPoint(position) - b;
            float num4 = Vector3.Dot(dir, rhs);
            if (flag && num4 < 0f)
            {
                num3 = (0f - num4) * rhs.sqrMagnitude;
                if (num3 > num2)
                {
                    num2 = num3;
                    result = selectable2;
                }
            }
            else if (!(num4 <= 0f))
            {
                num3 = num4 / rhs.sqrMagnitude;
                if (num3 > num)
                {
                    num = num3;
                    selectable = selectable2;
                }
            }
        }

        if (flag && null == selectable)
        {
            return result;
        }

        return selectable;
    }

    // Static Methods

    private static Vector3 GetPointOnRectEdge(RectTransform rect, Vector2 dir)
    {
        if (rect == null)
        {
            return Vector3.zero;
        }

        if (dir != Vector2.zero)
        {
            dir /= Mathf.Max(Mathf.Abs(dir.x), Mathf.Abs(dir.y));
        }

        dir = rect.rect.center + Vector2.Scale(rect.rect.size, dir * 0.5f);
        return dir;
    }

    // Structs

    [System.Serializable]
    public struct ExplicitNavigation
    {
        public CardSelectable up;
        public CardSelectable down;
        public CardSelectable left;
        public CardSelectable right;
    }

}
