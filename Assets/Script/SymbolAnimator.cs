using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SymbolAnimator : MonoBehaviour, ITweenable
{

    [Header("Resources")]
    public RectTransform thisRect;
    public Image image;

    #region Tween
    public Tween.Action TweenAction { get => action; set => action = value; }
    private Tween.Action action;
    private float progress;
    #endregion

    void Reset()
    {
        image = GetComponent<Image>();
        thisRect = GetComponent<RectTransform>();
    }

    public void Init(Sprite sprite)
    {
        gameObject.SetActive(true);
        image.sprite = sprite;
        transform.localScale = Vector3.zero;
    }

    void OnDisable()
    {
        action = null;
    }

    public void OnTween(float progress)
    {
        if (this.progress < 0f) return;
        this.progress = progress;
    }

    public void StartAnimationDelay()
    {
        progress += GameManager.deltaTime;
        if (progress >= 0f) action = StartAnimation;
    }

    void StartAnimation()
    {
        if (progress < 1f)
        {
            progress += GameManager.deltaTime;
            float eval = Mathf.LerpUnclamped(0f, 1f, Tween.instance.sticky.Evaluate(progress));
            transform.localScale = new Vector3(eval, eval, 1f);
            return;
        }
        transform.localScale = Vector3.one;
        action = null;
    }

    public void SelectDiamondDelay()
    {
        progress += GameManager.deltaTime;
        if (progress >= 0f) action = SelectDiamond;
    }

    void SelectDiamond()
    {
        if (progress < 1f)
        {
            progress += GameManager.deltaTime;
            float eval = Mathf.LerpUnclamped(0f, 1f, Tween.instance.sa_Diamond.Evaluate(progress));
            transform.localScale = new Vector3(Mathf.LerpUnclamped(1.2f, 1f, eval), Mathf.LerpUnclamped(0.7f, 1f, eval), 1f);
            thisRect.anchoredPosition = new Vector2(0f, Mathf.LerpUnclamped(-32f, 0f, eval));
            return;
        }
        transform.localScale = Vector3.one;
        thisRect.anchoredPosition = Vector2.zero;
        action = null;
    }

    public void SelectSpadeDelay()
    {
        progress += GameManager.deltaTime;
        if (progress >= 0f) action = SelectSpade;
    }

    public void SelectSpade()
    {
        if (progress < 1f)
        {
            progress += GameManager.deltaTime;
            float eval = Mathf.LerpUnclamped(0f, 1f, Tween.instance.sa_Spade.Evaluate(progress));
            transform.localScale = new Vector3(Mathf.LerpUnclamped(0.7f, 1f, eval), Mathf.LerpUnclamped(1.2f, 1f, eval), 1f);
            thisRect.anchoredPosition = new Vector2(0f, Mathf.LerpUnclamped(32f, 0f, eval));
            return;
        }
        transform.localScale = Vector3.one;
        thisRect.anchoredPosition = Vector2.zero;
        action = null;
    }

}
