using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SeolMJ;

public class PartyField : MonoBehaviour
{

    public static PartyField instance;

    public new SpriteRenderer renderer;

    private float alpha;
    private float scale;

    void Awake()
    {
        instance = this;
    }

    void Update()
    {
        renderer.color = Utils.OnlyAlpha(renderer.color, Mathf.LerpUnclamped(renderer.color.a, alpha, Time.deltaTime * 5f));
        transform.localScale = Utils.ToVector3(Mathf.LerpUnclamped(transform.localScale.x, scale, Time.deltaTime * 5f));
    }

    public void Open()
    {
        alpha = 0.7f;
        scale = 4f;
    }

    public void Party()
    {
        alpha = 1f;
        scale = 12f;
    }

    public void Close()
    {
        alpha = 0f;
        scale = 0f;
    }

}
