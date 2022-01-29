using System.Collections;
using UnityEngine;

public abstract class Card : MonoBehaviour
{

    [Header("Orientation")]
    public RectTransform thisRect;

    [Header("Card")]
    public CardType type;
    public CardNum num;

    [HideInInspector] public bool picked;
    [HideInInspector] public bool done;

    protected Coroutine activeJob;
    protected Coroutine activeMove;
    protected readonly WaitForEndOfFrame waitForEnd = new ();
    protected bool destroying;

    private void OnValidate()
    {
        thisRect = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        if (destroying) Destroy(gameObject);
    }

    public void Init(CardInfo card, bool home = false)
    {
        type = card.type;
        num = card.num;
        SelectSymbol(home);
    }

    public abstract void SelectSymbol(bool home);

    public void GoHome()
    {
        if (this == null) return;
        if (!gameObject.activeInHierarchy) return;
        if (activeMove != null) StopCoroutine(activeMove);
        activeMove = StartCoroutine(DoGoHome());
    }

    IEnumerator DoGoHome()
    {
        Vector2 homePos = CardManager.instance.previewPivot.anchoredPosition;
        float speed = CardManager.instance.cardSpeed;
        while (Vector2.Distance(homePos, thisRect.anchoredPosition) > 0.01f)
        {
            MoveTo(Vector2.Lerp(thisRect.anchoredPosition, homePos, Time.deltaTime * speed));
            yield return waitForEnd;
        }
    }

    public void Destroy()
    {
        if (this == null) return;
        if (!gameObject.activeInHierarchy) return;
        destroying = true;
        if (activeJob != null) StopCoroutine(activeJob);
        activeJob = StartCoroutine(DoDestroy());
    }

    IEnumerator DoDestroy()
    {
        CanvasGroup group = gameObject.GetComponent<CanvasGroup>();
        if (!group) group = gameObject.AddComponent<CanvasGroup>();
        while (group.alpha > 0)
        {
            group.alpha -= Time.deltaTime * CardManager.instance.cardFadeSpeed;
            transform.localScale = Vector3.one * Mathf.Lerp(0.75f, 1f, group.alpha);
            yield return waitForEnd;
        }
        Destroy(gameObject);
    }

    public virtual void Done(bool check = false)
    {
        if (this == null) return;
        if (!gameObject.activeInHierarchy) return;
        StartCoroutine(Popup(GameManager.Resource.previewDonePrefab));
        done = true;
    }

    public virtual void Pick()
    {
        if (this == null) return;
        if (!gameObject.activeInHierarchy) return;
        StartCoroutine(Popup(GameManager.Resource.previewPickPrefab));
        picked = true;
    }

    IEnumerator Popup(GameObject prefab)
    {
        Transform target = Instantiate(prefab, transform).transform;
        CanvasGroup group = target.GetComponent<CanvasGroup>();
        while (group.alpha <= 0.999f)
        {
            group.alpha = Mathf.Lerp(group.alpha, 1, Time.deltaTime * CardManager.instance.cardFadeSpeed * 4f);
            target.localScale = Vector3.one * Mathf.Lerp(1.25f, 1f, group.alpha);
            yield return waitForEnd;
        }
        group.alpha = 1;
        target.localScale = Vector3.one;
        //yield return new WaitForSeconds(0.1f);
        while (group.alpha > 0.001f)
        {
            yield return waitForEnd;
            group.alpha = Mathf.Lerp(group.alpha, 0, Time.deltaTime * CardManager.instance.cardFadeSpeed * 2f);
            target.localScale = Vector3.one * Mathf.Lerp(0.75f, 1f, group.alpha);
}
        Destroy(target.gameObject);
    }

    public void MoveTo(Vector2 position)
    {
        thisRect.anchoredPosition = position;
    }

    public void RotateTo(Quaternion rotation)
    {
        transform.rotation = rotation;
    }

    public CardInfo GetInfo()
    {
        return new CardInfo(type, num);
    }

}
