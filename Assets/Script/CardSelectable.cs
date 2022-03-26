using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem.UI;

using TMPro;
using SeolMJ;

[SelectionBase]
public abstract class CardSelectable : UIBehaviour, IMoveHandler, IEventSystemHandler, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler, IUpdateSelectedHandler
{

    [Header("Reference")]
    public RectTransform targetRect;
    public Graphic targetGraphic;
    public TMP_Text targetText;

    [Header("Settings")]
    public Direction direction = Direction.Down;
    public bool interactable = true;
    public bool selectable = false;
    public bool immediate = false;

    [Header("Navigation")]
    public Navigation navigation = new Navigation() { mode = Navigation.Mode.Automatic };
    public ExplicitNavigation explicitNavigation;

    [HideInInspector]
    public bool isPointerInside, isPointerDown, isSelected;

    // Private Variables
    private bool isUpdating;
    private Color targetColor;
    private Color targetTextColor;
    private Vector2 targetOffset;
    private Vector2 offset;
    private float moveSpeed;
    private float progress;

    // Static Variables
    public static ColorBlock color;
    public static ColorBlock textColor;
    public static List<CardSelectable> cardSelectables = new(64);

    protected override void Reset()
    {
        targetRect = GetComponent<RectTransform>();
        targetGraphic = GetComponent<Graphic>();
    }

    protected override void Awake()
    {
        color = GameManager.Resource.selectableColor;
        textColor = GameManager.Resource.selectableTextColor;

        targetColor = color.normalColor;
        targetTextColor = textColor.normalColor;

        if (targetGraphic is Image image)
        {
            image.alphaHitTestMinimumThreshold = 0f;
        }
    }

