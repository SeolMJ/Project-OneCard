using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopAnimator : MonoBehaviour, ITweenable
{

    public AnimationCurve curve;
    public float speed;

    #region Tween
    public Tween.Action TweenAction { get => action; set => action = value; }
    private Tween.Action action;
    private float progress;

    public void OnTween(float progress)
    {
        this.progress = 0f;
    }
    #endregion

    public void Animate()
    {
        gameObject.SetActive(true);
        Tween.Run(this, DoAnimate);
    }

    void DoAnimate()
    {
        if (progress < 1f)
        {
            progress += GameManager.deltaTime * speed;
            float eval = Tween.instance.pa_Pop.Evaluate(progress);
            transform.localScale = new Vector3(eval, eval, 1f);
            return;
        }
        gameObject.SetActive(false);
        action = null;
    }

}
