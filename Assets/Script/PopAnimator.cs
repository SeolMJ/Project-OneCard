using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopAnimator : MonoBehaviour
{

    public AnimationCurve curve;
    public float speed;

    private new Coroutine animation;

    public void Animate()
    {
        gameObject.SetActive(true);
        if (animation != null) StopCoroutine(animation);
        animation = StartCoroutine(DoAnimate());
    }

    IEnumerator DoAnimate()
    {
        float progress = 0f;
        while (progress < 1f)
        {
            transform.localScale = Vector3.one * curve.Evaluate(progress);
            progress += Time.deltaTime * speed;
            yield return null;
        }
        transform.localScale = Vector3.one * curve.Evaluate(1);
        animation = null;
        gameObject.SetActive(false);
    }

}
