using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using SeolMJ;

public class Card : MonoBehaviour
{

    [Header("Orientation")]
    public RectTransform thisRect;
    public Image image;
    public Sprite defaultImage;
    public CanvasGroup group;
    public SymbolAnimator[] symbols;

    [Header("Card")]
    public CardType type;
    public CardNum num;

    [HideInInspector] public bool picked;
    [HideInInspector] public bool done;

    private bool active;

    #region StateMachine

    public delegate void State();
    public State state;

    #endregion

    void OnValidate()
    {
        thisRect = GetComponent<RectTransform>();
    }

    void Update()
    {
        state?.Invoke();
    }

    void OnDisable()
    {
        if (active)
        {
            active = false;
            CardManager.instance.ReturnCard(this);
        }
    }

    public void Init(CardInfo card, CardInitMode mode = CardInitMode.Normal)
    {
        type = card.type;
        num = card.num;
        image.sprite = defaultImage;
        group.alpha = 1f;
        transform.localScale = Vector3.one;
        active = true;
        state = null;
        transform.localRotation = Quaternion.identity;
        SelectSymbol(mode);
    }

    public void SelectSymbol(CardInitMode mode)
    {
        if (!gameObject.activeInHierarchy)
        {
            Error("Init Canceled: GameObject not Active");
            return;
        }
        switch (num)
        {
            case CardNum.A: ActiveSymbols(16); break;
            case CardNum.Two: ActiveSymbols(5, 10); break;
            case CardNum.Three: ActiveSymbols(5, 8, 10); break;
            case CardNum.Four: ActiveSymbols(0, 4, 11, 15); break;
            case CardNum.Five: ActiveSymbols(0, 4, 8, 11, 15); break;
            case CardNum.Six: ActiveSymbols(0, 2, 4, 11, 13, 15); break;
            case CardNum.Seven: ActiveSymbols(0, 2, 4, 7, 11, 13, 15); break;
            case CardNum.Eight: ActiveSymbols(0, 1, 3, 4, 11, 12, 14, 15); break;
            case CardNum.Nine: ActiveSymbols(0, 1, 3, 4, 8, 11, 12, 14, 15); break;
            case CardNum.Ten: ActiveSymbols(0, 1, 3, 4, 6, 9, 11, 12, 14, 15); break;
            case CardNum.BlackJoker: ActiveSymbols(16); break;
            case CardNum.ColorJoker: ActiveSymbols(16); break;
            case CardNum.None: ActiveSymbols(); break;
            default: ActiveSymbols(0, 16, 15); break;
        };
        switch (mode)
        {
            case CardInitMode.Return: Return(new Vector2(Random.Range(-2000f, 2000f), Random.Range(-1000f, -2000f)));  break;
            case CardInitMode.Popup: ReturnAndKill(); break;
            case CardInitMode.Direct:
                killSpeed = CardManager.instance.cardFadeSpeed * 2f;
                state = KillState;
                break;
        }
    }

    void ActiveSymbols(params int[] indexs)
    {
        if (indexs == null || indexs.Length == 0)
        {
            for (int i = 0; i < 17; i++) symbols[i].gameObject.SetActive(false);
        }
        else
        {
            int count = indexs.Length;
            float delay = CardManager.instance.symbolDelay / count;
            List<int> index = new(indexs);
            for (int i = 0, j = 0; i < 17; i++)
            {
                int id = index.IndexOf(i);
                if (j < count && id != -1)
                {
                    symbols[i].Init(delay * j, GameManager.instance.GetSymbol(i == 16, type, num));
                    index.RemoveAt(id);
                    j++;
                }
                else
                {
                    symbols[i].gameObject.SetActive(false);
                }
            }
        }
    }

    public void Return(Vector2 velocity)
    {
        returnPos = CardManager.instance.previewPivot.anchoredPosition;
        returnVel = velocity;
        returnVelVel = Vector2.zero;
        state = ReturnState;
    }

    private Vector2 returnPos;
    private Vector2 returnVel, returnVelVel;

    private void ReturnState()
    {
        MoveTo(Utils.SpringDamp(thisRect.anchoredPosition, returnPos, ref returnVel, ref returnVelVel, 150f, 0.07f, GameManager.deltaTime));
        //MoveTo(Vector2.SmoothDamp(thisRect.anchoredPosition, returnPos, ref returnVel, 0.1f, Mathf.Infinity, GameManager.deltaTime));
    }

    public void Kill()
    {
        killSpeed = CardManager.instance.cardFadeSpeed;
        state = KillState;
    }

    private float killSpeed;
    private float killVelocity;

    private void KillState()
    {
        group.alpha = Mathf.SmoothDamp(group.alpha, 0f, ref killVelocity, killSpeed, Mathf.Infinity, GameManager.deltaTime);
        transform.localScale = Vector3.one * Mathf.Lerp(0.75f, 1f, group.alpha);
        if (group.alpha <= 0.01f)
        {
            state = null;
            active = false;
            CardManager.instance.ReturnCard(this);
        }
    }

    public void ReturnAndKill()
    {
        returnPos = CardManager.instance.previewPivot.anchoredPosition;
        killSpeed = CardManager.instance.cardFadeSpeed * 2f;
        state = ReturnAndKillState;
    }

    public void ReturnAndKillState()
    {
        MoveTo(Vector2.SmoothDamp(thisRect.anchoredPosition, returnPos, ref returnVel, 0.1f, Mathf.Infinity, GameManager.deltaTime));
        group.alpha = Mathf.SmoothDamp(group.alpha, 0f, ref killVelocity, killSpeed, Mathf.Infinity, GameManager.deltaTime);
        transform.localScale = Vector3.one * Mathf.Lerp(0.75f, 1f, group.alpha);
        if (group.alpha <= 0.01f)
        {
            state = null;
            active = false;
            CardManager.instance.ReturnCard(this);
        }
    }

    public void MoveTo(Vector2 position)
    {
        thisRect.anchoredPosition = position;
    }

    public void RotateTo(Quaternion rotation)
    {
        transform.localRotation = rotation;
    }

    public CardInfo GetInfo()
    {
        return new CardInfo(type, num);
    }

    #region Logging

    public static LogPreset? logPreset;

    public static void Log(string message)
    {
        logPreset ??= new("Card Item", GameManager.Resource.cardItemLogColor);
        Log4u.Log(logPreset.Value, message);
    }

    public static void Error(string message)
    {
        logPreset ??= new("Card Item", GameManager.Resource.cardItemLogColor);
        Log4u.Error(logPreset.Value, message);
    }

    #endregion

}

public enum CardInitMode
{
    Normal, Return, Popup, Direct
}