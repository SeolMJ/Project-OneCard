using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SymbolAnimator : MonoBehaviour
{

    [Header("Resources")]
    public Image image;

    [Header("Settings")]
    public float endSize = 1f;

    void Reset()
    {
        image = GetComponent<Image>();
    }

    public void Init(float delay, Sprite sprite)
    {
        gameObject.SetActive(true);
        image.sprite = sprite;
        transform.localScale = Vector3.zero;
        StartCoroutine(StartAnimation(delay));
    }

    IEnumerator StartAnimation(float delay)
    {
        yield return new WaitForSeconds(delay);
        while (endSize - transform.localScale.x > 0.001f)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * endSize, GameManager.deltaTime * 10f);
            yield return new WaitForEndOfFrame();
        }
        transform.localScale = Vector3.one * endSize;
    }

}
