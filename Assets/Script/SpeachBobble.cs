using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

using SeolMJ;

public class SpeachBobble : MonoBehaviour
{

    [Header("UI")]
    public TMP_Text text;
    public new SpriteRenderer renderer;

    [Header("Animation")]
    public AnimationCurve openCurve;
    public AnimationCurve closeCurve;

    [Header("Dialogue")]
    public Conversation conversation;

    private float progress;

    #region StateMachine

    public delegate void State();
    public State state;

    #endregion

    void Start()
    {
        
    }

    public void Init(Conversation conversation)
    {
        this.conversation = conversation;
        text.text = conversation.content;
        transform.position = conversation.speaker.position + (Vector3)conversation.offset;
        progress = 0f;
        state = OpenState;
    }

    /* States */

    void Update()
    {
        state?.Invoke();
        transform.position = Vector3.Lerp(transform.position, conversation.speaker.position + (Vector3)conversation.offset, Time.deltaTime * 30f);
    }

    void OpenState()
    {
        progress += Time.deltaTime;

        float eval = openCurve.Evaluate(progress);
        renderer.color = Utils.OnlyAlpha(renderer.color, eval);
        transform.localScale = Utils.ToVector3(eval, 1f);

        if (progress >= 1f)
        {
            progress = 1f;
            state = null;
        }
    }

    void CloseState()
    {
        progress -= Time.deltaTime;

        float eval = closeCurve.Evaluate(progress);
        renderer.color = Utils.OnlyAlpha(renderer.color, eval);
        transform.localScale = Utils.ToVector3(eval, 1f);

        if (progress <= 0f)
        {
            progress = 0f;
            state = null;
            // Return
        }
    }

    /* States */

}