    protected override void OnEnable()
    {
        cardSelectables.Add(this);

        targetColor = color.normalColor;
        targetTextColor = textColor.normalColor;
        if (GameManager.instance.eventSystem && GameManager.instance.eventSystem.currentSelectedGameObject == gameObject && selectable)
        {
            isSelected = true;
            targetColor = color.selectedColor;
            targetTextColor = color.selectedColor;
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

        targetRect.anchoredPosition -= offset;
        offset = Vector2.zero;
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

        float deltaSpeed = Time.unscaledDeltaTime * color.fadeDuration;

        targetGraphic.color = Color.Lerp(targetGraphic.color, targetColor, deltaSpeed);
        if (targetText) targetText.color = Color.Lerp(targetText.color, targetTextColor, deltaSpeed);

        targetRect.anchoredPosition -= offset;
        offset = Vector2.Lerp(offset, targetOffset, deltaSpeed * moveSpeed);
        targetRect.anchoredPosition += offset;

        progress = Mathf.Lerp(progress, 1f, deltaSpeed);

        if (progress > 0.99f) isUpdating = false;
    }

    // Private Methods

    public void ApplyColor(ColorType type, bool immediate = false)
    {
        switch (type)
        {
            case ColorType.Normal:
                targetColor = color.normalColor;
                targetTextColor = textColor.normalColor;
                break;
            case ColorType.Highlighted:
                targetColor = color.highlightedColor;
                targetTextColor = textColor.highlightedColor;
                break;
            case ColorType.Pressed:
                targetColor = color.pressedColor;
                targetTextColor = textColor.pressedColor;
                break;
            case ColorType.Selected:
                targetColor = color.selectedColor;
                targetTextColor = textColor.selectedColor;
                break;
        }
        if (immediate)
        {
            targetGraphic.color = targetColor;
            if (targetText) targetText.color = targetTextColor;
        }
        progress = 0f;
        isUpdating = true;
    }

    public void ApplyOffset(float amount, float speed)
    {
        amount *= 10f;
        moveSpeed = speed;
        targetOffset = direction switch
        {
            Direction.Up => new(0, amount),
            Direction.Down => new(0, -amount),
            Direction.Left => new(-amount, 0),
            Direction.Right => new(amount, 0),
            _ => Vector2.zero
        };
        progress = 0f;
        isUpdating = true;
    }

    public void ApplyOffsetImmediately(float amount)
    {
        amount *= 10f;
        targetRect.anchoredPosition -= offset;
        offset = direction switch
        {
            Direction.Up => new(0, amount),
            Direction.Down => new(0, -amount),
            Direction.Left => new(-amount, 0),
            Direction.Right => new(amount, 0),
            _ => Vector2.zero
        };
        targetRect.anchoredPosition += offset;
        progress = 0f;
        isUpdating = true;
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

            ApplyColor(ColorType.Pressed, true);
            ApplyOffset(1f, 2f);
            if (immediate) OnPress(eventData);
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
                ApplyColor(ColorType.Selected);
                ApplyOffset(0.5f, 1f);
                if (!immediate) OnPress(eventData);
            }
            else if (isPointerInside)
            {
                ApplyColor(ColorType.Highlighted);
                ApplyOffset(-0.25f, 1f);
                if (!immediate) OnPress(eventData);
            }
            else
            {
                ApplyColor(ColorType.Normal);
                ApplyOffset(0f, 1f);
            }

            isPointerDown = false;
        }
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        if (!interactable) return;
        if (eventData != null && !(eventData.pointerEnter == null) && !(eventData.pointerEnter.GetComponentInParent<CardSelectable>() != this))
        {
            if (!isPointerDown && !isSelected)
            {
                ApplyColor(ColorType.Highlighted, true);
                ApplyOffset(-0.25f, 2f);
            }

            isPointerInside = true;
        }
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        if (!interactable) return;
        if (eventData != null && !(eventData.pointerEnter == null) && !(eventData.pointerEnter.GetComponentInParent<CardSelectable>() != this))
        {
            if (!isPointerDown && !isSelected)
            {
                ApplyColor(ColorType.Normal);
                ApplyOffset(0f, 1f);
            }

            isPointerInside = false;
        }
    }

    public virtual void OnSelect(BaseEventData eventData)
    {
        if (!interactable) return;

        ApplyColor(ColorType.Highlighted, true);
        ApplyOffset(-0.25f, 2f);

        if (!isPointerDown) isPointerInside = true;

        if (selectable)
        {
            isSelected = true;
        }
    }

    public virtual void OnDeselect(BaseEventData eventData)
    {
        if (!interactable) return;

        ApplyColor(ColorType.Normal);
        ApplyOffset(0f, 1f);

        isPointerInside = false;

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

    public void OnUpdateSelected(BaseEventData eventData)
    {
        if (eventData.used || !EventSystem.current.sendNavigationEvents) return;

        if (IsActive() && interactable && eventData.currentInputModule is InputSystemUIInputModule module)
        {
            var submitAction = module.submit.action ?? null;

            if (submitAction != null)
            {
                if (submitAction.WasPressedThisFrame())
                {
                    isPointerDown = true;

                    ApplyColor(ColorType.Pressed, true);
                    ApplyOffset(1f, 2f);
                    if (immediate) OnPress(eventData);
                }
                else if (submitAction.WasReleasedThisFrame())
                {
                    isPointerDown = false;

                    if (selectable && isPointerInside)
                    {
                        isSelected = true;
                        ApplyColor(ColorType.Selected);
                        ApplyOffset(0.5f, 1f);
                        if (!immediate) OnPress(eventData);
                    }
                    else if (isPointerInside)
                    {
                        ApplyColor(ColorType.Highlighted);
                        ApplyOffset(-0.25f, 1f);
                        if (!immediate) OnPress(eventData);
                    }
                    else
                    {
                        ApplyColor(ColorType.Normal);
                        ApplyOffset(0f, 1f);
                    }
                }
            }
        }
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

    [System.Serializable]
    public enum ColorType
    {
        Normal, Highlighted, Pressed, Selected
    }

}
