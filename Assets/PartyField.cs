using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SeolMJ;

public class PartyField : MonoBehaviour
{

    public static PartyField instance;

    public ParticleSystem[] particles;
    public SpriteRenderer symbol;
    public SpriteRenderer background;
    public Transform symbolMask;
    public SpriteRenderer lightRenderer;

    private float alpha;
    private bool party;

    void Awake()
    {
        instance = this;
    }

    void Update()
    {
        lightRenderer.color = Utils.OnlyAlpha(Color.white, Mathf.LerpUnclamped(lightRenderer.color.a, 0f, Time.deltaTime * 2f));
        symbol.color = Utils.OnlyAlpha(symbol.color, Mathf.LerpUnclamped(symbol.color.a, alpha, Time.deltaTime * 5f));
        background.color = Utils.OnlyAlpha(background.color, Mathf.LerpUnclamped(background.color.a, party ? 0.5f : 0f, Time.deltaTime * 5f));
        symbolMask.localScale = Utils.ToVector3(Mathf.LerpUnclamped(symbolMask.localScale.x, party ? 12f : 4f, Time.deltaTime * 5f));
    }

    public void Open(Vector2 position)
    {
        transform.position = position;
        lightRenderer.color = Utils.OnlyAlpha(Color.white, 1f);
        foreach (ParticleSystem particle in particles)
        {
            particle.Play();
        }
        alpha = 0.5f;
        party = false;
    }

    public void Party()
    {
        alpha = 1f;
        party = true;
    }

    public void Close()
    {
        foreach (ParticleSystem particle in particles)
        {
            particle.Stop();
        }
        alpha = 0f;
        party = false;
    }

}
