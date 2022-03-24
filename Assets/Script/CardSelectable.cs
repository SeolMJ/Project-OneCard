using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using SeolMJ;
using UnityEngine.InputSystem.UI;

[SelectionBase]
public abstract class CardSelectable : UIBehaviour, IMoveHandler, IEventSystemHandler, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler, IUpdateSelectedHandler
{

    [Header("Reference")]
    public RectTransform targetRect;
    public Graphic targetGraphic;

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
    private Vector2 targetOffset;
    private Vector2 offset;
    private float moveSpeed;

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

        targetColor = color.normalColor;
        if (GameManager.instance.eventSystem && GameManager.instance.eventSystem.currentSelectedGameObject == gameObject && selectable)
        {
            isSelected = true;
            targetColor = color.selectedColor;
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

        targetRect.anchoredPosition -= offset;
        offset = Vector2.Lerp(offset, targetOffset, deltaSpeed * moveSpeed);
        targetRect.anchoredPosition += offset;
    }

    // Private Methods

    public void ApplyColor()
    {
        targetGraphic.color = targetColor;
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

            targetColor = color.pressedColor;
            ApplyColor();
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
                targetColor = color.selectedColor;
                ApplyOffset(0.5f, 1f);
                if (!immediate) OnPress(eventData);
            }
            else if (isPointerInside)
            {
                targetColor = color.highlightedColor;
                ApplyOffset(-0.25f, 1f);
                if (!immediate) OnPress(eventData);
            }
            else
            {
                targetColor = color.normalColor;
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
                targetColor = color.highlightedColor;
                ApplyColor();
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
                targetColor = color.normalColor;
                ApplyOffset(0f, 1f);
            }

            isPointerInside = false;
        }
    }

    public virtual void OnSelect(BaseEventData eventData)
    {
        if (!interactable) return;

        targetColor = color.highlightedColor;
        ApplyColor();
        ApplyOffset(-0.25f, 2f);

        if (selectable)
        {
            isSelected = true;
        }
    }

    public virtual void OnDeselect(BaseEventData eventData)
    {
        if (!interactable) return;

        targetColor = color.normalColor;
        ApplyOffset(0f, 1f);

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

        if (eventData.used || !EventSystem.current.sendNavigationEvents)
        {

            return;
        }

        //new input system handles submit on release, we can never animate the button down
        //so, we manually check the submit action and handle ourself

        if (IsActive() && interactable && eventData.currentInputModule is InputSystemUIInputModule module)
        {

            var submitAction = module.submit?.action;

            if (submitAction != null)
            {
                if (submitAction.WasPressedThisFrame())
                {
                    targetColor = color.pressedColor;
                    ApplyColor();
                    ApplyOffset(1f, 2f);
                    if (immediate) OnPress(eventData);
                }
                else if (submitAction.WasReleasedThisFrame())
                {
                    if (selectable && isPointerInside)
                    {
                        isSelected = true;
                        targetColor = color.selectedColor;
                        ApplyOffset(0.5f, 1f);
                        if (!immediate) OnPress(eventData);
                    }
                    else if (isPointerInside)
                    {
                        targetColor = color.highlightedColor;
                        ApplyOffset(-0.25f, 1f);
                        if (!immediate) OnPress(eventData);
                    }
                    else
                    {
                        targetColor = color.normalColor;
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

}
