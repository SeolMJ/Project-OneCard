using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SeolMJ.CardUtils;

public class AttackCard : MonoBehaviour
{

    public RectTransform thisRect;
    public CanvasGroup thisGroup;
    public Vector2 endSize;

    private bool running = false;

    void OnEnable()
    {
        thisRect.sizeDelta = Vector2.zero;
        //MoveTo(new Vector2(cardManager.attackCardParent.sizeDelta.x / -2f - 64f, cardManager.attackCardParent.sizeDelta.y / -2f - 64f));
        if (running) StopAllCoroutines();
        StartCoroutine(StartAnimation());
    }

    IEnumerator StartAnimation()
    {
        running = true;
        while (endSize.x - thisRect.sizeDelta.x > 0.01f)
        {
            thisRect.sizeDelta = Vector3.Lerp(thisRect.sizeDelta, endSize, Time.deltaTime * 10f);
            yield return new WaitForEndOfFrame();
        }
        thisRect.sizeDelta = endSize;
        running = false;
    }

    public void MoveTo(Vector2 position)
    {
        thisRect.anchoredPosition = position;
    }

}
