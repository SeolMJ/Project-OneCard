using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SymbolAnimator : MonoBehaviour
{

    [Header("Settings")]
    public float endSize;

    private bool running = false;

    void OnEnable()
    {
        transform.localScale = Vector3.zero;
        if (running) StopAllCoroutines();
        StartCoroutine(StartAnimation());
    }

    IEnumerator StartAnimation()
    {
        running = true;
        while (endSize - transform.localScale.x > 0.001f)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * endSize, Time.deltaTime * 10f);
            yield return new WaitForEndOfFrame();
        }
        transform.localScale = Vector3.one * endSize;
        running = false;
    }

}
